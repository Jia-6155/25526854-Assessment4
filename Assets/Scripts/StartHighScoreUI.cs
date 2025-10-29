using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class StartHighScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text level1ScoreText;
    [SerializeField] private TMP_Text level1TimeText;
    void Start()
    {
        int hs = PlayerPrefs.GetInt(GameManager.PP_HSCORE_KEY, 0);
        float bt = PlayerPrefs.GetFloat(GameManager.PP_BESTTIME_KEY, 0f);
        if (level1ScoreText) level1ScoreText.text = hs.ToString("000000");
        if (level1TimeText)  level1TimeText.text = GameManager.FormatTime(bt);
    }
    void Update()
    {
        
    }
}
