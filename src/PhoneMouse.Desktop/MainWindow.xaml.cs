using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PhoneMouse.Core.Input;
using PhoneMouse.Core.Windows;
using PhoneMouse.Desktop.Services;
using PhoneMouse.Server;
using PhoneMouse.Server.Network;
using PhoneMouse.Server.Notes;
using PhoneMouse.Server.Security;
using PhoneMouse.Server.Settings;

namespace PhoneMouse.Desktop;

public partial class MainWindow : Window
{
    private readonly IMouseController _mouseController;
    private readonly IKeyboardController _keyboardController;
    private readonly IForegroundWindowService _foregroundWindowService;
    private readonly VoiceNoteService _voiceNoteService;
    private readonly PairingService _pairingService;
    private readonly TrustedDeviceStore _trustedDeviceStore;
    private readonly ControlSettingsService _controlSettingsService;
    private readonly ServerHost _serverHost;

    private string _phoneUrl =
        string.Empty;

    private bool _settingsUiReady;


    public MainWindow()
    {
        InitializeComponent();

        _mouseController =
            new WindowsMouseController();

        _keyboardController =
            new WindowsKeyboardController();

        _foregroundWindowService =
            new WindowsForegroundWindowService();

        _voiceNoteService =
            new VoiceNoteService();

        _pairingService =
            new PairingService();

        _trustedDeviceStore =
            new TrustedDeviceStore();

        _controlSettingsService =
            new ControlSettingsService();

        _serverHost =
            new ServerHost(
                _mouseController,
                _keyboardController,
                _foregroundWindowService,
                _voiceNoteService,
                _pairingService,
                _trustedDeviceStore,
                _controlSettingsService);

        _serverHost.ConnectedClientCountChanged +=
            ServerHost_ConnectedClientCountChanged;

        _pairingService.PairingTokenChanged +=
            PairingService_PairingTokenChanged;

        _trustedDeviceStore.DevicesChanged +=
            TrustedDeviceStore_DevicesChanged;

        _controlSettingsService.SettingsChanged +=
            ControlSettingsService_SettingsChanged;

        SyncSettingsUi(
            _controlSettingsService
                .GetSnapshot());

        _settingsUiReady =
            true;

        Loaded +=
            MainWindow_Loaded;

        Closed +=
            MainWindow_Closed;
    }


    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            ServerStatusText.Text =
                "● 正在启动";

            await _serverHost.StartAsync();

            _phoneUrl =
                NetworkAddressService
                    .GetPhoneUrl(
                        9527);

            PhoneUrlText.Text =
                _phoneUrl;

            RefreshPairingQrCode();

            RefreshDeviceList();

            ConnectedClientsText.Text =
                _serverHost
                    .ConnectedClientCount
                    .ToString();

            ServerStatusText.Text =
                "● 服务已启动";

            Title =
                "Phone Mouse";
        }
        catch (Exception ex)
        {
            ServerStatusText.Text =
                "● 启动失败";

            MessageBox.Show(
                ex.ToString(),
                "Phone Mouse 服务启动失败",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }


    // =====================================================
    // 控制体验设置
    // =====================================================

    private void SyncSettingsUi(
        ControlSettingsSnapshot settings)
    {
        bool previousReady =
            _settingsUiReady;

        _settingsUiReady =
            false;

        MouseSensitivitySlider.Value =
            settings.MouseSensitivity;

        ScrollSpeedSlider.Value =
            settings.ScrollSpeed;

        NaturalScrollingCheckBox.IsChecked =
            settings.NaturalScrolling;

        LongPressSlider.Value =
            settings.LongPressMs;

        WeChatSendModeComboBox.SelectedIndex =
            settings.WeChatSendMode ==
                1
                ? 1
                : 0;

        MouseSensitivityValueText.Text =
            $"{settings.MouseSensitivity:0.0}×";

        ScrollSpeedValueText.Text =
            $"{settings.ScrollSpeed:0.0}×";

        LongPressValueText.Text =
            $"{settings.LongPressMs} ms";

        _settingsUiReady =
            previousReady;
    }


    private void ControlSetting_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_settingsUiReady)
        {
            return;
        }

        SaveControlSettingsFromUi();
    }


    private void NaturalScrollingCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        if (!_settingsUiReady)
        {
            return;
        }

        SaveControlSettingsFromUi();
    }


    private void WeChatSendModeComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!_settingsUiReady)
        {
            return;
        }


        SaveControlSettingsFromUi();
    }


    private void SaveControlSettingsFromUi()
    {
        double mouseSensitivity =
            Math.Round(
                MouseSensitivitySlider.Value,
                1);

        double scrollSpeed =
            Math.Round(
                ScrollSpeedSlider.Value,
                1);

        bool naturalScrolling =
            NaturalScrollingCheckBox.IsChecked ==
                true;

        int longPressMs =
            (int)Math.Round(
                LongPressSlider.Value);

        int weChatSendMode =
            WeChatSendModeComboBox.SelectedIndex ==
                1
                ? 1
                : 0;

        MouseSensitivityValueText.Text =
            $"{mouseSensitivity:0.0}×";

        ScrollSpeedValueText.Text =
            $"{scrollSpeed:0.0}×";

        LongPressValueText.Text =
            $"{longPressMs} ms";

        _controlSettingsService.Update(
            mouseSensitivity,
            scrollSpeed,
            naturalScrolling,
            longPressMs,
            weChatSendMode);
    }


    private void ResetControlSettingsButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _controlSettingsService
            .ResetDefaults();
    }


    private void ControlSettingsService_SettingsChanged(
        object? sender,
        ControlSettingsSnapshot settings)
    {
        Dispatcher.Invoke(
            () =>
            {
                SyncSettingsUi(
                    settings);
            });
    }


    // =====================================================
    // 配对二维码
    // =====================================================

    private void RefreshPairingQrCode()
    {
        if (
            string.IsNullOrWhiteSpace(
                _phoneUrl))
        {
            return;
        }

        string pairingUrl =
            $"{_phoneUrl}/?pair={Uri.EscapeDataString(_pairingService.CurrentPairingToken)}";

        QrCodeImage.Source =
            QrCodeService.Create(
                pairingUrl,
                pixelsPerModule: 5,
                quietZone: 4);
    }


    private void RefreshQrButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _pairingService
            .RegeneratePairingToken();

        PairingHintText.Text =
            "已刷新 · 上一个二维码立即失效";
    }


    private void PairingService_PairingTokenChanged(
        object? sender,
        string newToken)
    {
        Dispatcher.Invoke(
            () =>
            {
                RefreshPairingQrCode();

                PairingHintText.Text =
                    "已生成新的安全配对二维码";
            });
    }


    // =====================================================
    // 设备管理
    // =====================================================

    private void RefreshDeviceList()
    {
        IReadOnlyList<TrustedDeviceInfo> devices =
            _trustedDeviceStore
                .GetDevices();

        TrustedDevicesText.Text =
            devices.Count
                .ToString();

        TrustedDevicesPanel
            .Children
            .Clear();

        NoDevicesText.Visibility =
            devices.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        RemoveAllDevicesButton.IsEnabled =
            devices.Count > 0;

        foreach (
            TrustedDeviceInfo device
            in devices)
        {
            TrustedDevicesPanel
                .Children
                .Add(
                    CreateDeviceRow(
                        device));
        }
    }


    private UIElement CreateDeviceRow(
        TrustedDeviceInfo device)
    {
        bool isOnline =
            _serverHost
                .IsDeviceConnected(
                    device.Id);

        Border container =
            new()
            {
                Padding =
                    new Thickness(
                        0,
                        15,
                        0,
                        15),

                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            238,
                            238,
                            238)),

                BorderThickness =
                    new Thickness(
                        0,
                        0,
                        0,
                        1)
            };

        Grid grid =
            new();

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    new GridLength(
                        1,
                        GridUnitType.Star)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width =
                    GridLength.Auto
            });

        StackPanel information =
            new();

        StackPanel titleLine =
            new()
            {
                Orientation =
                    Orientation.Horizontal
            };

        TextBlock name =
            new()
            {
                Text =
                    device.Name,

                FontSize =
                    14,

                FontWeight =
                    FontWeights.SemiBold,

                Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            32,
                            33,
                            36))
            };

        Border stateBadge =
            new()
            {
                Margin =
                    new Thickness(
                        9,
                        0,
                        0,
                        0),

                Padding =
                    new Thickness(
                        7,
                        2,
                        7,
                        2),

                CornerRadius =
                    new CornerRadius(
                        8),

                Background =
                    new SolidColorBrush(
                        isOnline
                            ? Color.FromRgb(
                                233,
                                247,
                                239)
                            : Color.FromRgb(
                                242,
                                243,
                                245))
            };

        stateBadge.Child =
            new TextBlock
            {
                Text =
                    isOnline
                        ? "在线"
                        : "离线",

                FontSize =
                    11,

                Foreground =
                    new SolidColorBrush(
                        isOnline
                            ? Color.FromRgb(
                                24,
                                128,
                                56)
                            : Color.FromRgb(
                                117,
                                117,
                                117))
            };

        titleLine.Children.Add(
            name);

        titleLine.Children.Add(
            stateBadge);

        TextBlock lastSeen =
            new()
            {
                Margin =
                    new Thickness(
                        0,
                        6,
                        0,
                        0),

                Text =
                    $"最后连接：{device.LastSeenUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}",

                FontSize =
                    12,

                Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            119,
                            119,
                            119))
            };

        TextBlock deviceId =
            new()
            {
                Margin =
                    new Thickness(
                        0,
                        4,
                        0,
                        0),

                Text =
                    $"设备 ID：{device.Id[..Math.Min(12, device.Id.Length)]}",

                FontSize =
                    11,

                Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            153,
                            153,
                            153))
            };

        information.Children.Add(
            titleLine);

        information.Children.Add(
            lastSeen);

        information.Children.Add(
            deviceId);

        Button removeButton =
            new()
            {
                Content =
                    "移除",

                Tag =
                    device.Id,

                Padding =
                    new Thickness(
                        14,
                        7,
                        14,
                        7),

                Margin =
                    new Thickness(
                        16,
                        0,
                        0,
                        0),

                VerticalAlignment =
                    VerticalAlignment.Center
            };

        removeButton.Click +=
            RemoveDeviceButton_Click;

        Grid.SetColumn(
            information,
            0);

        Grid.SetColumn(
            removeButton,
            1);

        grid.Children.Add(
            information);

        grid.Children.Add(
            removeButton);

        container.Child =
            grid;

        return
            container;
    }


    private void RemoveDeviceButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (
            sender is not Button button ||
            button.Tag is not string deviceId)
        {
            return;
        }

        MessageBoxResult result =
            MessageBox.Show(
                "确定解除这台设备的配对吗？\n\n如果设备当前在线，它会立即失去控制权限。",
                "解除设备配对",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (
            result !=
                MessageBoxResult.Yes)
        {
            return;
        }

        _trustedDeviceStore
            .RemoveDevice(
                deviceId);
    }


    private void RemoveAllDevicesButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (
            _trustedDeviceStore.Count ==
                0)
        {
            return;
        }

        MessageBoxResult result =
            MessageBox.Show(
                "确定解除所有设备的配对吗？\n\n所有已连接手机都会立即失去控制权限，需要重新扫码授权。",
                "全部解除配对",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

        if (
            result !=
                MessageBoxResult.Yes)
        {
            return;
        }

        int removed =
            _trustedDeviceStore
                .RemoveAllDevices();

        _pairingService
            .RegeneratePairingToken();

        MessageBox.Show(
            $"已解除 {removed} 台设备的配对。",
            "Phone Mouse",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }


    private void TrustedDeviceStore_DevicesChanged(
        object? sender,
        EventArgs e)
    {
        Dispatcher.Invoke(
            RefreshDeviceList);
    }


    private void ServerHost_ConnectedClientCountChanged(
        object? sender,
        int count)
    {
        Dispatcher.Invoke(
            () =>
            {
                ConnectedClientsText.Text =
                    count.ToString();

                RefreshDeviceList();
            });
    }


    // =====================================================
    // 普通访问地址
    // =====================================================

    private void CopyUrlButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (
            string.IsNullOrWhiteSpace(
                _phoneUrl))
        {
            return;
        }

        Clipboard.SetText(
            _phoneUrl);

        CopyUrlButton.Content =
            "已复制";

        Task.Delay(
                1200)
            .ContinueWith(
                _ =>
                {
                    Dispatcher.Invoke(
                        () =>
                        {
                            CopyUrlButton.Content =
                                "复制普通访问地址";
                        });
                });
    }


    private async void MainWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _serverHost.ConnectedClientCountChanged -=
            ServerHost_ConnectedClientCountChanged;

        _pairingService.PairingTokenChanged -=
            PairingService_PairingTokenChanged;

        _trustedDeviceStore.DevicesChanged -=
            TrustedDeviceStore_DevicesChanged;

        _controlSettingsService.SettingsChanged -=
            ControlSettingsService_SettingsChanged;

        await _serverHost.DisposeAsync();
    }
}
