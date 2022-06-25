using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private float speed = 1.0f;
    private float sensitivity = 2.0f;

    private void Start()
    {
        Cursor.visible = false;
    }
    
    void Update()
    {
        // Move faster if left shild is pressed
        if (Input.GetKey(KeyCode.LeftShift)) speed = 10.0f;
        else speed = 1.0f;

        // Handle movement
        if (Input.GetKey(KeyCode.W)) transform.position += speed * transform.forward;
        if (Input.GetKey(KeyCode.S)) transform.position -= speed * transform.forward;
        if (Input.GetKey(KeyCode.D)) transform.position += speed * transform.right;
        if (Input.GetKey(KeyCode.A)) transform.position -= speed * transform.right;
        if (Input.GetKey(KeyCode.E)) transform.position += speed * transform.up;
        if (Input.GetKey(KeyCode.Q)) transform.position -= speed * transform.up;

        // Handle rotation
        float axisX = Input.GetAxis("Mouse X");
        float axisY = Input.GetAxis("Mouse Y");
        transform.Rotate(0, axisX * sensitivity, 0);
        transform.Rotate(-axisY * sensitivity, 0, 0);

    }
}
