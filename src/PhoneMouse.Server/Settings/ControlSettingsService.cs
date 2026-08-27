using System.Text.Json;

namespace PhoneMouse.Server.Settings;

/// <summary>
/// Phone Mouse 控制体验设置。
///
/// 设置保存在：
/// %LOCALAPPDATA%\PhoneMouse\control-settings.json
/// </summary>
public sealed class ControlSettingsService
{
    private readonly object _gate = new();

    private readonly string _filePath;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            WriteIndented = true
        };

    private ControlSettingsSnapshot _settings;


    public ControlSettingsService()
    {
        string directory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "PhoneMouse");

        Directory.CreateDirectory(directory);

        _filePath =
            Path.Combine(
                directory,
                "control-settings.json");

        _settings =
            Load();
    }


    public event EventHandler<ControlSettingsSnapshot>?
        SettingsChanged;


    public ControlSettingsSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            return _settings;
        }
    }


    public void Update(
        double mouseSensitivity,
        double scrollSpeed,
        bool naturalScrolling,
        int longPressMs,
        int weChatSendMode)
    {
        ControlSettingsSnapshot normalized =
            Normalize(
                new ControlSettingsSnapshot(
                    mouseSensitivity,
                    scrollSpeed,
                    naturalScrolling,
                    longPressMs,
                    weChatSendMode));

        bool changed;

        lock (_gate)
        {
            changed =
                !_settings.Equals(
                    normalized);

            if (!changed)
            {
                return;
            }

            _settings =
                normalized;

            SaveLocked();
        }

        SettingsChanged?
            .Invoke(
                this,
                normalized);
    }


    public void ResetDefaults()
    {
        ControlSettingsSnapshot defaults =
            ControlSettingsSnapshot.Default;

        lock (_gate)
        {
            _settings =
                defaults;

            SaveLocked();
        }

        SettingsChanged?
            .Invoke(
                this,
                defaults);
    }


    private ControlSettingsSnapshot Load()
    {
        if (!File.Exists(_filePath))
        {
            return
                ControlSettingsSnapshot.Default;
        }

        try
        {
            string json =
                File.ReadAllText(
                    _filePath);

            ControlSettingsSnapshot? loaded =
                JsonSerializer.Deserialize<ControlSettingsSnapshot>(
                    json,
                    _jsonOptions);

            return
                loaded is null
                    ? ControlSettingsSnapshot.Default
                    : Normalize(loaded);
        }
        catch
        {
            return
                ControlSettingsSnapshot.Default;
        }
    }


    private void SaveLocked()
    {
        string json =
            JsonSerializer.Serialize(
                _settings,
                _jsonOptions);

        string tempPath =
            _filePath + ".tmp";

        File.WriteAllText(
            tempPath,
            json);

        File.Move(
            tempPath,
            _filePath,
            true);
    }


    private static ControlSettingsSnapshot Normalize(
        ControlSettingsSnapshot value)
    {
        double mouseSensitivity =
            Math.Clamp(
                Math.Round(
                    value.MouseSensitivity,
                    1),
                0.5,
                3.0);

        double scrollSpeed =
            Math.Clamp(
                Math.Round(
                    value.ScrollSpeed,
                    1),
                0.5,
                3.0);

        int longPressMs =
            Math.Clamp(
                value.LongPressMs,
                250,
                900);

        int weChatSendMode =
            value.WeChatSendMode ==
                1
                ? 1
                : 0;

        return
            new ControlSettingsSnapshot(
                mouseSensitivity,
                scrollSpeed,
                value.NaturalScrolling,
                longPressMs,
                weChatSendMode);
    }
}


public sealed record ControlSettingsSnapshot(
    double MouseSensitivity,
    double ScrollSpeed,
    bool NaturalScrolling,
    int LongPressMs,
    int WeChatSendMode)
{
    public static ControlSettingsSnapshot Default =>
        new(
            MouseSensitivity: 1.0,
            ScrollSpeed: 1.0,
            NaturalScrolling: false,
            LongPressMs: 420,
            WeChatSendMode: 0);
}
