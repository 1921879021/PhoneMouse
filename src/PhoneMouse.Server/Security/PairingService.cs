using System.Security.Cryptography;
using System.Text;

namespace PhoneMouse.Server.Security;

/// <summary>
/// 管理一次性配对 Token。
///
/// - 程序启动时生成随机 Token
/// - 成功配对一次后立即刷新
/// - 电脑端也可以手动刷新二维码
/// </summary>
public sealed class PairingService
{
    private readonly object _gate =
        new();


    private string
        _currentPairingToken;


    public PairingService()
    {
        _currentPairingToken =
            GenerateToken(
                24);
    }


    public event EventHandler<string>?
        PairingTokenChanged;


    public string CurrentPairingToken
    {
        get
        {
            lock (_gate)
            {
                return
                    _currentPairingToken;
            }
        }
    }


    public bool TryConsumePairingToken(
        string? candidate)
    {
        if (
            string.IsNullOrWhiteSpace(
                candidate))
        {
            return false;
        }


        string newToken;


        lock (_gate)
        {
            if (
                !FixedTimeEquals(
                    candidate,
                    _currentPairingToken))
            {
                return false;
            }


            _currentPairingToken =
                GenerateToken(
                    24);


            newToken =
                _currentPairingToken;
        }


        PairingTokenChanged?
            .Invoke(
                this,
                newToken);


        return true;
    }


    public string RegeneratePairingToken()
    {
        string newToken;


        lock (_gate)
        {
            _currentPairingToken =
                GenerateToken(
                    24);


            newToken =
                _currentPairingToken;
        }


        PairingTokenChanged?
            .Invoke(
                this,
                newToken);


        return newToken;
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


    private static bool FixedTimeEquals(
        string left,
        string right)
    {
        byte[] leftBytes =
            Encoding.UTF8
                .GetBytes(
                    left);


        byte[] rightBytes =
            Encoding.UTF8
                .GetBytes(
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
}
