using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tiny helper — attach to a language-select Button in the Options panel.
/// Set <see cref="languageCode"/> in the Inspector (e.g. "en", "zh_cn", "zh_tw").
/// The button's OnClick wires automatically in Awake — no manual wiring needed.
/// </summary>
[RequireComponent(typeof(Button))]
public class LanguageButton : MonoBehaviour
{
    [Tooltip("The language code this button switches to (e.g. 'en', 'zh_cn', 'zh_tw').")]
    [SerializeField] private string languageCode;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetLanguage(languageCode);
    }
}
