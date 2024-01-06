using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float initialSpeed; //not velocity cuz scalar xD :D

    float initialAngle;

    Vector3 movementVector;
    Vector3 movementDirection;
    Vector3 destination;

    float startTime;
    float incidentTime;

    float initialVerticalOffset;

    bool allSet = false;
    public void InitBulletForShooting(float speed, float thetao_rad,Vector3 groundPosition,Vector3 target)
    {
        transform.forward = (target-groundPosition).normalized;
        movementDirection = transform.forward;
        movementVector = transform.forward;
        initialSpeed = speed;
        initialAngle = thetao_rad;
        destination = target;
        initialVerticalOffset = (this.transform.position - groundPosition).y;
        startTime = Time.time;
        allSet = true;
    }

    //x = Vo* cos(THETAo) * t
    //y = Vo sin(THETAo)*t - (0.5 * g* t^2) 
    Vector3 horizontalComponent;

    private void FixedUpdate()
    {
        if (!allSet) return;

        incidentTime = Time.time - startTime;

        movementVector.z = initialSpeed * Mathf.Cos(initialAngle) * (incidentTime);
        movementVector.y = ((initialSpeed * Mathf.Sin(initialAngle) * (incidentTime)) - (0.5f * 9.8f * incidentTime * incidentTime));

        horizontalComponent = movementVector.z * movementDirection;

        Debug.DrawRay(this.transform.position, movementVector);

        this.transform.position += new Vector3(horizontalComponent.x, movementVector.y, horizontalComponent.z) * Time.fixedDeltaTime;
    }

}
