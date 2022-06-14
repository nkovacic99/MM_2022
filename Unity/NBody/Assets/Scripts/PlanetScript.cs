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
        // Second Newton's Law
        Vector3 acceleration = forceToAdd / (float) mass;

        // Formula for the enakomerno pospešeno gibanje
        float quadTime = dt * dt * 0.5f;
        transform.position += velocity * dt + acceleration * quadTime;

        velocity += acceleration * dt;

        // Color the body based on its velocity
        float colorVal = MathF.Min((velocity.sqrMagnitude - minMagnitude) / (maxMagnitude - minMagnitude) * 255f, 255f);
        r.material.color = new Color(colorVal, 0f, 255f - colorVal, 255f);
    }

    public void assignForce(Vector3 force)
    {
        forceToAdd = force;
    }

}
