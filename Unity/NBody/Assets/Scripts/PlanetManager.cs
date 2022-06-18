using UnityEngine;
using System;
using System.IO;

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

    public Boolean enableCollision = false;

    [Header("Simulation Settings")]
    public ComputeShader gravityComputeShader;
    public SimulationMode simulationMode = SimulationMode.computeOnCPU;
    public Boolean exportToVideo = false;
    public Vector2Int exportResolution = new Vector2Int(1280, 720);
    private string folder = "Exports";

    [Header("Visual Settings")]
    public Boolean colorBodies = true;

    // Arrays for storing body objects and their properties
    private GameObject[] bodiesObj;
    private Body[] bodies;
    private ComputeBuffer bodiesBuffer;

    // forces[i,j] is the force that the i-th body experiences from the j-th
    private Vector3[,] forcesAll;
    private int stepSkipIndex = 0;
    private const int nStepsSkip = 9;
    private const int numShaderThreads = 100;

    private Material[] colorMaterials;
    private float maxMagnitude;
    private float minMagnitude;
    private int numberOfIterations = 0;

    void Awake()
    {
        if (exportToVideo)
        {
            Time.captureFramerate = 30;
            Screen.SetResolution(exportResolution.x, exportResolution.y, false);
        }

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

        // Initialize body colors
        GenerateColoredMaterials();
        maxMagnitude = float.MinValue;
        minMagnitude = float.MaxValue;
    }
    void Update()
    {
        if (!exportToVideo)
            return;

        string filename = string.Format("{0}/Screenshots/{1:D06}.png", folder, Time.frameCount);
        ScreenCapture.CaptureScreenshot(filename);

        switch (simulationMode)
        {
            case SimulationMode.computeOnCPU: ComputeOnCPU(); break;
            case SimulationMode.computeOnCPUWithStepSkipping: ComputeOnCPUWithStepSkipping(); break;
            case SimulationMode.computeOnGPU: ComputeOnGPU(); break;
        }

        if (enableCollision)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                PlanetScript v1 = bodiesObj[i].GetComponent<PlanetScript>();
                if (v1.mass == 0.0d) { continue; }
                for (int j = i + 1; j < bodies.Length; j++)
                {
                    PlanetScript v2 = bodiesObj[j].GetComponent<PlanetScript>();
                    if (v2.mass == 0.0d) { continue; }
                    if (Vector3.Distance(bodies[i].position, bodies[j].position) < System.Math.Min(System.Math.Pow(v1.mass, (double)1 / 3), System.Math.Pow(v2.mass, (double)1 / 3)))
                    {
                        double joinedMass = v1.mass + v2.mass;

                        float velocityX = ((float)v1.mass * v1.velocity[0] + (float)v2.mass * v2.velocity[0]) / (float)joinedMass;
                        float velocityY = ((float)v1.mass * v1.velocity[1] + (float)v2.mass * v2.velocity[1]) / (float)joinedMass;
                        float velocityZ = ((float)v1.mass * v1.velocity[2] + (float)v2.mass * v2.velocity[2]) / (float)joinedMass;

                        Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);

                        v1.velocity = velocity;
                        bodies[i].mass = (float)joinedMass;
                        v1.mass = joinedMass;
                        double r = System.Math.Pow(joinedMass, (double)1 / 6);
                        bodiesObj[i].transform.localScale = new Vector3((float)r, (float)r, (float)r);
                        bodies[j].mass = 0.0f;
                        bodiesObj[j].transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                        v2.mass = 0.0d;
                        v2.rend.enabled = false;

                    }
                }
            }
        }

        // Update velocity magnitudes used for coloring every 50 iterations
        if (colorBodies && numberOfIterations % 50 == 0)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                PlanetScript bodyScript = bodiesObj[i].GetComponent<PlanetScript>();

                if (bodyScript.velocity.magnitude > maxMagnitude) { maxMagnitude = bodyScript.velocity.magnitude; }
                if (bodyScript.velocity.magnitude < minMagnitude) { minMagnitude = bodyScript.velocity.magnitude; }
            }
        }
        numberOfIterations++;
    }

    void FixedUpdate()
    {
        if (exportToVideo)
            return;

        switch (simulationMode)
        {
            case SimulationMode.computeOnCPU: ComputeOnCPU(); break;
            case SimulationMode.computeOnCPUWithStepSkipping: ComputeOnCPUWithStepSkipping(); break;
            case SimulationMode.computeOnGPU: ComputeOnGPU(); break;
        }

        if (enableCollision)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                PlanetScript v1 = bodiesObj[i].GetComponent<PlanetScript>();
                if (v1.mass == 0.0d) { continue; }
                for (int j = i+1; j < bodies.Length; j++)
                {
                    PlanetScript v2 = bodiesObj[j].GetComponent<PlanetScript>();
                    if (v2.mass == 0.0d) { continue; }
                    if (Vector3.Distance(bodies[i].position, bodies[j].position) < System.Math.Min(System.Math.Pow(v1.mass, (double) 1/3), System.Math.Pow(v2.mass, (double) 1/3))) {
                        double joinedMass = v1.mass + v2.mass;

                        float velocityX = ((float)v1.mass * v1.velocity[0] + (float)v2.mass * v2.velocity[0]) / (float)joinedMass;
                        float velocityY = ((float)v1.mass * v1.velocity[1] + (float)v2.mass * v2.velocity[1]) / (float)joinedMass;
                        float velocityZ = ((float)v1.mass * v1.velocity[2] + (float)v2.mass * v2.velocity[2]) / (float)joinedMass;

                        Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);

                        v1.velocity = velocity;
                        bodies[i].mass = (float) joinedMass;
                        v1.mass = joinedMass;
                        double r = System.Math.Pow(joinedMass, (double) 1/6);
                        bodiesObj[i].transform.localScale = new Vector3((float) r, (float) r, (float) r);
                        bodies[j].mass = 0.0f;
                        bodiesObj[j].transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
                        v2.mass = 0.0d;
                        v2.rend.enabled = false;

                    }
                }
            }
        }
        // Update velocity magnitudes used for coloring every 50 iterations
        if (colorBodies && numberOfIterations % 50 == 0)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                PlanetScript bodyScript = bodiesObj[i].GetComponent<PlanetScript>();

                if (bodyScript.velocity.magnitude > maxMagnitude) { maxMagnitude = bodyScript.velocity.magnitude; }
                if (bodyScript.velocity.magnitude < minMagnitude) { minMagnitude = bodyScript.velocity.magnitude; }
            }
        }
        numberOfIterations++;
    }

    void OnDestroy()
    {
        if (exportToVideo)
        {
            // Make a video out of png files
            string exportName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".mp4";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "ffmpeg";
            p.StartInfo.Arguments = "-framerate 30 -i " + folder + "/Screenshots/%06d.png " + folder + "/" + exportName;
            p.Start();
            p.WaitForExit();

            // Delete screenshots
            DirectoryInfo di = new DirectoryInfo(folder + "/Screenshots");
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
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

            if (planetScript.mass != 0.0d)
            { 
                planetScript.assignForce(bodies[i].force);
                planetScript.applyForce(dt, maxMagnitude, minMagnitude);
                bodies[i].position = bodiesObj[i].transform.position;
            }
            if (colorBodies)
                planetScript.setColor( DetermineColorMaterial(planetScript.velocity.sqrMagnitude) );
            planetScript.assignForce(bodies[i].force);
            planetScript.applyForce(dt, maxMagnitude, minMagnitude);
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
            planetScript.applyForce(dt, maxMagnitude, minMagnitude);
            bodies[i].position = bodiesObj[i].transform.position;

            if (colorBodies)
                planetScript.setColor( DetermineColorMaterial(planetScript.velocity.sqrMagnitude) );
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
            bodyScript.applyForce(dt, maxMagnitude, minMagnitude);
            bodies[i].position = bodiesObj[i].transform.position;

            if (colorBodies)
                bodyScript.setColor( DetermineColorMaterial(bodyScript.velocity.sqrMagnitude) );
        }
    }

    void GenerateColoredMaterials()
    {
        colorMaterials = new Material[128];
        for (int colorIx = 0; colorIx < 256 / 2; colorIx++)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(2 * colorIx, 0f, 255f - 2 * colorIx, 255f);
            material.enableInstancing = true;
            colorMaterials[colorIx] = material;
        }
    }

    public Material DetermineColorMaterial(float magnitude)
    {
        float colorVal = (magnitude - minMagnitude) / (maxMagnitude - minMagnitude) * 255f;
        int colorIx = (int) (colorVal / 2);
        if (colorIx < 0) colorIx = 0;
        if (colorIx >= colorMaterials.Length) colorIx = colorMaterials.Length - 1;
        return colorMaterials[colorIx];
    }
}
