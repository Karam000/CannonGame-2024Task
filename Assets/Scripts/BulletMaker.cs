using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMaker : MonoBehaviour
{
    [SerializeField] Bullet bulletPrefab;

    Transform firingPos;
    Vector3 target;

    float speed;
    float firingAngle;

    private void Start()
    {
        firingPos = CannonRotator.instance.firingPosition;
        speed = CannonRotator.instance.v;
        InvokeRepeating("Fire",2,5);
    }

    private void Fire()
    {
        if (CannonRotator.instance.angle == null) return;

        target = CannonRotator.instance.bulletEndPosition;

        Bullet spawnedBullet = Instantiate(bulletPrefab,firingPos.position,this.transform.rotation);

        firingAngle = (float)CannonRotator.instance.angle * Mathf.Deg2Rad;

        spawnedBullet.InitBulletForShooting(speed, firingAngle,target);
    }
}
