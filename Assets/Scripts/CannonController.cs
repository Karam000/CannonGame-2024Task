using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CannonController : MonoBehaviour
{

    [Header("Cannon parts")]
    [SerializeField] Transform rotatingPart;
    [SerializeField] public Transform firingPosition;

    [Header("Projectile")]
    [SerializeField] float initialBulletVelocity = 15;
    [SerializeField] Bullet bulletPrefab;

    [Header("Enemy")]
    [SerializeField] public AIMovement enemy;

    [Header("Recoil presets")]
    [SerializeField] float recoilDistance;
    [SerializeField] float recoilSpeed;

    [HideInInspector] public Vector3 bulletEndPosition;
    [HideInInspector] public float? angle_deg;
    [HideInInspector] public bool canRecoil;

    Vector3 targetVec;
    Vector3 startingRecoilTransform;
    float flightTime;
    bool canRevertRecoil;
    float g = 9.81f;
    public static CannonController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        startingRecoilTransform = this.transform.position;
        InvokeRepeating("Fire", 2, 5);
    }
    private void Update()
    {
        AimCannon();
    }

    private void FixedUpdate()
    {
        if (canRecoil)
            Recoil();

        else if (canRevertRecoil) RevertRecoil();
    }

    #region Aim 
    private void AimCannon()
    {
        float? highAngle = 0f;
        float? lowAngle = 0f;

        targetVec = enemy.transform.position - firingPosition.position;

        CalculateAngleToHitTarget(out highAngle, out lowAngle); //this gives us two angles for two curves (shallow and steep) cuz it has a square root

        transform.LookAt(enemy.transform);
        transform.eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f); //to make it only look in y rotation

        if (lowAngle != null) //by experimenting it's way better to use the shallow curve with high speeds (25+)
        {
            angle_deg = (float)lowAngle;

            rotatingPart.localEulerAngles = new Vector3(360f - (float)angle_deg, 0f, 0f); //because it rotates negatively in the scene (-ve = up)
            
            //The following region has an approximation as we don't adjust the angle after setting the target to bulletEndPosition instead of enemy
            //I think there should be some kind of recursive operations that goes back and forth to determine the perfect target and firing angle
            //(something like PID control where you get output feedback and adjust your input accordingly)

            #region This should be the accurate flight time calculation but it's not giving good results
            //float vsin = initialBulletVelocity * Mathf.Sin((float)angle_deg * Mathf.Deg2Rad);

            //float sqrt = Mathf.Sqrt((vsin * vsin) + (2 * g * verticalDisplacment.y));

            //float top = vsin + sqrt;

            //flightTime = top / g; 
            #endregion

            flightTime = targetVec.magnitude / (initialBulletVelocity * Mathf.Cos((float)angle_deg * Mathf.Deg2Rad));

            bulletEndPosition = enemy.transform.position + (enemy.currentVelocity.magnitude * flightTime) * enemy.transform.forward;

            transform.LookAt(bulletEndPosition);
            transform.eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
        }

        if (highAngle == null && lowAngle == null) angle_deg = null;
    }
    void CalculateAngleToHitTarget(out float? theta1, out float? theta2)
    {
        //lots of local variables are declared each frame, bad performance (too many garbage collection)


        //Vertical distance
        float y = targetVec.y;

        //Reset y so we can get the horizontal distance x
        targetVec.y = 0f;

        //Horizontal distance
        float x = targetVec.magnitude;

        //Calculate the angles
        float vSqr = initialBulletVelocity * initialBulletVelocity;

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
    #endregion

    private void Fire()
    {
        if (angle_deg == null) return;

        Bullet spawnedBullet = Instantiate(bulletPrefab, firingPosition.position, this.transform.rotation);

        spawnedBullet.InitBulletForShooting(initialBulletVelocity, (float)angle_deg * Mathf.Deg2Rad, bulletEndPosition);

        canRecoil = true;
    }

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

}
