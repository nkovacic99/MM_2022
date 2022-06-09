using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    public int numberOfLightBodies = 500;
    public float dt = 0.00001f;

    private GameObject[] lightBodies;  // Array of bodies
    private GameObject heavyBody;
    
    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        heavyBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        heavyBody.transform.position.Set(0, 0, 0);
        Vector3 scale = heavyBody.transform.localScale;
        scale.Set(10, 10, 10);
        heavyBody.transform.localScale = scale;

        heavyBody.AddComponent<PlanetScript>();
        heavyBody.GetComponent<PlanetScript>().addProperties(new Vector3(0,0,0), 100000);

        lightBodies = new GameObject[numberOfLightBodies];

        for (int i = 0; i < numberOfLightBodies; i++)
        {
            // Create a sphere
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            // Remove the sphere's collider (as we will define our own collision logic later)
            SphereCollider sphereCollider = body.GetComponent<SphereCollider>();
            if (sphereCollider) Destroy(sphereCollider);

            // Turn off some rendering settings that affect performance
            MeshRenderer meshRenderer = body.GetComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            // Determine initial conditions (position, velocity and mass) randomly
            Vector3 position = new Vector3( Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100) );
            // Velocity here is calculated so that it is perpendiculat to the radius vector to heavy body
            Vector3 velocity = 1000 * Vector3.Cross(position, new Vector3(-position.y, position.x, position.z)).normalized;
            double mass = Random.Range(800, 1000);

            // Add properties to sphere
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
