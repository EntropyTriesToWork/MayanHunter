using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject confirmExitPanel;

    private bool _isPaused;
    public bool IsPaused => _isPaused;

    private void Start()
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(confirmExitPanel, false);
    }

    private void Update()
    {
        // Keyboard / back-button support
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
                CloseOptions();
            else
                TogglePause();
        }
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        ApplyPauseState();
    }
    public void Resume()
    {
        _isPaused = false;
        SetPanelActive(optionsPanel, false);
        ApplyPauseState();
    }
    public void MainMenu()
    {
        SceneTransitionManager.Instance.LoadScene(0);
    }
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void OpenOptions()
    {
        SetPanelActive(pausePanel,   false);
        SetPanelActive(optionsPanel, true);
        SetPanelActive(confirmExitPanel, false);
    }
    public void CloseOptions()
    {
        SetPanelActive(optionsPanel, false);
        SetPanelActive(pausePanel,   true);
        SetPanelActive(confirmExitPanel, false);
    }

    /// <summary>
    /// Switch language — wire each language button's OnClick to this,
    /// or use the <see cref="LanguageButton"/> helper component instead.
    /// </summary>
    public void SetLanguage(string languageCode)
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLanguage(languageCode);
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // WebGL can't quit — optionally load a "thanks for playing" page
        Application.OpenURL("https://itch.io"); // replace with your itch page
#else
        Application.Quit();
#endif
    }
    private void ApplyPauseState()
    {
        //Time.timeScale = _isPaused ? 0f : 1f;
        SetPanelActive(pausePanel, _isPaused);

        if (!_isPaused)
            SetPanelActive(optionsPanel, false);

        GameEvents.PauseStateChanged(_isPaused);
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
