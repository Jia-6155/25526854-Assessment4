using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }
    [Header("Config")]
    [Header("Gameplay")]
    [SerializeField] private int startLives = 3;
    [SerializeField] private string levelDisplayName = "LEVEL 1";
    [SerializeField] private string startSceneName = "StartScene";
    public int Score { get; private set; }
    public int Lives { get; private set; }
    private float runTimer;
    private bool timerRunning;
    private float scaredRemain;
    private bool scaredActive;
    [Header("HUD Refs")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scaredTimerText;
    [SerializeField] private Transform livesContainer;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject lifeIconPrefab;
    [Header("Round Start Overlay")]
    [SerializeField] private GameObject overlayBlocker;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text gameOverText;
    [Header("Scene Hooks")]
    [SerializeField] private PacStudentController pac;
    [SerializeField] private GhostController[] ghosts;
    [Header("Audio")]
    [SerializeField] private AudioManager audioMgr;
    public const string PP_HSCORE_KEY = "A4_Level1_HighScore";
    public const string PP_BESTTIME_KEY = "A4_Level1_BestTime";
    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }
    void Start()
    {
        Lives = startLives;
        Score = 0;
        runTimer = 0f;
        timerRunning = false;
        scaredActive = false;
        if (levelNameText) levelNameText.text = levelDisplayName;
        if (overlayBlocker) overlayBlocker.SetActive(true);
        if (gameOverText) gameOverText.gameObject.SetActive(false);
        if (scaredTimerText) { scaredTimerText.gameObject.SetActive(false); scaredTimerText.text = ""; }
        BuildLivesUI();
        UpdateScoreUI();
        UpdateTimerUI(0f);
        SetPlayable(false);
        StartCoroutine(RoundStartRoutine());
    }
    public void AddScore(int delta)
    {
        Score += Mathf.Max(0, delta);
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = Score.ToString("000000");
    }
    private void BuildLivesUI()
    {
        if (!livesContainer || !lifeIconPrefab)
        {
            Debug.LogWarning("⚠️ LivesContainer or LifeIconPrefab is missing!");
            return;
        }
        foreach (Transform c in livesContainer) Destroy(c.gameObject);
        for (int i = 0; i < Lives; i++)
        {
            Instantiate(lifeIconPrefab, livesContainer);
            Debug.Log($"✅ Created life {i + 1}");
        }
    }
    public void LoseOneLifeAndRespawn()
    {
        Lives = Mathf.Max(0, Lives - 1);
        BuildLivesUI();
        if (Lives <= 0)
        {
            StartCoroutine(GameOverRoutine());
        }
        else
        {
            SetPlayable(false);
            if (pac) pac.RespawnToStart();
            foreach (var g in ghosts) if (g) g.ResetToSpawn_Normal();
            StartCoroutine(ShortReadyGoAndResume());
        }
    }
    private IEnumerator ShortReadyGoAndResume()
    {
        audioMgr?.StopAll();
        SetPlayable(false);
        if (overlayBlocker) overlayBlocker.SetActive(true);
        if (countdownText) countdownText.gameObject.SetActive(true);
        string[] steps = { "3", "2", "1", "GO!" };
        foreach (var s in steps)
        {
            countdownText.text = s;
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(0.5f);
        if (countdownText) countdownText.gameObject.SetActive(false);
        if (overlayBlocker) overlayBlocker.SetActive(false);
        SetPlayable(true);
        StartTimer();
        audioMgr?.PlayBGM_Normal();
    }
    private IEnumerator RoundStartRoutine()
    {
        audioMgr?.StopAll();
        SetPlayable(false);
        if (overlayBlocker) overlayBlocker.SetActive(true);
        if (countdownText) countdownText.gameObject.SetActive(true);
        string[] steps = { "3", "2", "1", "GO!" };
        foreach (var s in steps)
        {
            countdownText.text = s;
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(0.5f);
        if (scoreText) scoreText.gameObject.SetActive(true);
        if (livesContainer) livesContainer.gameObject.SetActive(true);
        if (levelNameText) levelNameText.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        if (countdownText) countdownText.gameObject.SetActive(false);
        if (overlayBlocker) overlayBlocker.SetActive(false);
        SetPlayable(true);
        StartTimer();
        audioMgr?.PlayBGM_Normal();
    }
    void Update()
    {
        if (timerRunning)
        {
            runTimer += Time.deltaTime;
            UpdateTimerUI(runTimer);
        }
        if (scaredActive)
        {
            scaredRemain -= Time.deltaTime;
            if (scaredRemain < 0f) scaredRemain = 0f;
            if (scaredTimerText)
            {
                if (scaredRemain > 0f)
                {
                    scaredTimerText.gameObject.SetActive(true);
                    scaredTimerText.text = Mathf.CeilToInt(scaredRemain).ToString();
                }
                else
                {
                    scaredTimerText.gameObject.SetActive(false);
                    scaredTimerText.text = "";
                }
            }
            if (scaredRemain <= 3f && scaredRemain > 0f)
            {
                foreach (var g in ghosts) if (g) g.SetRecovering();
            }
            if (scaredRemain <= 0f)
            {
                scaredActive = false;
                foreach (var g in ghosts) if (g) g.SetNormalIfNotDead();
                audioMgr?.PlayBGM_Normal();
            }
        }
    }
    private void StartTimer() { timerRunning = true; }
    private void StopTimer()  { timerRunning = false; }
    private void UpdateTimerUI(float seconds)
    {
        if (!timerText) return;
        int mm = Mathf.FloorToInt(seconds / 60f);
        int ss = Mathf.FloorToInt(seconds % 60f);
        int cs = Mathf.FloorToInt((seconds - Mathf.Floor(seconds)) * 100f);
        timerText.text = $"{mm:00}:{ss:00}:{cs:00}";
    }
    public void TriggerPowerPellet()
    {
        AddScore(50);
        scaredRemain = 10f;
        scaredActive = true;
        foreach (var g in ghosts)
        {
            if (g)
            {
                g.SetMovable(true);
                g.SetScared();
            }
        }
        audioMgr?.PlayBGM_Scared();
    }
    public void OnGhostEaten(GhostController g)
    {
        AddScore(300);
        audioMgr?.PlaySFX_GhostEaten();
    }
    public void CheckPelletWinCondition(int remainingPellets)
    {
        if (remainingPellets <= 0)
        {
            StartCoroutine(GameOverRoutine());
        }
    }
    private IEnumerator GameOverRoutine()
    {
        SetPlayable(false);
        StopTimer();
        if (overlayBlocker) overlayBlocker.SetActive(true);
        if (gameOverText)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "GAME OVER";
        }
        audioMgr?.PlayBGM_GameOver();
        SaveHighScoreIfBetter();
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(startSceneName);
    }
    private void SaveHighScoreIfBetter()
    {
        int prevScore = PlayerPrefs.GetInt(PP_HSCORE_KEY, 0);
        float prevTime = PlayerPrefs.GetFloat(PP_BESTTIME_KEY, float.MaxValue);

        bool better = (Score > prevScore) || (Score == prevScore && runTimer < prevTime);
        if (better)
        {
            PlayerPrefs.SetInt(PP_HSCORE_KEY, Score);
            PlayerPrefs.SetFloat(PP_BESTTIME_KEY, runTimer);
            PlayerPrefs.Save();
        }
    }
    private void SetPlayable(bool enableMove)
    {
        if (pac)
        {
            pac.SetControllable(enableMove);
            pac.enabled = enableMove;
        }
        foreach (var g in ghosts)
        {
            if (!g) continue;
            g.SetMovable(enableMove);
            g.enabled = enableMove;
        }
        if (exitButton) exitButton.interactable = enableMove;
    }
    public static string FormatTime(float seconds)
    {
        int mm = Mathf.FloorToInt(seconds / 60f);
        int ss = Mathf.FloorToInt(seconds % 60f);
        int cs = Mathf.FloorToInt((seconds - Mathf.Floor(seconds)) * 100f);
        return $"{mm:00}:{ss:00}:{cs:00}";
    }
}