using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonRotator : MonoBehaviour
{
    [SerializeField] Transform rotatingPart;
    [SerializeField] public AIMovement enemy;
    [SerializeField] public Transform firingPosition;
    public float v = 15;

    [HideInInspector] public float? angle;

    float flightTime;
    [HideInInspector] public Vector3 bulletEndPosition;
    public static CannonRotator instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

   
    private void Update()
    {
        AimCannon();
    }
    Vector3 targetVec;
    private void AimCannon()
    {
        float? highAngle = 0f;
        float? lowAngle = 0f;

        targetVec = enemy.transform.position - firingPosition.position;

        CalculateAngleToHitTarget(out highAngle, out lowAngle);

        transform.LookAt(enemy.transform);
        transform.eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);

        if (lowAngle != null)
        {
            angle = (float)lowAngle;

            print("low " + angle);

            rotatingPart.localEulerAngles = new Vector3(360f - (float)angle, 0f, 0f);

            flightTime = targetVec.magnitude / (v * Mathf.Cos((float)angle * Mathf.Deg2Rad));

            bulletEndPosition = enemy.transform.position + (enemy.currentVelocity.magnitude * flightTime) * enemy.transform.forward;

            transform.LookAt(bulletEndPosition);
            transform.eulerAngles = new Vector3(0f, transform.rotation.eulerAngles.y, 0f);
        }


        if (highAngle == null && lowAngle == null) angle = null;
    }

    void CalculateAngleToHitTarget(out float? theta1, out float? theta2)
    {
        //Initial speed
        
               
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

}
