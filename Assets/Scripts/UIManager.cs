using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Text currentTimeTxt;
    [SerializeField] Text hitAmount_inGameTxt;
    [SerializeField] Text hitAmount_resTxt;
    [SerializeField] Text accuracyTxt;

    [SerializeField] GameObject resultPanel;
    public static UIManager Instance { get; private set; }

    bool islevelEnd = false;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        LevelManager.Instance.updateUIEvent.AddListener((score)=>OnEnemyHit(score));

        LevelManager.Instance.levelEndEvent.AddListener((accuracy,score)=>OnLevelEnd(accuracy, score));
    }


    //not sure if it's the best way to calculate time here to print to currentTimeTxt
    float elapsedTime;
    private void Update()
    {
        if (islevelEnd) return;

        elapsedTime += Time.deltaTime;
        currentTimeTxt.text = ((int)elapsedTime).ToString();
    }

    private void OnEnemyHit(int hitCount)
    {
        hitAmount_inGameTxt.text = hitCount.ToString();
    }

    private void OnLevelEnd(float accuracy, int hitCount)
    {
        accuracyTxt.text = accuracy.ToString() + " %";
        hitAmount_resTxt.text = hitCount.ToString();
        islevelEnd = true;  
        resultPanel.SetActive(true);
    }

    //called from inspector (replay button in result panel)  << comment inspector functions to avoid confusion in the future
    public void RestartLevel()
    {
        SceneManager.LoadScene("Gameplay"); //it would be better to have a sceneHandler class to handle restart and other operations on scenes
    }

}
