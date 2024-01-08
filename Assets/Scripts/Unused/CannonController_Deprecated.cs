using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController_Deprecated : MonoBehaviour
{
    [SerializeField] AIMovement enemy;
    [SerializeField] Transform firingCannon;
    [SerializeField] Transform firingStartPosition;
    [SerializeField] Transform grounded_firingStartPosition; //to make a unified vertical plane for firing position and bullet destination 
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] float maxHeight = 5;
    [SerializeField] float bulletDestinationOffset = 2;
    [SerializeField] float grav = 9.8f;

    // this should fall on the line drawn to represent currentVelocity (enemy's path)
    Vector3 bulletEndPosition;


    //this represents the time the enemy takes to reach bulletEndPosition (enemy moves with constant/uniform velocity so it's a simple t = d/v)
    float enemyReachTime;

   
    //this represents the distance between the firing position bulletEndPosition (they have to be on the same vertical plane for our calcs to be correct)
    float bulletRange;

    void Start()
    {
        //InvokeRepeating("Fire",2,5);
        //Invoke("Fire", 3);
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
        SetEnemyReachTime();
        CalculateBulletRange();
        //CalculateInitialFiringSpeed();
        CalculateInitialFiringAngle();


        ////spawn the bullet at the cannon nozzle and orient it according to the firing angle
        Bullet spawnedBullet = Instantiate(bulletPrefab, firingStartPosition.position, firingCannon.transform.rotation);

        //spawnedBullet.InitBulletForShooting(speed_o, theta_o,  grounded_firingStartPosition.position,bulletEndPosition);
    }

    #region Strategy

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
    // THETAo = cotInverse(2*R/g^2*T^2)
    // Vo = g*T/(2 sin(cotInverse(2*R/g^2*T^2))) >> g * T / (2 * sin(THETAo))
    //
    // we can then use these two values to determine the position of the bullet each frame using the two projectile position equations below:
    // [where t is the respective current point in time of this (x,y) calculation]
    // x = Vo * cos(THETAo) * t 
    // y = Vo sin(THETAo)*t - (0.5 * g * t^2) 

    #endregion


    float theta_o;
    float speed_o = 20;

    float cannonFiringAngle_deg;
    private void CalculateBulletRange()
    {
        bulletRange = (bulletEndPosition - grounded_firingStartPosition.position).magnitude;
    }
    private void CalculateInitialFiringAngle()
    {
        float? highAngle;
        float? lowAngle;

        CalculateAngleToHitTarget(out highAngle,out lowAngle);

        if (bulletRange >= 7)
        {
            theta_o = (float)highAngle;
        }
        else
        {
            theta_o = (float)lowAngle;
        }
    }

    //private void CalculateInitialFiringSpeed()
    //{
    //    float Vox = bulletRange / enemyReachTime;
    //    float Voy = (maxHeight/enemyReachTime) + (0.5f * grav * enemyReachTime);

    //    speed_o = Mathf.Sqrt((Vox*Vox) + (Voy*Voy));
    //    print(speed_o);
    //}

    void CalculateAngleToHitTarget(out float? theta1, out float? theta2)
    {
        //Initial speed
        float v = speed_o;

        Vector3 targetVec = bulletEndPosition - this.transform.position;

        //Vertical distance
        float y = targetVec.y;

        //Reset y so we can get the horizontal distance x
        targetVec.y = 0f;

        //Horizontal distance
        float x = targetVec.magnitude;

        //Gravity
        float g = 9.81f;


        //Calculate the angles

        float vSqr = v * v;

        float underTheRoot = (vSqr * vSqr) - g * (g * x * x + 2 * y * vSqr);

        //Check if we are within range
        if (underTheRoot >= 0f)
        {
            float rightSide = Mathf.Sqrt(underTheRoot);

            float top1 = vSqr + rightSide;
            float top2 = vSqr - rightSide;

            float bottom = g * x;

            theta1 = Mathf.Atan2(top1, bottom) * Mathf.Rad2Deg;
            theta2 = Mathf.Atan2(top2, bottom) * Mathf.Rad2Deg;
        }
        else
        {
            theta1 = null;
            theta2 = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(bulletEndPosition + Vector3.up * 0.5f, 0.5f);
    }
}
