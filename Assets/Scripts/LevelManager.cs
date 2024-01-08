using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField] float firingPeriod;
    [SerializeField] float firingMaxCount;

    [HideInInspector] public UnityEvent canFireEvent;

    [HideInInspector] public UnityEvent<bool> bulletHitEvent;

    [HideInInspector] public UnityEvent<int> updateUIEvent;
    [HideInInspector] public UnityEvent<float,int> levelEndEvent;

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
        yield return new WaitForSeconds(firingPeriod);
        canFireEvent.Invoke();
        StopCoroutine(firingCoroutine);
        firingCoroutine = StartCoroutine(FireCoroutine());
    }

    private void OnBulletHitEvent(bool isEnemy)
    {
        shotsCount++;
        if (isEnemy)
        {
            score++;

            //update UI here
            updateUIEvent.Invoke(score);
        }
        if (shotsCount >= firingMaxCount)
        {
            //level end here

            CalcualteAccuracy();

            levelEndEvent.Invoke(accuracy, score);
        }
    }
    private void OnLevelEnd()
    {
        StopAllCoroutines();
    }

    private void CalcualteAccuracy()
    {
        accuracy = (score / firingMaxCount) * 100;
    }
}
