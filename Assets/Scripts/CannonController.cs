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
    Vector3 PredictedbulletEndPosition;
    Vector3 targetVec;
    float flightTime;
    float angle_deg;
    float requiredInitialVelocity;
    float g = 9.81f;
    #endregion


    #region Recoil
    Vector3 startingRecoilTransform;
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

        else if (canRevertRecoil) RevertRecoil();
    }


    //methods
    #region Aim 
    private void SetBulletDestination()
    {
        PredictedbulletEndPosition = enemy.transform.position + predictedTargetOffset * enemy.transform.forward;
    }
    private void CalculateFlightTime()
    {
        flightTime = (predictedTargetOffset / enemy.currentVelocity.magnitude);
    }
    private void CalculateRequiredInitialVelocity()
    {
        requiredInitialVelocity = (targetVec.magnitude - (0.5f * g * flightTime * flightTime)) / flightTime;
    }
    void CalculateAngleToHitTarget(out float? theta1, out float? theta2, float v,Vector3 targetVector)
    {
        //lots of local variables are declared each frame, bad performance (too many garbage collection)

        //Vertical distance
        float y = targetVector.y;

        //Reset y so we can get the horizontal distance x
        targetVector.y = 0f;
        //Horizontal distance
        float x = targetVector.magnitude;

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
    private void AimCannon()
    {
        float? highAngle = 0f;
        float? lowAngle = 0f;

        targetVec = PredictedbulletEndPosition - firingPosition.position;
        print(flightTime);

        if (flightTime <= 0 || flightTime >= Mathf.Infinity) //happens if AIMovement hadn't determinded currentVelocity yet (only first shot)
            CalculateFlightTime();

        CalculateRequiredInitialVelocity();

        CalculateAngleToHitTarget(out highAngle, out lowAngle, requiredInitialVelocity,targetVec); //this gives us two angles for two curves (shallow and steep) cuz it has a square root

        transform.LookAt(PredictedbulletEndPosition);
        transform.eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f); //to make it only look in y rotation

        if (lowAngle != null) //by experimenting it's way better to use the shallow curve
        {
            angle_deg = (float)lowAngle;

            rotatingPart.localEulerAngles = new Vector3(360f - (float)angle_deg, 0f, 0f); //because it rotates negatively in the scene (-ve = up)
        }

        if (highAngle == null && lowAngle == null)
        {
            print("no angle found");
            angle_deg = 45; //set for max reach angle (it won't reach anyway)
        }
    }
    #endregion

    #region Fire
    private void Fire()
    {
        Bullet spawnedBullet = Instantiate(bulletPrefab, firingPosition.position, this.transform.rotation);

        spawnedBullet.InitBulletForShooting(requiredInitialVelocity, (float)angle_deg * Mathf.Deg2Rad, PredictedbulletEndPosition);

        canRecoil = true;
    } 
    #endregion

    #region Recoil
    private void Recoil()
    {
        float movedSpace = Vector3.Distance(this.transform.position, startingRecoilTransform);
        if (movedSpace <= recoilDistance)
        {
            this.transform.position -= (this.transform.forward * recoilSpeed * Time.fixedDeltaTime);
        }
        else
        {
            canRecoil = false;
            canRevertRecoil = true;
        }
    }
    private void RevertRecoil()
    {
        float movedSpace = Vector3.Distance(this.transform.position, startingRecoilTransform);

        if (movedSpace > 0)
        {
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
        Gizmos.DrawSphere(PredictedbulletEndPosition, 0.5f);

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
