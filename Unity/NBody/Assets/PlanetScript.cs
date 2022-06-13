using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This script defines properties of a single body (velocity and mass).
 * Movement logic based on forces and acceleration is also defined here.
 * It must be added as a Component to a body when it is Instantiated.
 */
public class PlanetScript : MonoBehaviour
{
    public Vector3 velocity;
    public double mass;

    private Vector3 forceToAdd;  // Force to apply when moving the body

    public void addProperties(Vector3 velocity, double mass)
    {
        this.velocity = velocity;
        this.mass = mass;
    }

    /**
     * Applies force that is saved in forceToAdd variable. Calculates the
     * acceleration caused by the force, adds it to the velocity and moves
     * the object in the Unity scene.
     */
    public void applyForce(float dt)
    {
        Vector3 acceleration = forceToAdd / (float) mass;   // Second Newton's Law
        velocity += acceleration * dt;          // Increase velocity based on given time step
        transform.position += velocity * dt;    // Move body
    }

    public void resetForce()
    {
        forceToAdd = new Vector3(0, 0, 0);
    }







    //Assigngs the precalculated force vector to rhe object.
    public void assignForce(Vector3 force)
    {
        forceToAdd = force;
    }


    /**
     * Adds the gravitational force caused by another body to this body.
     * Does not apply the force yet (and does not move this body), as force
     * of many other bodies can be added before applying it.
     */
    public void addForce(GameObject other)
    {
        // Put variables into Newton's gravitational equation
        double G = 1;
        Vector3 forceTmp = other.transform.position - this.transform.position;
        forceTmp = forceTmp / forceTmp.magnitude;
        double massOther = other.GetComponent<PlanetScript>().mass;
        forceTmp = (float) (G * mass * massOther) * forceTmp;
        forceToAdd += forceTmp;
    }

}
