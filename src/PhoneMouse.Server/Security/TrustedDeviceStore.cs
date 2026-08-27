using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PhoneMouse.Server.Security;

/// <summary>
/// 已配对设备的本地持久化存储。
///
/// - 浏览器保存原始 Device Token
/// - 电脑仅保存 SHA-256 哈希
/// - 支持读取、单独移除、全部解除配对
/// </summary>
public sealed class TrustedDeviceStore
{
    private readonly object _gate =
        new();


    private readonly string
        _filePath;


    private readonly JsonSerializerOptions
        _jsonOptions =
            new()
            {
                WriteIndented =
                    true
            };


    private List<TrustedDeviceRecord>
        _devices =
            new();


    public TrustedDeviceStore()
    {
        string directory =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment
                        .SpecialFolder
                        .LocalApplicationData),
                "PhoneMouse");


        Directory.CreateDirectory(
            directory);


        _filePath =
            Path.Combine(
                directory,
                "trusted-devices.json");


        Load();
    }


    public event EventHandler?
        DevicesChanged;


    public event EventHandler<string>?
        DeviceRevoked;


    public int Count
    {
        get
        {
            lock (_gate)
            {
                return
                    _devices.Count;
            }
        }
    }


    public IReadOnlyList<TrustedDeviceInfo>
        GetDevices()
    {
        lock (_gate)
        {
            return _devices
                .OrderByDescending(
                    x =>
                        x.LastSeenUtc)
                .Select(
                    x =>
                        new TrustedDeviceInfo(
                            x.Id,
                            x.Name,
                            x.CreatedAtUtc,
                            x.LastSeenUtc))
                .ToArray();
        }
    }


    public IssuedDeviceToken IssueDeviceToken(
        string? deviceName)
    {
        string rawToken =
            GenerateToken(
                32);


        string tokenHash =
            HashToken(
                rawToken);


        DateTimeOffset now =
            DateTimeOffset.UtcNow;


        TrustedDeviceRecord record =
            new()
            {
                Id =
                    Guid.NewGuid()
                        .ToString(
                            "N"),

                Name =
                    NormalizeDeviceName(
                        deviceName),

                TokenHash =
                    tokenHash,

                CreatedAtUtc =
                    now,

                LastSeenUtc =
                    now
            };


        lock (_gate)
        {
            _devices.Add(
                record);


            SaveLocked();
        }


        DevicesChanged?
            .Invoke(
                this,
                EventArgs.Empty);


        return
            new IssuedDeviceToken(
                record.Id,
                rawToken);
    }


    public bool TryValidateAndTouch(
        string? rawToken,
        out string? deviceId)
    {
        deviceId =
            null;


        if (
            string.IsNullOrWhiteSpace(
                rawToken))
        {
            return false;
        }


        string hash =
            HashToken(
                rawToken);


        TrustedDeviceRecord?
            matchedDevice;


        lock (_gate)
        {
            matchedDevice =
                _devices
                    .FirstOrDefault(
                        x =>
                            FixedTimeEqualsHex(
                                x.TokenHash,
                                hash));


            if (
                matchedDevice is null)
            {
                return false;
            }


            matchedDevice.LastSeenUtc =
                DateTimeOffset.UtcNow;


            deviceId =
                matchedDevice.Id;


            SaveLocked();
        }


        DevicesChanged?
            .Invoke(
                this,
                EventArgs.Empty);


        return true;
    }


    public bool RemoveDevice(
        string deviceId)
    {
        bool removed;


        lock (_gate)
        {
            removed =
                _devices
                    .RemoveAll(
                        x =>
                            string.Equals(
                                x.Id,
                                deviceId,
                                StringComparison.Ordinal))
                > 0;


            if (removed)
            {
                SaveLocked();
            }
        }


        if (!removed)
        {
            return false;
        }


        DeviceRevoked?
            .Invoke(
                this,
                deviceId);


        DevicesChanged?
            .Invoke(
                this,
                EventArgs.Empty);


        return true;
    }


    public int RemoveAllDevices()
    {
        string[] ids;


        lock (_gate)
        {
            ids =
                _devices
                    .Select(
                        x =>
                            x.Id)
                    .ToArray();


            if (
                ids.Length == 0)
            {
                return 0;
            }


            _devices.Clear();


            SaveLocked();
        }


        foreach (
            string id
            in ids)
        {
            DeviceRevoked?
                .Invoke(
                    this,
                    id);
        }


        DevicesChanged?
            .Invoke(
                this,
                EventArgs.Empty);


        return
            ids.Length;
    }


    private void Load()
    {
        lock (_gate)
        {
            if (
                !File.Exists(
                    _filePath))
            {
                _devices =
                    new();

                return;
            }


            try
            {
                string json =
                    File.ReadAllText(
                        _filePath);


                _devices =
                    JsonSerializer
                        .Deserialize<
                            List<
                                TrustedDeviceRecord>>(
                            json,
                            _jsonOptions)
                    ?? new();
            }
            catch
            {
                _devices =
                    new();
            }
        }
    }


    private void SaveLocked()
    {
        string json =
            JsonSerializer
                .Serialize(
                    _devices,
                    _jsonOptions);


        string tempPath =
            _filePath +
            ".tmp";


        File.WriteAllText(
            tempPath,
            json);


        File.Move(
            tempPath,
            _filePath,
            true);
    }


    private static string NormalizeDeviceName(
        string? value)
    {
        string name =
            string.IsNullOrWhiteSpace(
                value)
                ? "Mobile Browser"
                : value.Trim();


        if (
            name.Length >
                80)
        {
            name =
                name[..80];
        }


        return name;
    }


    private static string GenerateToken(
        int byteLength)
    {
        byte[] bytes =
            RandomNumberGenerator
                .GetBytes(
                    byteLength);


        return Convert
            .ToBase64String(
                bytes)
            .TrimEnd('=')
            .Replace(
                '+',
                '-')
            .Replace(
                '/',
                '_');
    }


    private static string HashToken(
        string rawToken)
    {
        byte[] bytes =
            Encoding.UTF8
                .GetBytes(
                    rawToken);


        byte[] hash =
            SHA256.HashData(
                bytes);


        return Convert
            .ToHexString(
                hash);
    }


    private static bool FixedTimeEqualsHex(
        string left,
        string right)
    {
        try
        {
            byte[] leftBytes =
                Convert.FromHexString(
                    left);


            byte[] rightBytes =
                Convert.FromHexString(
                    right);


            if (
                leftBytes.Length !=
                    rightBytes.Length)
            {
                return false;
            }


            return CryptographicOperations
                .FixedTimeEquals(
                    leftBytes,
                    rightBytes);
        }
        catch
        {
            return false;
        }
    }


    private sealed class TrustedDeviceRecord
    {
        public string Id
        {
            get;
            set;
        } =
            string.Empty;


        public string Name
        {
            get;
            set;
        } =
            string.Empty;


        public string TokenHash
        {
            get;
            set;
        } =
            string.Empty;


        public DateTimeOffset CreatedAtUtc
        {
            get;
            set;
        }


        public DateTimeOffset LastSeenUtc
        {
            get;
            set;
        }
    }
}


public sealed record TrustedDeviceInfo(
    string Id,
    string Name,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastSeenUtc);


public sealed record IssuedDeviceToken(
    string DeviceId,
    string Token);
