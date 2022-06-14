using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Globalization;

public class PlanetManager : MonoBehaviour
{
    public TextAsset initialConditionsCsv;
    public float dt = 0.01f;
    public float G = 0.01f;

    public Boolean usePartialStepSkipping = false;

    private GameObject[] bodies;

    // forces[i,j] is the force that the i-th body experiences from the j-th
    private Vector3[,] forces2d;
    private Vector3[] forces;
    private Vector3[] positions;
    private double[] masses;

    private int stepSkipIndex = 0;
    
    private float maxMagnitude;
    private float minMagnitude;
    private int numberOfIterations;
    void Awake()
    {
        PlanetSpawner spawner = new PlanetSpawner();
        string[] initialConds = spawner.ReadCSV(initialConditionsCsv);
        (bodies, positions, masses) = spawner.PopulateSpace(initialConds);
        if (usePartialStepSkipping)
            forces2d = new Vector3[bodies.Length, bodies.Length];
        else
            forces = new Vector3[bodies.Length];
        maxMagnitude = float.MinValue;
        minMagnitude = float.MaxValue;
    }

    void FixedUpdate()
    {
        if (usePartialStepSkipping)
            PartialStepSkipping();
        else
            NoStepSkipping();
            
        if (numberOfIterations % 50 == 0 )
        {
            for (int i = 0; i < bodies.Length; i++)
            {
            PlanetScript bodyScript = bodies[i].GetComponent<PlanetScript>();

            if (bodyScript.velocity.magnitude > maxMagnitude) { maxMagnitude = bodyScript.velocity.magnitude; }
            if (bodyScript.velocity.magnitude < minMagnitude) { minMagnitude = bodyScript.velocity.magnitude; }
            }
        }
        numberOfIterations ++;
    }

    void NoStepSkipping()
    {
        forces = new Vector3[bodies.Length];
        for (int i = 0; i < bodies.Length; i++)
            forces[i] = new Vector3(0, 0, 0);

        for (int i = 0; i < bodies.Length; i++)
        {
            Vector3 position1 = positions[i];
            double mass1 = masses[i];

            for (int j = i + 1; j < bodies.Length; j++)
            {
                Vector3 distVector = positions[j] - position1;
                float scalar = (float) (G * mass1 * masses[j] / (MathF.Pow(distVector.magnitude, 3)));
                Vector3 force = scalar * distVector;

                forces[i] += force;
                forces[j] -= force;
            }
        }

        for (int i = 0; i < bodies.Length; i++)
        {
            PlanetScript planetScript = bodies[i].GetComponent<PlanetScript>();
            planetScript.assignForce(forces[i]);
            planetScript.applyForce(dt, maxMagnitude, minMagnitude);
            positions[i] = bodies[i].transform.position;
        }        
    }

    void PartialStepSkipping()
    {
        //partial step skipping
        // The first calculation in the nested for loop gets reevaluated only
        // every n-th frame
        int n = 9;
        stepSkipIndex++;
        int quantity = bodies.Length / n;
        if (stepSkipIndex == n)
        {
            stepSkipIndex = 0;
        }
        int start = quantity * (stepSkipIndex);
        int end = quantity * (stepSkipIndex + 1);
        if (stepSkipIndex == (n - 1))
        {
            end = bodies.Length;
        }

        // the calculation of forces.
        // G is provided on the top of the class.

        for (int i = start; i < end; i++)
        {
            Vector3 position1 = positions[i];
            double mass1 = masses[i];

            for (int j = i + 1; j < bodies.Length; j++)
            {
                Vector3 distanceVector = positions[j] - position1;

                double mass2 = masses[j];

                float scalar = (float)(G * mass1 * mass2 / (Math.Pow(distanceVector.magnitude, 3)));
                Vector3 force = scalar * distanceVector;

                forces2d[i, j] = force;
                forces2d[j, i] = -force;
            }
        }

        // Force summation and assignment
        for (int i = start; i < end; i++)
        {
            Vector3 summedForce = new Vector3(0, 0, 0);

            for (int j = 0; j < bodies.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                summedForce += forces2d[i, j];

            }

            PlanetScript bodyScript = bodies[i].GetComponent<PlanetScript>();
            bodyScript.assignForce(summedForce);
        }




        // Apply calculated forces
        for (int i = 0; i < bodies.Length; i++)
        {
            PlanetScript bodyScript = bodies[i].GetComponent<PlanetScript>();
            bodyScript.applyForce(dt, maxMagnitude, minMagnitude);
            positions[i] = bodies[i].transform.position;
        }
    }
}
