using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    [Tooltip("The localization key to look up in the active language file.")]
    [SerializeField] private string localizationKey;
    private TMP_Text _tmpText;
    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            UpdateText(); // Apply current language immediately on enable.
        }
        else
        {
            Debug.LogWarning($"[LocalizedText] '{gameObject.name}': LocalizationManager not found on enable.");
        }
    }

    private void Start()
    {
        if (LocalizationManager.Instance != null && _tmpText != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            UpdateText();
        }
    }

    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }
    public void SetKey(string newKey)
    {
        localizationKey = newKey;
        UpdateText();
    }
    public string Key => localizationKey;
    private void UpdateText()
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogError($"[LocalizedText] '{gameObject.name}': LocalizationManager instance is missing.");
            return;
        }

        if (string.IsNullOrEmpty(localizationKey))
        {
            Debug.LogWarning($"[LocalizedText] '{gameObject.name}': localizationKey is empty.");
            return;
        }

        _tmpText.text = LocalizationManager.Instance.GetText(localizationKey);
    }
}
