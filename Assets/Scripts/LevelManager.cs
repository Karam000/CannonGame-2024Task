using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    [SerializeField] float firingPeriod;
    [SerializeField] float firingMaxCount;

    [HideInInspector] public UnityEvent enemyHitEvent;
    [HideInInspector] public UnityEvent canFireEvent;

    Coroutine firingCoroutine;


    int score = 0;

    public static LevelManager Instance {  get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        enemyHitEvent.AddListener(OnEnemyHit);
        firingCoroutine = StartCoroutine(FireCoroutine());
    }
   
    IEnumerator FireCoroutine()
    {
        yield return new WaitForSeconds(firingPeriod);
        canFireEvent.Invoke();
        StopCoroutine(firingCoroutine);
        firingCoroutine = StartCoroutine(FireCoroutine());
    }

    private void OnEnemyHit()
    {
        if(score < firingMaxCount)
        {
            score++;
            //update UI here
        }

        if(score > firingMaxCount)
        {
            //level end here
        }      
    }
}
