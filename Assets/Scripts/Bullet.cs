using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    float initialSpeed; //not velocity cuz scalar xD :D

    float initialAngle;

    Vector3 instantPoint;
    
    public void InitBulletForShooting(float speed, float thetao_rad)
    {
        initialSpeed = speed;
        initialAngle = thetao_rad;
    }

    //x = Vo* cos(THETAo) * t
    //y = Vo sin(THETAo)*t - (0.5 * g* t^2) 

    private void FixedUpdate()
    {
        instantPoint.z = initialSpeed * Mathf.Cos(initialAngle) * Time.fixedDeltaTime;
        instantPoint.y = initialSpeed * Mathf.Sin(initialAngle) * Time.fixedDeltaTime - (0.5f * -9.8f * Time.fixedDeltaTime * Time.fixedDeltaTime);

        print(instantPoint);
        //this.transform.position = instantPoint;
    }
}
