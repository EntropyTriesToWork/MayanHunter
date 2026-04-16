using System;
using System.Collections.Generic;
using UnityEngine;
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSavedLanguage();
    }
    [Tooltip("All language TextAssets to load on startup. Each file must follow the KEY=VALUE format.")]
    [SerializeField] private LanguageAsset[] languageAssets;
    private const string PlayerPrefsKey = "SelectedLanguage";
    private readonly Dictionary<string, Dictionary<string, string>> _allLanguages 
        = new Dictionary<string, Dictionary<string, string>>();

    private string _currentLanguage;
    public string CurrentLanguage => _currentLanguage;

    public event Action OnLanguageChanged;

    private void Start()
    {
        foreach (var asset in languageAssets)
        {
            if (asset == null || asset.textAsset == null)
            {
                Debug.LogWarning("[LocalizationManager] Skipping null LanguageAsset entry.");
                continue;
            }

            var entries = ParseLanguageFile(asset.textAsset);
            _allLanguages[asset.languageCode] = entries;
            Debug.Log($"[LocalizationManager] Loaded language '{asset.languageCode}' with {entries.Count} keys.");
        }

        RefreshAllTexts();
    }
    public void SetLanguage(string languageCode)
    {
        if (!_allLanguages.ContainsKey(languageCode))
        {
            Debug.LogError($"[LocalizationManager] Language '{languageCode}' is not loaded.");
            return;
        }

        _currentLanguage = languageCode;
        PlayerPrefs.SetString(PlayerPrefsKey, languageCode);
        PlayerPrefs.Save();

        RefreshAllTexts();
    }
    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(_currentLanguage))
        {
            Debug.LogWarning("[LocalizationManager] No language set yet.");
            return key;
        }

        if (_allLanguages.TryGetValue(_currentLanguage, out var dict))
        {
            if (dict.TryGetValue(key, out var value))
                return value;

            Debug.LogWarning($"[LocalizationManager] Key '{key}' not found in language '{_currentLanguage}'.");
        }
        else
        {
            Debug.LogWarning($"[LocalizationManager] Language '{_currentLanguage}' dictionary not found.");
        }

        return key; // graceful fallback
    }
    public void RefreshAllTexts()
    {
        OnLanguageChanged?.Invoke();
    }
    private void LoadSavedLanguage()
    {
        string saved = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

        if (!string.IsNullOrEmpty(saved))
        {
            _currentLanguage = saved;
        }
        else if (languageAssets != null && languageAssets.Length > 0 && languageAssets[0] != null)
        {
            _currentLanguage = languageAssets[0].languageCode;
        }
        else
        {
            _currentLanguage = "en";
        }
    }
    private Dictionary<string, string> ParseLanguageFile(TextAsset textAsset)
    {
        var dict = new Dictionary<string, string>();

        string[] words = TextAssetToWordArray(textAsset);

        foreach (string word in words)
        {
            int separatorIndex = word.IndexOf('=');

            if (separatorIndex <= 0)
            {
                // Skip blank lines, comments (#), or malformed tokens
                //if (!word.StartsWith("#")) Debug.LogWarning($"[LocalizationManager] Skipping malformed token: '{word}'");
                continue;
            }

            string key   = word.Substring(0, separatorIndex).Trim();
            string value = word.Substring(separatorIndex + 1);

            // Convert underscores → spaces and literal \n → newline
            value = value.Replace('_', ' ').Replace("\\n", "\n");

            if (dict.ContainsKey(key))
                Debug.LogWarning($"[LocalizationManager] Duplicate key '{key}' — overwriting.");

            dict[key] = value;
        }

        return dict;
    }
    public static string[] TextAssetToWordArray(TextAsset textAsset)
    {
        if (textAsset == null)
        {
            Debug.LogError("[BlondieUtils] TextAssetToWordArray: TextAsset is null.");
            return Array.Empty<string>();
        }
        return textAsset.text.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
[Serializable]
public class LanguageAsset
{
    [Tooltip("Short language code, e.g. 'en', 'zh', 'ja'")]
    public string languageCode;

    [Tooltip("The .txt TextAsset containing KEY=VALUE pairs for this language")]
    public TextAsset textAsset;
}
