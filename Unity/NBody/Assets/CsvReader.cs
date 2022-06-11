using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CsvReader : MonoBehaviour
{
    public TextAsset textAssetData;
    public float dt = 0.00001f;

    private GameObject[] lightBodies;
    private GameObject heavyBody;

    private int numberOfCsvColumns = 7;

    // Start is called before the first frame update
    void Awake()
    {
        heavyBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        heavyBody.transform.position.Set(0, 0, 0);
        Vector3 scale = heavyBody.transform.localScale;
        scale.Set(10, 10, 10);
        heavyBody.transform.localScale = scale;

        heavyBody.AddComponent<PlanetScript>();
        heavyBody.GetComponent<PlanetScript>().addProperties(new Vector3(0,0,0), 100000);

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
            
            float positionX = float.Parse(data[numberOfCsvColumns * (i+1)]);
            float positionY = float.Parse(data[numberOfCsvColumns * (i+1) + 1]);
            float positionZ = float.Parse(data[numberOfCsvColumns * (i+1) + 2]);
            Vector3 position = new Vector3(positionX, positionY, positionZ);

            float velocityX = float.Parse(data[numberOfCsvColumns * (i+1) + 3]);
            float velocityY = float.Parse(data[numberOfCsvColumns * (i+1) + 4]);
            float velocityZ = float.Parse(data[numberOfCsvColumns * (i+1) + 5]);
            // Vector3 velocity = 1000 * Vector3.Cross(position, new Vector3(-position.y, position.x, position.z)).normalized;
            Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);
            
            double mass = double.Parse(data[numberOfCsvColumns * (i+1) + 6]);

            body.AddComponent<PlanetScript>();
            body.GetComponent<PlanetScript>().addProperties(velocity, mass);
            body.transform.position = position;

            lightBodies[i] = body;
            
        }
    }

    void FixedUpdate()
    {
        // Calculate forces for all bodies
        for (int i = 0; i < lightBodies.Length; i++)
        {
            PlanetScript bodyScript = lightBodies[i].GetComponent<PlanetScript>();
            bodyScript.resetForce();
            bodyScript.addForce(heavyBody);
        }

        // Apply calculated forces
        for (int i = 0; i < lightBodies.Length; i++)
        {
            PlanetScript bodyScript = lightBodies[i].GetComponent<PlanetScript>();
            bodyScript.applyForce(dt);
        }
    }
}
