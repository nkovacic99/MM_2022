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

    private MeshRenderer rend;

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
        // Second Newton's Law
        Vector3 acceleration = forceToAdd / (float) mass;

        // Formula for the enakomerno pospešeno gibanje
        float quadTime = dt * dt * 0.5f;
        transform.position += velocity * dt + acceleration * quadTime;

        velocity += acceleration * dt;  
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
