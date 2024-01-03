using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    [SerializeField] AIMovement enemy;
    [SerializeField] Transform firingCannon;

    Vector3 bulletEndPosition; // this should fall on the line drawn to represent currentVelocity (enemy's path)
    void Start()
    {
        
    }

    void Update()
    {
        //just to visualize the enemy path and currentVelocity
        Debug.DrawLine(enemy.transform.position + Vector3.up * 0.5f, enemy.transform.position + enemy.currentVelocity * 5);
        SetBulletDestination();
    }

    private void SetBulletDestination()
    {
        //an initial position to throw at, should adjust with other factors to achieve a nice game feel
        bulletEndPosition = enemy.transform.position + enemy.transform.forward * 2;

        //aim at destination to shoot the bullet only on the horizontal axis (forward of the cannon)
        //this needs to have some coordination between cart rotation and firing cannon rotation to achieve a nice game feel
        //cart should rotate first then the firing cannon (IK animation ??)
        //or just adjust the inital rotation of the firing cannon to align horizontally on the cart (right now it has a little offset angle)
       this.transform.LookAt(bulletEndPosition);  
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(bulletEndPosition, 0.5f);
    }
}
