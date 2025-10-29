using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class StartSceneHighScore : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText;
    void Start()
    {
        int bestScore = PlayerPrefs.GetInt(GameManager.PP_HSCORE_KEY, 0);
        float bestTime = PlayerPrefs.GetFloat(GameManager.PP_BESTTIME_KEY, 0f);
        if (highScoreText)
            highScoreText.text = $"High Score: {bestScore}\nBest Time: {GameManager.FormatTime(bestTime)}";
    }
    void Update()
    {
        
    }
}
