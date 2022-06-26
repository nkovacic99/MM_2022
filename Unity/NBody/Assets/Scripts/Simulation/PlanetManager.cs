using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public enum ComputeMode
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
    public bool loadFromSimParams = true;
    public TextAsset initialConditionsCsv;
    public float dt = 0.01f;
    public float G = 0.01f;
    public bool enableCollision = false;

    [Header("Simulation Settings")]
    public ComputeShader gravityComputeShader;
    public ComputeMode computeMode = ComputeMode.computeOnCPU;
    public bool exportToVideo = false;
    private string exportFolder;

    [Header("Visual Settings")]
    public bool colorBodies = true;
    public bool addLights = false;
    public float sizeModifier = 1.0f;
    public Color defaultColor = new Color(1, 1, 1);

    // Arrays for storing body objects and their properties
    private GameObject[] bodiesObj;
    private Body[] bodies;
    private ComputeBuffer bodiesBuffer;  // for passing data to GPU

    // Variables used in step skipping computation mode
    // forcesAll[i,j] is the force that the i-th body experiences from the j-th
    private Vector3[,] forcesAll;
    private int stepSkipIndex = 0;
    private const int nStepsSkip = 9;
    private const int numShaderThreads = 100;

    // Variables for coloring bodies
    private Material[] colorMaterials;
    private float maxMagnitude;
    private float minMagnitude;

    private int numberOfIterations = 0;

    void Awake()
    {
        // Set a frame rate cap
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // Load simulation parameters
        if (loadFromSimParams)
        {
            string text = File.ReadAllText(SimulationParameters.initConditionsFilename);
            initialConditionsCsv = new TextAsset(text);
            
            computeMode = (ComputeMode) SimulationParameters.computeModeIx;
            exportToVideo = SimulationParameters.exportToVideo;
            G = SimulationParameters.G;
            dt = SimulationParameters.dt;
            colorBodies = SimulationParameters.colorBodies;
            enableCollision = SimulationParameters.enableCollision;
        }

        // Initialize export
        if (exportToVideo)
        {
            exportFolder = Directory.GetCurrentDirectory() + "/Exports";

            // Delete old screenshots
            DirectoryInfo dir = new DirectoryInfo(exportFolder + "/Screenshots");
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }
        }

        // Read initial conditions and spawn bodies
        PlanetSpawner spawner = new PlanetSpawner();
        string[] initialConds = spawner.ReadCSV(initialConditionsCsv);
        Vector3[] positions;
        double[] masses;
        (bodiesObj, positions, masses) = spawner.PopulateSpace(initialConds, transform);
        spawner.setColor(defaultColor);

        // Add lights (if enabled)
        if (addLights)
            foreach (GameObject bodyObj in bodiesObj)
                bodyObj.AddComponent<Light>();

        // Modify radius of bodies
        foreach (GameObject bodyObj in bodiesObj)
            bodyObj.transform.localScale = sizeModifier * bodyObj.transform.localScale;

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
        if (computeMode == ComputeMode.computeOnCPUWithStepSkipping)
            forcesAll = new Vector3[bodiesObj.Length, bodiesObj.Length];

        // Initialize body colors
        GenerateColoredMaterials();
        maxMagnitude = float.MinValue;
        minMagnitude = float.MaxValue;
    }
    void Update()
    {
        // Capture current frame
        if (exportToVideo)
        {
            string filename = string.Format("{0}/Screenshots/{1:D06}.png", exportFolder, numberOfIterations);
            ScreenCapture.CaptureScreenshot(filename);
        }

        // Return to title screen if Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (exportToVideo) GenerateVideo();
            SceneManager.LoadScene(0);
        }

        // Compute forces in a given mode
        switch (computeMode)
        {
            case ComputeMode.computeOnCPU: ComputeOnCPU(); break;
            case ComputeMode.computeOnCPUWithStepSkipping: ComputeOnCPUWithStepSkipping(); break;
            case ComputeMode.computeOnGPU: ComputeOnGPU(); break;
        }

        // Check for collisions if enabled (naive O(n^2) implementation)
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
                        // Law of momentum conservation
                        double joinedMass = v1.mass + v2.mass;
                        Vector3 velocity = ((float) v1.mass * v1.velocity + (float) v2.mass * v2.velocity) / (float) joinedMass;

                        // Set velocity and radius of joined body
                        v1.velocity = velocity;
                        v1.mass = joinedMass;
                        bodies[i].mass = (float) joinedMass;
                        double r = Math.Pow(joinedMass, 1.0 / 6);
                        bodiesObj[i].transform.localScale = new Vector3((float)r, (float)r, (float)r);

                        // (Effectively) delete the second body
                        bodies[j].mass = 0.0f;
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

            if (colorBodies)
                planetScript.setColor(DetermineColorMaterial(planetScript.velocity.magnitude));
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

            if (colorBodies)
                planetScript.setColor( DetermineColorMaterial(planetScript.velocity.magnitude) );
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

            if (colorBodies)
                bodyScript.setColor( DetermineColorMaterial(bodyScript.velocity.magnitude) );
        }
    }

    void GenerateVideo()
    {
        if (exportToVideo)
        {
            // Make a video out of png files
            string exportName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".mp4";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "ffmpeg";
            p.StartInfo.Arguments = "-framerate 60 -i \"" + exportFolder + "/Screenshots/%06d.png\" \"" + exportFolder + "/" + exportName + "\"";
            p.Start();
            p.WaitForExit();
        }
    }

    void GenerateColoredMaterials()
    {
        colorMaterials = new Material[128];
        for (int colorIx = 0; colorIx < 256 / 2; colorIx++)
        {
            Material material = new Material(Shader.Find("Standard"));
            Color color = new Color(2 * colorIx, 0f, 255f - 2 * colorIx, 255f);
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color);
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
