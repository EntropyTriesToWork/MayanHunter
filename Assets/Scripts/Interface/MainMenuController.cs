using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

    [Header("Level Prefabs")]
    [SerializeField] private LevelSettings levelSettings;
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI")]
    [SerializeField] private LevelSelectButton levelButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private Transform leaderboardEntryContainer;
    [SerializeField] private LeaderboardEntryUI leaderboardEntryPrefab;
    [SerializeField] private CanvasGroup leaderBoardPanel;
    [SerializeField] private RectTransform leaderBoardListParent;

    [Header("Score Submission Panel")]
    [SerializeField] private GameObject submitPanel;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_Text submitScorePreviewText;

    [Header("Confirm Panels")]
    [SerializeField] private GameObject confirmClearLeaderboardPanel;
    [SerializeField] private GameObject confirmDeleteAllPanel;

    private void Start()
    {
        var levelIds = BuildLevelIdList();
        SaveManager.Instance?.InitialiseLevelList(levelIds);

        BuildLevelButtons();
        RefreshLeaderboard();

        SetPanelActive(submitPanel, false);
        SetPanelActive(confirmClearLeaderboardPanel, false);
        SetPanelActive(confirmDeleteAllPanel, false);
    }
    private List<string> BuildLevelIdList()
    {
        var ids = new List<string>();
        foreach (var prefab in levelSettings.allLevels)
            if (prefab != null) ids.Add(prefab.name);
        return ids;
    }
    private void BuildLevelButtons()
    {
        if (levelButtonContainer == null || levelButtonPrefab == null) return;

        foreach (Transform child in levelButtonContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < levelSettings.allLevels.Count; i++)
        {
            if (levelSettings.allLevels[i] == null) continue;

            string levelId = levelSettings.allLevels[i].name;
            LevelSaveEntry entry = SaveManager.Instance?.GetLevelEntry(levelId);

            LevelSelectButton btn = Instantiate(levelButtonPrefab, levelButtonContainer);
            btn.Initialise(
                levelNumber: i + 1,
                levelId: levelId,
                gameScene: gameSceneName,
                isUnlocked: entry?.isUnlocked ?? (i == 0),
                isCompleted: entry?.isCompleted ?? false,
                bestScore: entry?.bestScore ?? 0,
                bestStars: entry?.bestStars ?? 0
            );
        }
    }
    private void RefreshLeaderboard()
    {
        if (leaderboardEntryContainer == null || leaderboardEntryPrefab == null) return;

        foreach (Transform child in leaderboardEntryContainer)
            Destroy(child.gameObject);

        List<LeaderboardEntry> entries = SaveManager.Instance?.GetLeaderboard()
                                         ?? new List<LeaderboardEntry>();

        for (int i = 0; i < entries.Count; i++)
        {
            LeaderboardEntryUI row = Instantiate(leaderboardEntryPrefab, leaderboardEntryContainer);
            row.Populate(rank: i + 1, entry: entries[i]);
        }
    }
    public void OpenSubmitPanel()
    {
        int sessionScore = SaveManager.Instance?.SessionTotalScore ?? 0;

        if (submitScorePreviewText != null)
            submitScorePreviewText.text = sessionScore.ToString();

        if (playerNameInput != null)
            playerNameInput.text = "";

        SetPanelActive(submitPanel, true);
    }
    public void ConfirmSubmit()
    {
        string name = playerNameInput != null ? playerNameInput.text : "Player";
        SaveManager.Instance?.SubmitSessionToLeaderboard(name);
        CloseSubmitPanel();
        RefreshLeaderboard();
        BuildLevelButtons();
    }
    public void CloseSubmitPanel()
    {
        SetPanelActive(submitPanel, false);
    }
    public void ClearSessionProgress()
    {
        SaveManager.Instance?.ClearSessionProgress();
        BuildLevelButtons();
    }
    public void OpenConfirmClearLeaderboard()
        => SetPanelActive(confirmClearLeaderboardPanel, true);

    public void CloseConfirmClearLeaderboard()
        => SetPanelActive(confirmClearLeaderboardPanel, false);
    public void ConfirmClearLeaderboard()
    {
        SaveManager.Instance?.ClearLeaderboard();
        CloseConfirmClearLeaderboard();
        RefreshLeaderboard();
    }
    public void OpenConfirmDeleteAll()
        => SetPanelActive(confirmDeleteAllPanel, true);

    public void CloseConfirmDeleteAll()
        => SetPanelActive(confirmDeleteAllPanel, false);
    public void ConfirmDeleteAll()
    {
        SaveManager.Instance?.DeleteAllSaveData();

        var levelIds = BuildLevelIdList();
        SaveManager.Instance?.InitialiseLevelList(levelIds);

        CloseConfirmDeleteAll();
        BuildLevelButtons();
        RefreshLeaderboard();
        levelSettings.selectedLevel = levelSettings.allLevels[0];
    }
    private Vector2 lbHidingPos = new Vector3(780, 0);
    private Vector2 lbVisiblePos = new Vector3(480, 0);
    private float _progress = 0f;
    private float _duration = 0.2f;
    private Coroutine slideRoutine = null;
    public void ToggleLeaderboardVisibility()
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);
        float target = (_progress >= 0.5f) ? 0f : 1f;
        slideRoutine = StartCoroutine(SlideRoutine(target));

        IEnumerator SlideRoutine(float target)
        {
            Debug.Log(target);
            float start = _progress;
            float time = 0f;

            // Adjust duration based on remaining distance
            float adjustedDuration = _duration * Mathf.Abs(target - start);

            while (time < adjustedDuration)
            {
                time += Time.deltaTime;
                float t = time / adjustedDuration;

                // Smooth easing
                float eased = Mathf.SmoothStep(0f, 1f, t);

                _progress = Mathf.Lerp(start, target, eased);
                leaderBoardListParent.anchoredPosition = Vector2.Lerp(lbHidingPos, lbVisiblePos, _progress);
                leaderBoardPanel.alpha = eased;
                yield return null;
            }

            _progress = target;
            leaderBoardListParent.anchoredPosition = Vector2.Lerp(lbHidingPos, lbVisiblePos, _progress);
            if(_progress >= 0.5f) { leaderBoardPanel.interactable = true; }
            else { leaderBoardPanel.interactable = false; }
            slideRoutine = null;
        }
}
    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}