using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Globalization;

public class PlanetSpawner
{
    private const int bodyResolution = 2;
    private const int numberOfCsvColumns = 7;

    private Mesh bodyMesh;
    private Material bodyMaterial;

    public PlanetSpawner()
    {
        // Create shared Material and Mesh
        GenerateMesh();
        bodyMaterial = new Material(Shader.Find("Standard"));
        bodyMaterial.enableInstancing = true;
    }

    public (GameObject[], Vector3[], double[]) PopulateSpace(string[] data)
    {
        // Create an empty array of bodies
        int numberOfBodies = data.Length / numberOfCsvColumns - 1;
        GameObject[] bodies = new GameObject[numberOfBodies];
        Vector3[] initialPositions = new Vector3[numberOfBodies];
        Vector3[] initialVelocities = new Vector3[numberOfBodies];
        double[] initialMasses = new double[numberOfBodies];

        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        for (int i = 0; i < numberOfBodies; i++)
        {
            // Create an empty GameObject
            GameObject body = new GameObject("Planet");

            // Add a Mesh to the body
            body.AddComponent<MeshFilter>().sharedMesh = bodyMesh;
            MeshRenderer meshRenderer = body.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = bodyMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            // Parse position
            float positionX = float.Parse(data[numberOfCsvColumns * (i + 1)]);
            float positionY = float.Parse(data[numberOfCsvColumns * (i + 1) + 1]);
            float positionZ = float.Parse(data[numberOfCsvColumns * (i + 1) + 2]);
            Vector3 position = new Vector3(positionX, positionY, positionZ);

            // Parse velocity
            float velocityX = float.Parse(data[numberOfCsvColumns * (i + 1) + 3]);
            float velocityY = float.Parse(data[numberOfCsvColumns * (i + 1) + 4]);
            float velocityZ = float.Parse(data[numberOfCsvColumns * (i + 1) + 5]);
            Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);

            // Parse mass
            double mass = double.Parse(data[numberOfCsvColumns * (i + 1) + 6]);

            // Add properties to the body
            body.AddComponent<PlanetScript>();
            body.GetComponent<PlanetScript>().addProperties(velocity, mass);
            body.transform.position = position;

            initialPositions[i] = position;
            initialVelocities[i] = velocity;
            initialMasses[i] = mass;
            bodies[i] = body;
        }

        return (bodies, initialPositions, initialMasses);
    }

    public string[] ReadCSV(TextAsset initialConditionsCsv)
    {
        string[] data = initialConditionsCsv.text.Split(new string[] { ",", "\n" }, System.StringSplitOptions.None);
        return data;
    }

    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>(new Vector3[]
        {
            new Vector3(0, 1, 0), new Vector3(0, -1, 0), new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0), new Vector3(0, 0, -1), new Vector3(0, 0, 1)
        });

        List<int> triangles = new List<int>(new int[]
        {
            0, 2, 4,
            0, 4, 3,
            0, 3, 5,
            0, 5, 2,
            1, 4, 2,
            1, 3, 4,
            1, 5, 3,
            1, 2, 5
        });

        for (int i = 0; i < bodyResolution - 1; i++)
        {
            int trianglesCount = triangles.Count;

            for (int tri = 0; tri < trianglesCount; tri += 3)
            {
                int v0_ix = triangles[tri];
                int v1_ix = triangles[tri + 1];
                int v2_ix = triangles[tri + 2];
                int v3_ix = -1;
                int v4_ix = -1;
                int v5_ix = -1;

                Vector3 v0 = vertices[v0_ix];
                Vector3 v1 = vertices[v1_ix];
                Vector3 v2 = vertices[v2_ix];
                Vector3 v3 = (0.5f * (v0 + v1)).normalized;
                Vector3 v4 = (0.5f * (v1 + v2)).normalized;
                Vector3 v5 = (0.5f * (v2 + v0)).normalized;

                for (int j = 0; j < vertices.Count; j++)
                {
                    if ((vertices[j] - v3).magnitude < 1e-9f) v3_ix = j;
                    if ((vertices[j] - v4).magnitude < 1e-9f) v4_ix = j;
                    if ((vertices[j] - v5).magnitude < 1e-9f) v5_ix = j;
                }

                if (v3_ix == -1) { vertices.Add(v3); v3_ix = vertices.Count - 1; }
                if (v4_ix == -1) { vertices.Add(v4); v4_ix = vertices.Count - 1; }
                if (v5_ix == -1) { vertices.Add(v5); v5_ix = vertices.Count - 1; }

                triangles.AddRange(new int[]
                {
                    v0_ix, v3_ix, v5_ix,
                    v3_ix, v1_ix, v4_ix,
                    v4_ix, v2_ix, v5_ix,
                    v3_ix, v4_ix, v5_ix
                });
            }

            triangles.RemoveRange(0, trianglesCount);
        }

        bodyMesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
    }

}
