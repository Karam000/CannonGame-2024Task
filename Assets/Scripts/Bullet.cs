using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    bool canFly;

    public void InitBulletForShooting(float speed, float thetao_rad,Vector3 target)
    {
        Target = target;
        movementDirection = (target-this.transform.position).normalized;
        velocityVector = (target - this.transform.position).normalized;
        initialSpeed = speed;
        initialAngle = thetao_rad;
        startTime = Time.time;
        canFly = true;
    }

    //x = Vo* cos(THETAo) * t
    //y = Vo sin(THETAo)*t - (0.5 * g* t^2) 

    //Vx = Vo* cos(THETAo);
    //Vy = Vo* sin(THETAo) - g*T;
    private void FixedUpdate()
    {
        if (!canFly) return;

        incidentTime = Time.time - startTime;

        velocityVector.z = initialSpeed * Mathf.Cos(initialAngle);
        velocityVector.y = ((initialSpeed * Mathf.Sin(initialAngle)) - (9.81f * incidentTime));

        horizontalComponent = velocityVector.z * movementDirection;

        Debug.DrawRay(this.transform.position, velocityVector);

        this.transform.position += new Vector3(horizontalComponent.x , velocityVector.y, horizontalComponent.z ) * Time.fixedDeltaTime;
    }


    //better to kill on collision instead of max time calculations to avoid weird looking results as we're not using accurate calculations
    private void OnTriggerEnter(Collider other) 
    {
        //using a class for tags to avoid misspelling
        if(other.CompareTag(MyTags.GroundTag))
        {
            print("destroyed");
            canFly = false;
            Destroy(this.gameObject); //only destroy for now, effects later
        }
        if (other.CompareTag(MyTags.EnemyTag))
        {
            print("successful hit !!!");
            canFly = false;
            LevelManager.Instance.enemyHitEvent.Invoke();
            Destroy(this.gameObject); //only destroy for now, effects later
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(Target , 0.5f);
    }

}
