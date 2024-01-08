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
    float incidentTime; //any point in time (typically updates each fixed frame by an increment of Time.fixedDeltaTime)

    Vector3 velocityVector;
    Vector3 movementDirection;
    Vector3 horizontalComponent; //horizontal component of velocity (at target direction)
    Vector3 Target;

    bool canFly;

    public void InitBulletForShooting(float speed, float thetao_rad,Vector3 target)
    {
        Target = target;
        movementDirection = (target-this.transform.position).normalized;
        velocityVector = (target - this.transform.position).normalized; //aim velocity at target direction
        initialSpeed = speed;
        initialAngle = thetao_rad;
        startTime = Time.time;
        canFly = true;
    }

    //Displacement from firing position at any point in time (not used but here to clarify)
    //x = Vo* cos(THETAo) * t
    //y = Vo sin(THETAo)*t - (0.5 * g* t^2) 

    //Velocity at any point in time (used and updated each fixed frame, then added to the bullet previous frame position)
    //Vx = Vo* cos(THETAo);
    //Vy = Vo* sin(THETAo) - g*T;

    private void FixedUpdate()
    {
        if (!canFly) return;

        incidentTime = Time.time - startTime; //time value at this point in time

        velocityVector.z = initialSpeed * Mathf.Cos(initialAngle);
        velocityVector.y = ((initialSpeed * Mathf.Sin(initialAngle)) - (9.81f * incidentTime));

        horizontalComponent = velocityVector.z * movementDirection; //for the bullet to move in target direction

        Debug.DrawRay(this.transform.position, velocityVector);

        this.transform.position += new Vector3(horizontalComponent.x , velocityVector.y, horizontalComponent.z ) * Time.fixedDeltaTime;
    }


    //better to kill on collision instead of max time calculations to avoid weird looking results as we're not using accurate calculations
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag(MyTags.GroundTag))  //using a class for tags to avoid misspelling
        {
            print("destroyed");
            canFly = false;

            CannonController.Instance.AddWrongPosition(Target);


            LevelManager.Instance.bulletHitEvent.Invoke(/*isEnemy:*/ false);

            Destroy(this.gameObject); //only destroy for now, effects later
        }

        if (other.CompareTag(MyTags.EnemyTag))  //using a class for tags to avoid misspelling
        {
            print("successful hit !!!");
            canFly = false;

            CannonController.Instance.AddCorrectPosition(Target);

            LevelManager.Instance.bulletHitEvent.Invoke(/*isEnemy:*/ true);

            Destroy(this.gameObject); //only destroy for now, effects later
        }
    }

    private void OnDrawGizmos()
    {
        //Target to hit
        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(Target , 0.5f);
    }

}
