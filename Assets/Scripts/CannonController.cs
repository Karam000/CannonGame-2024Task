using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    [Header("Cannon parts")]
    [SerializeField] Transform rotatingPart;
    [SerializeField] Transform firingPosition;

    [Header("Projectile")]
    [SerializeField] Bullet bulletPrefab;

    [Header("Enemy")]
    [SerializeField] AIMovement enemy;
    [SerializeField] float predictedTargetOffset;

    [Header("Recoil presets")]
    [SerializeField] float recoilDistance;
    [SerializeField] float recoilSpeed;

    //variables
    #region Cannon firing calculations
    Vector3 PredictedbulletDestination;
    Vector3 targetVec;
    float flightTime;
    float firingAngle_deg;
    float requiredInitialVelocity;
    float g = 9.81f;
    #endregion


    #region Recoil
    Vector3 startingRecoilTransform;
    float recoilDisplacement;
    bool canRevertRecoil;
    bool canRecoil; 
    #endregion

    public static CannonController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LevelManager.Instance.canFireEvent.AddListener(Fire);

        CalculateFlightTime();

        startingRecoilTransform = this.transform.position;
    }

    private void Update()
    {
        SetBulletDestination();

        AimCannon();
    }

    private void FixedUpdate()
    {
        if (canRecoil) 
            Recoil();

        else if (canRevertRecoil) 
            RevertRecoil();
    }


    //methods
    #region Aim 
    private void SetBulletDestination()
    {
        //destination = enemy postion + offset in the direction of enemy's movement
        PredictedbulletDestination = enemy.transform.position + predictedTargetOffset * enemy.transform.forward;
    }
    private void CalculateFlightTime()
    {
        //t = d/v (constant speed formula) 

        //>> this is the same for both the bullet reach and enemy reach time to the same point starting at the same time
        //>> this assumption solves us the whole problem :)

        //>> predictedTargetOffset can be adjusted to achieve better game feel (better flightTime translates to better/more realistic game feel)

        flightTime = (predictedTargetOffset / enemy.currentVelocity.magnitude);
    }
    private void CalculateRequiredInitialVelocity()
    {
        //using kinematic equation (displacement equation):
        //d = (Vo * t) + (0.5 * a * t^2)

        //where:

        //d: displacement to target, in our case the distance between firingPosition & PredictedbulletDestination (magnitude of the vector between them)
        //Vo: the required initial velocity for an object to travel a distance "d" in time "t"
        //t: time for the object to reach "object positon + d" starting at "Vo" speed
        //a: the working acceleration, in our case it's only gravitional force "g"

        //solving for "Vo" we get: Vo = (d - 0.5 * a * t^2) / t

        requiredInitialVelocity = (targetVec.magnitude - (0.5f * g * flightTime * flightTime)) / flightTime;
    }
    void CalculateAngleToHitTarget(out float? theta1, out float? theta2, float v,Vector3 targetVector)
    {
        //There are two angles because tan(_angle) at any point on a curve represents the slope of the vector tangent to the curve at this point where _angle is 
        //the angle between the horizontal axis and the tangent vector at this point
        //the slope of the curve is derivative of the curve
        //for the projectile motion curve equation: y = tan(theta_o) * x - g*x^2/2*(Vo * cos(theta_o))^2 
        //it's clear that there are two values for theta_o that solve: y' = tan(theta_o) = derivative of this curve [because it'a a quadratic equation in theta_o]

        //this might not be fully correct but that's what I could deduce :)

        //there might be another reason related to trignometry/analytical approaches
        //stuff like tan 30 = tan (30 +180) ... etc. But I couldn't come up with a relation this way


        //lots of local variables are declared each frame, bad performance (too many garbage collection)

        //Vertical distance occuring due to the offset between firingPosition and the enemy's vertical position
        float y = targetVector.y;

        //Reset y so we can get the horizontal distance x
        targetVector.y = 0f;

        //Horizontal distance
        float x = targetVector.magnitude;

        //Calculate the angles
        float vSqr = v * v;

        float underTheRoot = (vSqr * vSqr) - g * (g * x * x + 2 * y * vSqr);
       
        if (underTheRoot >= 0f)  //Check if we are within range (negative underTheRoot means that this speed won't get us to target point no matter what)
        {
            float rightSide = Mathf.Sqrt(underTheRoot);

            float top1 = vSqr + rightSide;
            float top2 = vSqr - rightSide;

            float bottom = g * x;

            theta1 = Mathf.Atan2(top1, bottom) * Mathf.Rad2Deg; //convert to degree for Transform.localEulerAngles rotation
            theta2 = Mathf.Atan2(top2, bottom) * Mathf.Rad2Deg; //convert to degree for Transform.localEulerAngles rotation
        }
        else
        {
            theta1 = null;
            theta2 = null;
        }
    }
    private void AimCannon()
    {
        float? highAngle = 0f;
        float? lowAngle = 0f;

        targetVec = PredictedbulletDestination - firingPosition.position; //projectile movement range (horizontal displacement)

        if (flightTime <= 0 || flightTime >= Mathf.Infinity) //happens if AIMovement hadn't determinded currentVelocity yet (only first shot)
            CalculateFlightTime();

        CalculateRequiredInitialVelocity();

        CalculateAngleToHitTarget(out highAngle, out lowAngle, requiredInitialVelocity,targetVec); //this gives us two angles for two curves (shallow and steep)

        transform.LookAt(PredictedbulletDestination);
        transform.eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f); //to make it only look in y rotation

        if (lowAngle != null) //by experimenting it's way better to use the shallow curve
        {
            firingAngle_deg = (float)lowAngle;

            rotatingPart.localEulerAngles = new Vector3(360f - (float)firingAngle_deg, 0f, 0f); //because it rotates negatively in the scene (-ve = up)
        }

        if (highAngle == null && lowAngle == null)
        {
            print("no angle found");
            firingAngle_deg = 45; //set for max reach angle (it won't reach anyway)
        }
    }
    #endregion

    #region Fire
    private void Fire()
    {
        //no need to have a pooling system for bullets spawning as it has a very slow rate
        Bullet spawnedBullet = Instantiate(bulletPrefab, firingPosition.position, this.transform.rotation);

        spawnedBullet.InitBulletForShooting(requiredInitialVelocity, (float)firingAngle_deg * Mathf.Deg2Rad, PredictedbulletDestination);

        canRecoil = true; //set it after shooting to avoid affecting firingPosition used in our previous calculations
    } 
    #endregion

    #region Recoil
    private void Recoil()
    {
        recoilDisplacement = Vector3.Distance(this.transform.position, startingRecoilTransform);

        if (recoilDisplacement <= recoilDistance) //try to reach a suitable backward position
        {
            this.transform.position -= (this.transform.forward * recoilSpeed * Time.fixedDeltaTime); //move backwards at high speed (recoilSpeed)
        }
        else
        {
            canRecoil = false;
            canRevertRecoil = true;
        }
    }
    private void RevertRecoil()
    {
        recoilDisplacement = Vector3.Distance(this.transform.position, startingRecoilTransform);

        if (recoilDisplacement > 0) //keep moving until the cannon reaches its original position
        {
            //move forward at low speed (one tenth of recoilSpeed)
            this.transform.position = Vector3.MoveTowards(this.transform.position, startingRecoilTransform, recoilSpeed * Time.fixedDeltaTime / 10);
        }
        else
        {
            canRevertRecoil = false;
        }
    }
    #endregion


    //validation
    #region Validation Gizmos
    [HideInInspector] public List<Vector3> correctPositions;
    [HideInInspector] public List<Vector3> wrongPositions;

    public void AddCorrectPosition(Vector3 corPos)
    {
        correctPositions.Add(corPos);
    }

    public void AddWrongPosition(Vector3 corPos)
    {
        if(!correctPositions.Contains(corPos))
        wrongPositions.Add(corPos);
    }

    private void OnDrawGizmos()
    {
        //Enemy movement vector visualization
        Gizmos.color = Color.white;
        Gizmos.DrawLine(enemy.transform.position, enemy.transform.position + enemy.transform.forward * 10);

        //Predicted bullet position visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(PredictedbulletDestination, 0.5f);

        //Enemy non-hit position visualization (mostly due to AIMovement randomness)
        Gizmos.color = Color.red;
        foreach (var pos in wrongPositions)
        {
            Gizmos.DrawSphere(pos, 0.5f);
        }

        //Enemy correctly-hit position visualization
        Gizmos.color = Color.green;
        foreach (var pos in correctPositions)
        {
            Gizmos.DrawSphere(pos, 0.5f);
        }
    } 
    #endregion
}
