using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text levelNumberText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject[] starIcons = new GameObject[3];

    private string _levelId;
    private string _gameScene;

    public void Initialise(int levelNumber, string levelId, string gameScene,
                           bool isUnlocked, bool isCompleted, int bestScore, int bestStars)
    {
        _levelId = levelId;
        _gameScene = gameScene;

        if (levelNumberText != null)
            levelNumberText.text = levelNumber.ToString();

        for (int i = 0; i < starIcons.Length; i++)
            if (starIcons[i] != null) starIcons[i].SetActive(isCompleted && i < bestStars);

        if (bestScoreText != null)
            bestScoreText.text = isCompleted ? bestScore.ToString() : "";

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        if (button != null)
        {
            button.interactable = isUnlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }
    private void OnClick()
    {
        LevelLoader.SelectedLevelId = _levelId;
        SceneTransitionManager.Instance?.LoadScene(_gameScene);
    }
}