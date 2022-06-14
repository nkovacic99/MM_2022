using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/**
 * This script defines properties of a single body (velocity and mass).
 * Movement logic based on forces and acceleration is also defined here.
 * It must be added as a Component to a body when it is Instantiated.
 */
public class PlanetScript : MonoBehaviour
{
    public Vector3 velocity;
    private double mass;
    private Vector3 forceToAdd;

    private Renderer r;

    public void addProperties(Vector3 velocity, double mass)
    {
        this.velocity = velocity;
        this.mass = mass;

        this.r = this.GetComponent<Renderer>();
        r.material.color = new Color(0f, 0f, 255f, 255f);
    }

    /**
     * Applies given force to the Body. Calculates the acceleration caused by the
     * force, adds it to the velocity and moves the object in the Unity scene.
     */
    public void applyForce(float dt, float maxMagnitude, float minMagnitude)
    {
        Vector3 acceleration = forceToAdd / (float) mass;   // Second Newton's Law
        velocity += acceleration * dt;          // Increase velocity based on given time step

        float v = MathF.Min((velocity.sqrMagnitude - minMagnitude)/(maxMagnitude-minMagnitude) * 255f, 255f);
        r.material.color = new Color(v, 0f, 255f-v, 255f);

        float quadTime = (float) Math.Pow(dt, 2) * 0.5f;
        transform.position += velocity * dt + acceleration * quadTime;    // Move body
    }

    public void assignForce(Vector3 force)
    {
        forceToAdd = force;
    }

}
