using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [Header("Level settings")]
    [SerializeField] float firingPeriod; //periodic firing every 3 seconds
    [SerializeField] float firingMaxCount; //number of required shots (10)

    [Header("Sound effects")] //would be better to add a separate audio manager class
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip fireSfx;
    [SerializeField] AudioClip hitEnemySfx;
    [SerializeField] AudioClip hitGroundSfx;


    [HideInInspector] public UnityEvent canFireEvent; //fires every 3 seconds

    [HideInInspector] public UnityEvent<bool> bulletHitEvent; //fires when the bullet hits the enemy (true) or the ground (false)

    [HideInInspector] public UnityEvent<int> updateUIEvent; //fires only after updating our score

    [HideInInspector] public UnityEvent<float,int> levelEndEvent; //fires when all required shots(10) have been fired

    Coroutine firingCoroutine;

    int score = 0;
    int shotsCount = 0;
    float accuracy = 0;

    public static LevelManager Instance {get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        bulletHitEvent.AddListener((isEnemy)=>OnBulletHitEvent(isEnemy));

        levelEndEvent.AddListener((accuracy, score)=>OnLevelEnd());

        firingCoroutine = StartCoroutine(FireCoroutine());
    }
   
    IEnumerator FireCoroutine()
    {
        //probably not the best way to do a repeation coroutine ;D
        yield return new WaitForSeconds(firingPeriod);

        canFireEvent.Invoke();
        PlayFireSfx();

        StopCoroutine(firingCoroutine);
        firingCoroutine = StartCoroutine(FireCoroutine());
    }

    private void OnBulletHitEvent(bool isEnemy)
    {
        shotsCount++;

        if (isEnemy)//written first to update the score UI before ending the level in case of the last shot (shot number 10)
        {
            score++;

            PlayHitEnemySfx();
            //update UI here
            updateUIEvent.Invoke(score);
        }
        else
        {
            PlayHitGroundSfx();
        }

        if (shotsCount >= firingMaxCount)
        {
            //level end here

            CalculateAccuracy();

            levelEndEvent.Invoke(accuracy, score);
        }
    }
    private void OnLevelEnd()
    {
        StopAllCoroutines();
    }
    private void CalculateAccuracy()
    {
        accuracy = (score / firingMaxCount) * 100;
    }

    #region Sound effects
    private void PlayFireSfx()
    {
        audioSource.clip = fireSfx;
        audioSource.Play();
    }
    private void PlayHitEnemySfx()
    {
        audioSource.clip = hitEnemySfx;
        audioSource.Play();
    }
    private void PlayHitGroundSfx()
    {
        audioSource.clip = hitGroundSfx;
        audioSource.Play();
    }
    #endregion
}
