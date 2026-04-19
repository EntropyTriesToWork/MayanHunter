using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ThrowIndicator throwIndicator;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject failurePanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject starOne, starTwo, starThree;
    [SerializeField] private LevelSettings levelSettings;

    [Header("Level Settings")]
    [SerializeField] private float outcomeDelay = 1f;

    [Header("Score UI")]
    [SerializeField] private TMPro.TMP_Text scoreText;
    [SerializeField] private TMPro.TMP_Text levelText;

    private List<LevelObject> _targets = new List<LevelObject>();
    private int _throwsRemaining;
    private int _totalScore;
    private bool _levelEnded;
    public static GameManager Instance;

    public void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void OnEnable()
    {
        GameEvents.OnLevelObjectDestroyed += HandleObjectDestroyed;
        GameEvents.OnLevelObjectDamaged += HandleObjectDamaged;
    }

    private void OnDisable()
    {
        GameEvents.OnLevelObjectDestroyed -= HandleObjectDestroyed;
        GameEvents.OnLevelObjectDamaged -= HandleObjectDamaged;
    }

    private void Start()
    {
        SaveManager.Instance?.SetLevelOrder(levelSettings);
        if(levelSettings.selectedLevel == null) 
        { Debug.LogWarning("No level selected! Selecting level 1 as a backup."); 
            levelSettings.selectedLevel = levelSettings.allLevels[0]; }
        _totalScore = 0;
        _levelEnded = false;
        _throwsRemaining = levelSettings.selectedLevel.startingThrows;
        throwIndicator?.Initialise(_throwsRemaining);

        Instantiate(levelSettings.selectedLevel.levelPrefab);
        cameraController.scrollMax = levelSettings.selectedLevel.cameraScrollMax.y;
        cameraController.scrollMin = levelSettings.selectedLevel.cameraScrollMax.x;
        if (levelSettings.selectedLevel == levelSettings.allLevels[levelSettings.allLevels.Count-1])
        {
            nextLevelButton.SetActive(false);
        }
        // Wire up player controller callbacks
        if (playerController != null)
        {
            playerController.OnProjectileLaunched += HandleProjectileLaunched;
            playerController.OnProjectileSettled += HandleProjectileSettled;
        }

        // Hide outcome panels
        SetPanelActive(victoryPanel, false);
        SetPanelActive(failurePanel, false);
        SetPanelActive(hudPanel, true);

        RefreshHUD();
    }

    private void HandleProjectileLaunched()
    {
        _throwsRemaining = Mathf.Max(0, _throwsRemaining - 1);
        GameEvents.ThrowsRemainingChanged(_throwsRemaining);
        throwIndicator?.SetThrowsRemaining(_throwsRemaining);

        if (cameraController != null) cameraController.IsLockedForThrow = false;

        RefreshHUD();
    }

    private void HandleProjectileSettled()
    {
        if (_levelEnded) return;

        if (_throwsRemaining > 0)
        {
            playerController.PrepareNextThrow();
        }
        else { StartCoroutine(TriggerOutcome(false)); }
    }

    private void HandleObjectDestroyed(LevelObject obj)
    {
        if (_levelEnded) return;

        _totalScore += obj.ScoreValue;
        RefreshHUD();

        if (_targets.Contains(obj))
        {
            _targets.Remove(obj); 
            if(_targets.Count == 0)
            {
                StartCoroutine(TriggerOutcome(true));
            }
        }
    }
    private void HandleObjectDamaged(LevelObject obj, int damage)
    {
        // Hook for future effects (screen shake, particle flash, etc.)
        // Intentionally left minimal to keep scope compact.
    }
    public void AddTarget(LevelObject obj)
    {
        _targets.Add(obj);
    }
    private IEnumerator TriggerOutcome(bool victory)
    {
        if (_levelEnded) yield break;
        _levelEnded = true;

        // Disable further throws
        if (playerController != null)
            playerController.CanThrow = false;

        yield return new WaitForSeconds(outcomeDelay);

        LevelResultData result = new LevelResultData()
        {
            levelId = levelSettings.selectedLevel.name,
            rawScore = _totalScore, //This is before adding the bonus score from the extra javelins. 
            throwBonus = levelSettings.selectedLevel.throwBonusPerThrow,
            throwsRemaining = _throwsRemaining
        };

        int s = StarCalculator.Calculate(_totalScore, _throwsRemaining, levelSettings.selectedLevel, out _, out _totalScore);
        if (victory)
        {
            if (s > 0) { starOne.SetActive(true); }
            if (s > 1) { starTwo.SetActive(true); }
            if (s > 2) { starThree.SetActive(true); }

            result.stars = s;
            result.totalScore = _totalScore;
            GameEvents.LevelVictory(_totalScore);
            RefreshHUD();
            SetPanelActive(victoryPanel, true);
        }
        else
        {
            GameEvents.LevelFailed();
            SetPanelActive(failurePanel, true);
        }

        SaveManager.Instance?.SaveLevelResult(result);
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance?.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void GoToMainMenu()
    {
        SceneTransitionManager.Instance?.LoadScene(0);
    }
    public void GoToNextLevel()
    {
        levelSettings.selectedLevel = SaveManager.Instance.GetNextLevelConfig(levelSettings.selectedLevel.name, levelSettings);
        SceneTransitionManager.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void RefreshHUD()
    {
        if (scoreText != null) scoreText.text = _totalScore.ToString();
        if (levelText != null) levelText.text = levelSettings.selectedLevel.name;
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}