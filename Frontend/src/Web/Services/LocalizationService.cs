using System.Globalization;
using System.Text.Json;

namespace Web.Services;

/// <summary>
/// Manages application localization — supports English (en) and Indonesian (id).
/// Injected as Scoped. Components subscribe to OnLanguageChanged to re-render.
/// </summary>
public class LocalizationService
{
    private string _currentLanguage = "id";

    /// <summary>
    /// Current language code: "en" or "id".
    /// </summary>
    public string CurrentLanguage
    {
        get => _currentLanguage;
        private set => _currentLanguage = value;
    }

    /// <summary>
    /// Fired when the language changes — components subscribe to call StateHasChanged.
    /// </summary>
    public event Action? OnLanguageChanged;

    /// <summary>
    /// Switch language and notify subscribers.
    /// </summary>
    public void SetLanguage(string lang)
    {
        if (lang != "en" && lang != "id") return;
        if (_currentLanguage == lang) return;

        _currentLanguage = lang;
        CultureInfo.DefaultThreadCurrentUICulture = lang switch
        {
            "en" => new CultureInfo("en-US"),
            _ => new CultureInfo("id-ID")
        };

        OnLanguageChanged?.Invoke();
    }

    /// <summary>
    /// Translate a key to the current language.
    /// Returns the key itself if translation is not found.
    /// </summary>
    public string T(string key) => Translations.Get(key, _currentLanguage);

    /// <summary>
    /// Translate with format arguments (like string.Format).
    /// </summary>
    public string T(string key, params object[] args)
    {
        var template = Translations.Get(key, _currentLanguage);
        return string.Format(template, args);
    }
}

/// <summary>
/// Static dictionary of all translations — loaded from wwwroot/translations/*.json.
/// </summary>
public static class Translations
{
    private static Dictionary<string, string> _id = new();
    private static Dictionary<string, string> _en = new();
    private static bool _initialized = false;
    private static readonly object _lock = new();

    /// <summary>
    /// Load translations from JSON files. Call once at app startup.
    /// </summary>
    public static void Initialize(string idJsonPath, string enJsonPath)
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;

            _id = LoadFromFile(idJsonPath);
            _en = LoadFromFile(enJsonPath);

            _initialized = true;
        }
    }

    private static Dictionary<string, string> LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"[Translations] File not found: {path}");
            return new();
        }

        var json = File.ReadAllText(path);
        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        return result ?? new();
    }

    /// <summary>
    /// Get a translation for the given key and language code ("id" or "en").
    /// Falls back to Indonesian, then returns the key itself.
    /// </summary>
    public static string Get(string key, string lang)
    {
        var dict = lang == "en" ? _en : _id;

        if (dict.TryGetValue(key, out var value))
            return value;

        // Fallback: try the other language
        if (lang == "en" && _id.TryGetValue(key, out var idVal))
            return idVal;

        return key;
    }
}
