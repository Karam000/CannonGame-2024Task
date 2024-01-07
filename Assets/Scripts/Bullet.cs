using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Bullet : MonoBehaviour
{
    float initialSpeed; //not velocity cuz scalar xD :D
    float initialAngle;
    float startTime;
    float incidentTime;

    Vector3 velocityVector;
    Vector3 movementDirection;
    Vector3 horizontalComponent;
    Vector3 Target;

    public void InitBulletForShooting(float speed, float thetao_rad,Vector3 target)
    {
        Target = target;
        movementDirection = (target-this.transform.position).normalized;
        velocityVector = (target - this.transform.position).normalized;
        initialSpeed = speed;
        initialAngle = thetao_rad;
        startTime = Time.time;
    }

    //x = Vo* cos(THETAo) * t
    //y = Vo sin(THETAo)*t - (0.5 * g* t^2) 

    //Vx = Vo* cos(THETAo);
    //Vy = Vo* sin(THETAo) - g*T;

    private void FixedUpdate()
    {
        incidentTime = Time.time - startTime;

        velocityVector.z = initialSpeed * Mathf.Cos(initialAngle);
        velocityVector.y = ((initialSpeed * Mathf.Sin(initialAngle)) - (9.81f * incidentTime));

        horizontalComponent = velocityVector.z * movementDirection;

        Debug.DrawRay(this.transform.position, velocityVector);

        this.transform.position += new Vector3(horizontalComponent.x , velocityVector.y, horizontalComponent.z ) * Time.fixedDeltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(Target , 0.5f);
    }

}
