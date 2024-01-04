using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    [SerializeField] AIMovement enemy;
    [SerializeField] Transform firingCannon;
    [SerializeField] Transform firingStartPosition;
    [SerializeField] Transform grounded_firingStartPosition; //to make a unified vertical plane for firing position and bullet destination 
    [SerializeField] Bullet bulletPrefab;

    [SerializeField] float bulletDestinationOffset = 2;

    // this should fall on the line drawn to represent currentVelocity (enemy's path)
    Vector3 bulletEndPosition;


    //this represents the time the enemy takes to reach bulletEndPosition (enemy moves with constant/uniform velocity so it's a simple t = d/v)
    float enemyReachTime;


    //this represents the distance between the firing position bulletEndPosition (they have to be on the same vertical plane for our calcs to be correct)
    float bulletRange;

    void Start()
    {
        SetEnemyReachTime();

        InvokeRepeating("Fire",0,5);
    }

    void Update()
    {
        //just to visualize the enemy path and currentVelocity
        Debug.DrawLine(enemy.transform.position + Vector3.up * 0.5f, (enemy.transform.position + enemy.currentVelocity) + Vector3.up * 0.5f/* * 5*/);
    }

    private void SetBulletDestination()
    {
        //an initial position to throw at, should adjust with other factors to achieve a nice game feel
        bulletEndPosition = enemy.transform.position + enemy.transform.forward * bulletDestinationOffset;

        //aim at destination to shoot the bullet only on the horizontal axis (forward of the cannon)
        //this needs to have some coordination between cart rotation and firing cannon rotation to achieve a nice game feel
        //cart should rotate first then the firing cannon (IK animation ??)
        //or just adjust the inital rotation of the firing cannon to align horizontally on the cart (right now it has a little offset angle)
       this.transform.LookAt(bulletEndPosition);  
    }

    private void SetEnemyReachTime()
    {
        enemyReachTime = bulletDestinationOffset / enemy.currentVelocity.magnitude;
    }

    private void Fire()
    {
        SetBulletDestination();
        CalculateBulletRange();
        CalculateInitialFiringAngle();
        CalculateInitialFiringSpeed();

        firingCannon.Rotate(firingCannon.right, cannonFiringAngle_deg);

        //spawn the bullet at the cannon nozzle and orient it according to the firing angle
        Bullet spawnedBullet = Instantiate(bulletPrefab, firingStartPosition.position, firingCannon.transform.rotation);

        spawnedBullet.InitBulletForShooting(speed_o,theta_o); ;
    }


    //the set of methods used below are used to calculate the parameters to determine the bullet movement to reach the goal bulletEndPosition
    //we will be using projectile movement equations
    //I initially thought I'd use Newton's basic movement equations, but they take mass and air resistance into consideration, which isn't necessary in our case

    //we will be using: 
    //R = ((Vo)^2 * (sin 2*THETAo)) /g 
    //T = 2(Vo * (sin THETAo)/g)

    //where:
    // R is the horizontal range between firingStartPosition (cannon nozzle) and the the projectile destination bulletEndPosition
    // T is the total time for the bullet to reach bulletEndPosition
    // Vo is the initial velocity of the bullet at firingStartPosition
    // THETAo is the initial orientation angle of the bullet on the horizontal axis, in radians of course
    // g is the gravity downwards acceleration
    //
    // It's noteable that we have three unknowns in these two equations, so we will need to have an assumption to be able to solve them simultaneously: 
    // we assume T is equal to enemyReachTime (the time the enemy takes to reach bulletEndPosition)
    // Hence, we have two unknowns in two equations: Vo and THETAo
    // solving these equations simultaneously gives us:
    //
    // THETAo = cosInverse(2*R/g^2*T^2)
    // Vo = g*T/(2 sin(cosInverse(2*R/g^2*T^2))) >> g * T / (2 * sin(THETAo))
    //
    // we can then use these two values to determine the position of the bullet each frame using the two projectile position equations below:
    // [where t is the respective current point in time of this (x,y) calculation]
    // x = Vo * cos(THETAo) * t 
    // y = Vo sin(THETAo)*t - (0.5 * g * t^2) 


    float theta_o;
    float speed_o;

    float cannonFiringAngle_deg;

    private void CalculateBulletRange()
    {
        bulletRange = (bulletEndPosition - grounded_firingStartPosition.position).magnitude;
    }
    private void CalculateInitialFiringAngle()
    {
        theta_o = Mathf.Acos(2 * bulletRange / Mathf.Pow(9.8f * enemyReachTime, 2));

        cannonFiringAngle_deg = Mathf.Rad2Deg * theta_o; //convert to degrees for the Transform.Rotate fucntion
    }

    private void CalculateInitialFiringSpeed()
    {
        speed_o = 9.8f * enemyReachTime / (2 * Mathf.Sin(theta_o));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(bulletEndPosition + Vector3.up * 0.5f, 0.5f);
    }
}
