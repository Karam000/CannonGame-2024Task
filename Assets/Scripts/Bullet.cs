using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Bullet : MonoBehaviour
{
    float initialSpeed; //not velocity cuz scalar xD :D

    float initialAngle;

    Vector3 velocityVector;
    Vector3 movementDirection;

    Vector3 Target;

    float startTime;
    float incidentTime;

    bool allSet = false;
    public void InitBulletForShooting(float speed, float thetao_rad,Vector3 target/*,Vector3 groundPosition,Vector3 target*/)
    {
        //transform.forward = (target-groundPosition).normalized;
        Target = target;
        movementDirection = transform.forward;
        velocityVector = transform.forward;
        initialSpeed = speed;
        initialAngle = thetao_rad;
        startTime = Time.time;
        allSet = true;
    }

    //x = Vo* cos(THETAo) * t
    //y = Vo sin(THETAo)*t - (0.5 * g* t^2) 

    //Vx = Vo* cos(THETAo);
    //Vy = Vo* sin(THETAo) - g*T;

    Vector3 horizontalComponent;

    private void FixedUpdate()
    {
        if (!allSet) return;

        incidentTime = Time.time - startTime;

        velocityVector.z = initialSpeed * Mathf.Cos(initialAngle);
        velocityVector.y = ((initialSpeed * Mathf.Sin(initialAngle)) - (9.8f * incidentTime));

        horizontalComponent = velocityVector.z * movementDirection;

        Debug.DrawRay(this.transform.position, velocityVector);

        this.transform.position += new Vector3(horizontalComponent.x , velocityVector.y, horizontalComponent.z ) * Time.fixedDeltaTime;
    }

    private void OnDrawGizmos()
    {
        if (!allSet) return;
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(Target , 0.5f);
    }

}
