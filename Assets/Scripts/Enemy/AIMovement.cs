using UnityEngine;

/// <summary>
/// NOTE : Please don't modify/add/change this script your main goal is to only use currentVelocity
/// to predict movement if you feel there's an issue with the script please mail us
/// </summary>
public class AIMovement : MonoBehaviour
{
    //this is the only property you allowed to read from this script
    public Vector3 currentVelocity { get; private set; }

    //Split the map into 4 zones , the center for each zone should be 10 but we are forcing center near the edge
    //to help make the AI move longer in straight line
    private readonly Vector3[] mapZones = new Vector3[4] {
        new Vector3( 12  , 0 ,  12  ),
        new Vector3(-12  , 0 ,  12  ),
        new Vector3(-12  , 0 , -12  ),
        new Vector3( 12  , 0 , -12  ),
    };

    private readonly int maxSpeed = 5;

    //Don't modify this property to try and read it
    private Vector3 targetPosition;

    // Start is called before the first frame update
    private void Start()
    {
        GenerateNewTargetPosition();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateVelocity();
        MoveObject();
        CheckIfTargetReached();
    }

    //Update velocity to seek the target direction
    private void UpdateVelocity()
    {
        //this is based on steering behaviour logic for smooth movement
        Vector3 desiredVelocity = (targetPosition - transform.position).normalized;
        desiredVelocity *= maxSpeed;
        Vector3 steering = desiredVelocity - currentVelocity;
        steering.y = 0;
        currentVelocity = Vector3.ClampMagnitude(currentVelocity + steering, maxSpeed);
    }

    //Move object to target
    private void MoveObject()
    {
        transform.position += currentVelocity * Time.deltaTime;
        transform.forward = currentVelocity.normalized;
    }

    private void CheckIfTargetReached()
    {
        if ((targetPosition - transform.position).sqrMagnitude < 2)
        {
            GenerateNewTargetPosition();
        }
    }

    public void GenerateNewTargetPosition()
    {
        //we use the concept of zones to try and force find a random far position , so it moves in straigh path as long as possible
        int currentZone = 0;
        int nextZone = 0;
        for (int i = 0; i < mapZones.Length; i++)
        {
            if ((transform.position - mapZones[i]).sqrMagnitude < 100)
            {
                currentZone = i;
                break;
            }
        }

        //get random next zone
        nextZone = currentZone + Random.Range(1, 3);
        nextZone = nextZone % mapZones.Length;

        //Get a random position around that zone
        Vector3 rnd = (Random.onUnitSphere * 8);
        rnd.y = 0;
        targetPosition = mapZones[nextZone] + rnd;

        //some cases we will land on the edges between 2 zones
        //if we are too close to new position just move to center of this new zone
        if ((targetPosition - transform.position).sqrMagnitude < 80)
        {
            targetPosition = mapZones[nextZone];
        }

        targetPosition.y = 0;
    }
}