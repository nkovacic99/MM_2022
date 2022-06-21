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
    public double mass;
    private Vector3 forceToAdd;

    public MeshRenderer rend;

    public void addProperties(Vector3 velocity, double mass)
    {
        this.velocity = velocity;
        this.mass = mass;

        rend = GetComponent<MeshRenderer>();
    }

    /**
     * Applies given force to the Body. Calculates the acceleration caused by the
     * force, adds it to the velocity and moves the object in the Unity scene.
     */
    public void applyForce(float dt, float maxMagnitude, float minMagnitude)
    {
        if (this.mass == 0.0d) { return; }
        // Second Newton's Law
        Vector3 acceleration = forceToAdd / (float) mass;

        velocity += acceleration * dt;
        transform.position += velocity * dt;
    }

    public void setColor(Material colorMaterial)
    {
        rend.sharedMaterial = colorMaterial;
    }

    public void assignForce(Vector3 force)
    {
        forceToAdd = force;
    }

}
