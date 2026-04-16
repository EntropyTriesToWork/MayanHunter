using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [Header("References")]
    [Tooltip("Full-screen Image used as the transition overlay.")]
    [SerializeField] private RectTransform transitionPanel;

    [Header("Animation")]
    [Tooltip("Seconds for the panel to slide in or out.")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float sceneSwitchDuration = 0.3f;

    [Tooltip("Animation curve applied to the slide (ease in/out recommended).")]
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool _isTransitioning;

    private void Start()
    {
        if (transitionPanel != null)
        {
            transitionPanel.gameObject.SetActive(true);
            SnapPanelOffScreen(fromRight: false);
            StartCoroutine(SlideOut());  
        }
    }
    public void LoadScene(string sceneName)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionTo(sceneName));
    }
    public void LoadScene(int buildIndex)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionTo(buildIndex));
    }
    private IEnumerator TransitionTo(string sceneName)
    {
        _isTransitioning = true;
        yield return StartCoroutine(SlideIn());
        yield return new WaitForSecondsRealtime(slideDuration);
        SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitForSecondsRealtime(sceneSwitchDuration);
        yield return StartCoroutine(SlideOut());
        _isTransitioning = false;
    }

    private IEnumerator TransitionTo(int buildIndex)
    {
        _isTransitioning = true;
        yield return StartCoroutine(SlideIn());
        yield return new WaitForSecondsRealtime(slideDuration);
        SceneManager.LoadSceneAsync(buildIndex);
        yield return new WaitForSecondsRealtime(sceneSwitchDuration);
        yield return StartCoroutine(SlideOut());
        _isTransitioning = false;
    }
    private IEnumerator SlideIn()
    {
        float screenW = GetScreenWidth();
        yield return Slide(startX: -screenW, endX: 0f);
    }
    private IEnumerator SlideOut()
    {
        float screenW = GetScreenWidth();
        yield return Slide(startX: 0f, endX: screenW);
    }

    private IEnumerator Slide(float startX, float endX)
    {
        if (transitionPanel == null) yield break;

        float elapsed = 0f;
        SetPanelX(startX);

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            SetPanelX(Mathf.LerpUnclamped(startX, endX, slideCurve.Evaluate(t)));
            yield return null;
        }

        SetPanelX(endX);
    }
    private void SnapPanelOffScreen(bool fromRight)
    {
        float x = fromRight ? GetScreenWidth() : 0f;
        SetPanelX(x);
    }

    private void SetPanelX(float x)
    {
        if (transitionPanel == null) return;
        Vector2 pos = transitionPanel.anchoredPosition;
        pos.x = x;
        transitionPanel.anchoredPosition = pos;
    }
    private float GetScreenWidth()
    {
        if (transitionPanel != null)
        {
            Canvas canvas = transitionPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                return canvasRect.rect.width;
            }
        }
        return Screen.width;
    }
}