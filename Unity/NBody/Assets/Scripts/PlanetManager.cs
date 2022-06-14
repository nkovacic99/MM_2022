using UnityEngine;
using System;


public enum SimulationMode
{
    computeOnCPU,
    computeOnCPUWithStepSkipping,
    computeOnGPU
}

public struct Body
{
    public Vector3 position;
    public float mass;
    public Vector3 force;
}

public class PlanetManager : MonoBehaviour
{
    [Header("Simulation Data and Parameters")]
    public TextAsset initialConditionsCsv;
    public float dt = 0.01f;
    public float G = 0.01f;

    [Header("Simulation Settings")]
    public ComputeShader gravityComputeShader;
    public SimulationMode simulationMode = SimulationMode.computeOnCPU;

    // Arrays for storing body objects and their properties
    private GameObject[] bodiesObj;
    private Body[] bodies;
    private ComputeBuffer bodiesBuffer;

    // forces[i,j] is the force that the i-th body experiences from the j-th
    private Vector3[,] forcesAll;
    private int stepSkipIndex = 0;
    private const int nStepsSkip = 9;

    private const int numShaderThreads = 100;
    
    void Awake()
    {
        // Read initial conditions and spawn bodies
        PlanetSpawner spawner = new PlanetSpawner();
        string[] initialConds = spawner.ReadCSV(initialConditionsCsv);
        Vector3[] positions;
        double[] masses;
        (bodiesObj, positions, masses) = spawner.PopulateSpace(initialConds);

        // Save positions and masses into the array
        bodies = new Body[bodiesObj.Length];
        for (int i = 0; i < bodiesObj.Length; i++)
        {
            Body body = new Body();
            body.position = positions[i];
            body.mass = (float) masses[i];
            body.force = new Vector3(0, 0, 0);
            bodies[i] = body;
        }

        // Only allocate memory for all forces if the selected mode is 'Step Skipping'
        if (simulationMode == SimulationMode.computeOnCPUWithStepSkipping)
            forcesAll = new Vector3[bodiesObj.Length, bodiesObj.Length];
    }

    void FixedUpdate()
    {
        switch (simulationMode)
        {
            case SimulationMode.computeOnCPU: ComputeOnCPU(); break;
            case SimulationMode.computeOnCPUWithStepSkipping: ComputeOnCPUWithStepSkipping(); break;
            case SimulationMode.computeOnGPU: ComputeOnGPU(); break;
        }
    }

    void ComputeOnGPU()
    {
        // Determine size of struct that holds Body data
        int vector3size = sizeof(float) * 3;
        int floatSize = sizeof(float);
        int totalSize = 2 * vector3size + floatSize;

        // Create compute buffer for passing data to GPU
        bodiesBuffer = new ComputeBuffer(bodies.Length, totalSize);
        bodiesBuffer.SetData(bodies);

        // Pass data to Compute Shader
        gravityComputeShader.SetBuffer(0, "bodies", bodiesBuffer);
        gravityComputeShader.SetFloat("G", G);
        gravityComputeShader.SetInt("numBodies", bodies.Length);
        gravityComputeShader.Dispatch(0, bodiesObj.Length / numShaderThreads, 1, 1);

        // Read results
        bodiesBuffer.GetData(bodies);
        bodiesBuffer.Dispose();

        // Apply forces to Body Objects
        for (int i = 0; i < bodiesObj.Length; i++)
        {
            PlanetScript planetScript = bodiesObj[i].GetComponent<PlanetScript>();
            planetScript.assignForce(bodies[i].force);
            planetScript.applyForce(dt);
            bodies[i].position = bodiesObj[i].transform.position;
        }
    }

    void ComputeOnCPU()
    {
        // Reset forces
        for (int i = 0; i < bodies.Length; i++)
            bodies[i].force = new Vector3(0, 0, 0);

        // Compute gravitational forces using Newton's gravity equation
        for (int i = 0; i < bodiesObj.Length; i++)
        {
            Vector3 position1 = bodies[i].position;
            double mass1 = bodies[i].mass;

            for (int j = i + 1; j < bodiesObj.Length; j++)
            {
                Vector3 distVector = bodies[j].position - position1;
                float scalar = (float) (G * mass1 * bodies[j].mass / (MathF.Pow(distVector.magnitude, 3)));
                Vector3 force = scalar * distVector;

                bodies[i].force += force;
                bodies[j].force -= force;
            }
        }

        // Apply forces to Body Objects
        for (int i = 0; i < bodiesObj.Length; i++)
        {
            PlanetScript planetScript = bodiesObj[i].GetComponent<PlanetScript>();
            planetScript.assignForce(bodies[i].force);
            planetScript.applyForce(dt);
            bodies[i].position = bodiesObj[i].transform.position;
        }
    }

    void ComputeOnCPUWithStepSkipping()
    {
        // Partial step skipping
        //  The first calculation in the nested for loop gets reevaluated only
        //  every n-th frame
        stepSkipIndex++;
        int quantity = bodiesObj.Length / nStepsSkip;
        if (stepSkipIndex == nStepsSkip)
        {
            stepSkipIndex = 0;
        }
        int start = quantity * (stepSkipIndex);
        int end = quantity * (stepSkipIndex + 1);
        if (stepSkipIndex == (nStepsSkip - 1))
        {
            end = bodiesObj.Length;
        }

        // Calculate all pairs of forces
        for (int i = start; i < end; i++)
        {
            Vector3 position1 = bodies[i].position;
            double mass1 = bodies[i].mass;

            for (int j = i + 1; j < bodiesObj.Length; j++)
            {
                Vector3 distanceVector = bodies[j].position - position1;
                double mass2 = bodies[j].mass;

                float scalar = (float)(G * mass1 * mass2 / (Math.Pow(distanceVector.magnitude, 3)));
                Vector3 force = scalar * distanceVector;

                forcesAll[i, j] = force;
                forcesAll[j, i] = -force;
            }
        }

        // Force summation and assignment
        for (int i = start; i < end; i++)
        {
            Vector3 summedForce = new Vector3(0, 0, 0);

            for (int j = 0; j < bodiesObj.Length; j++)
            {
                if (i != j)
                    summedForce += forcesAll[i, j];
            }

            PlanetScript bodyScript = bodiesObj[i].GetComponent<PlanetScript>();
            bodyScript.assignForce(summedForce);
        }

        // Apply calculated forces
        for (int i = 0; i < bodiesObj.Length; i++)
        {
            PlanetScript bodyScript = bodiesObj[i].GetComponent<PlanetScript>();
            bodyScript.applyForce(dt);
            bodies[i].position = bodiesObj[i].transform.position;
        }
    }
}
