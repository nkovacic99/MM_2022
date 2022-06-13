using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Globalization;

public class CsvReader1 : MonoBehaviour
{
    public TextAsset textAssetData;
    public float dt = 0.01f;
    public double G = 0.01f;

    private GameObject[] lightBodies;
    private GameObject heavyBody;

    private int numberOfCsvColumns = 7;

    // forces[i,j] is the force that the i-th body experiences from the j-th
    private Vector3[,] forces;
    private bool[,] cooler;

    private int stepSkipIndex = 0;
    

    // Start is called before the first frame update
    void Awake()
    {
        heavyBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        heavyBody.transform.position.Set(0, 0, 0);
        Vector3 scale = heavyBody.transform.localScale;
        scale.Set(0, 0, 0);
        heavyBody.transform.localScale = scale;

        heavyBody.AddComponent<PlanetScript>();
        heavyBody.GetComponent<PlanetScript>().addProperties(new Vector3(0,0,0), 0);

        string[] data = readCSV();
        populateSpace(data);
    }

    string[] readCSV() {
        string[] data = textAssetData.text.Split(new string[] {",", "\n"}, StringSplitOptions.None);
        return data;
    }
    
    void populateSpace(string[] data) {
        // 5 columns, skipping the first line
        int tableSize = data.Length / numberOfCsvColumns - 1;

        forces = new Vector3[tableSize, tableSize];

        cooler = new bool[tableSize, tableSize];
        

        
        lightBodies = new GameObject[tableSize];
        for (int i = 0; i < tableSize; i++) {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            SphereCollider sphereCollider = body.GetComponent<SphereCollider>();
            if (sphereCollider) Destroy(sphereCollider);

            // Turn off some rendering settings that affect performance
            MeshRenderer meshRenderer = body.GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            
            float positionX = float.Parse(data[numberOfCsvColumns * (i+1)], CultureInfo.InvariantCulture);
            float positionY = float.Parse(data[numberOfCsvColumns * (i+1) + 1], CultureInfo.InvariantCulture);
            float positionZ = float.Parse(data[numberOfCsvColumns * (i+1) + 2], CultureInfo.InvariantCulture);
            Vector3 position = new Vector3(positionX, positionY, positionZ);

            Debug.Log(positionX);
            Debug.Log(positionY);
            Debug.Log(positionZ);


            float velocityX = float.Parse(data[numberOfCsvColumns * (i+1) + 3], CultureInfo.InvariantCulture);
            float velocityY = float.Parse(data[numberOfCsvColumns * (i+1) + 4], CultureInfo.InvariantCulture);
            float velocityZ = float.Parse(data[numberOfCsvColumns * (i+1) + 5], CultureInfo.InvariantCulture);
            // Vector3 velocity = 1000 * Vector3.Cross(position, new Vector3(-position.y, position.x, position.z)).normalized;
            Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);

            Debug.Log(velocityX);
            Debug.Log(velocityY);
            Debug.Log(velocityZ);


            
            double mass = double.Parse(data[numberOfCsvColumns * (i+1) + 6], CultureInfo.InvariantCulture);

            Debug.Log(mass);
            

            body.AddComponent<PlanetScript>();
            body.GetComponent<PlanetScript>().addProperties(velocity, mass);
            body.transform.position = position;

            lightBodies[i] = body;
            
        }
    }

    void FixedUpdate()
    {   


        //partial step skipping
        // The first calculation in the nested for loop gets reevaluated only
        // every n-th frame
        int n = 5;
        stepSkipIndex++;
        int quantity = lightBodies.Length / n;
        if (stepSkipIndex == n) {
            stepSkipIndex = 0;
        }
        int start = quantity * (stepSkipIndex);
        int end = quantity * (stepSkipIndex + 1);
        if (stepSkipIndex == (n-1)) {
            end = lightBodies.Length;
        }




        // the calculation of forces.
        // G is provided on the top of the class.

        for (int i = start; i < end; i++)
        {   
            PlanetScript bodyScript1 = lightBodies[i].GetComponent<PlanetScript>();
            Vector3 position1 = bodyScript1.transform.position;
            double mass1 = bodyScript1.mass;

            for (int j = i+1; j < lightBodies.Length; j++)
            {   

                PlanetScript bodyScript2 = lightBodies[j].GetComponent<PlanetScript>();
                Vector3 distanceVector = bodyScript2.transform.position - position1;

                double mass2 = bodyScript2.mass;

                float scalar = (float) ( G * mass1 * mass2 / (Math.Pow(distanceVector.magnitude, 3)) );
                Vector3 force = scalar * distanceVector;



                forces[i, j] = force;
                forces[j, i] = -force;


            
            }
        }

        
        

        // Force summation and assignment
        for (int i = start; i < end; i++)
        {   
            Vector3 summedForce = new Vector3(0, 0, 0);

            for (int j = 0; j < lightBodies.Length; j++)
            {   
                if(i == j)
                {
                    continue;
                }

                summedForce += forces[i, j];
            
            }

            PlanetScript bodyScript = lightBodies[i].GetComponent<PlanetScript>();
            bodyScript.assignForce(summedForce);

        }




        // Apply calculated forces
        for (int i = 0; i < lightBodies.Length; i++)
        {
            PlanetScript bodyScript = lightBodies[i].GetComponent<PlanetScript>();
            bodyScript.applyForce(dt);
        }
    }
}
