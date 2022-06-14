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
    private Vector3 velocity;
    private double mass;
    private Vector3 forceToAdd;

    public void addProperties(Vector3 velocity, double mass)
    {
        this.velocity = velocity;
        this.mass = mass;
    }

    /**
     * Applies given force to the Body. Calculates the acceleration caused by the
     * force, adds it to the velocity and moves the object in the Unity scene.
     */
    public void applyForce(float dt)
    {
        Vector3 acceleration = forceToAdd / (float) mass;   // Second Newton's Law
        velocity += acceleration * dt;          // Increase velocity based on given time step

        float quadTime = (float) Math.Pow(dt, 2) * 0.5f;
        transform.position += velocity * dt + acceleration * quadTime;    // Move body
    }

    public void assignForce(Vector3 force)
    {
        forceToAdd = force;
    }

}
