using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace MihomoDashboard;

public sealed class MainForm : Form
{
    private readonly AppSettings _settings;
    private readonly MihomoManager _mihomo = new();
    private readonly DashboardServer _dashboardServer;
    private readonly Uri _dashboardUri;
    private readonly NotifyIcon _trayIcon;
    private readonly WebView2 _webView = new();
    private bool _allowClose;
    private bool _initialized;
    private bool _startMinimized;

    public MainForm(bool startMinimized)
    {
        _startMinimized = startMinimized;
        _settings = AppSettings.Load();
        _dashboardServer = new DashboardServer(Path.Combine(AppContext.BaseDirectory, "resources", "dashboard"));
        _dashboardUri = _dashboardServer.Start();

        Text = "Mihomo Dashboard";
        MinimumSize = new Size(1120, 720);
        Size = new Size(1360, 840);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = SystemIcons.Application;

        _trayIcon = CreateTrayIcon();
        BuildLayout();
        BindEvents();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await InitializeWebViewAsync();
        RefreshStatus();

        if (_settings.StartCoreOnLaunch)
        {
            StartCore();
        }

        if (_startMinimized)
        {
            HideToTray();
        }
    }

    private void BuildLayout()
    {
        _webView.Dock = DockStyle.Fill;
        Controls.Add(_webView);
    }

    private void BindEvents()
    {
        _mihomo.StatusChanged += (_, _) => BeginInvoke(new Action(RefreshStatus));
        _mihomo.LogReceived += (_, _) => BeginInvoke(new Action(SendStateToDashboard));
    }

    private NotifyIcon CreateTrayIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("显示窗口", null, (_, _) => ShowFromTray());
        menu.Items.Add("启动内核", null, (_, _) => StartCore());
        menu.Items.Add("停止内核", null, (_, _) => StopCore());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => ExitApplication());

        var icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Mihomo Dashboard",
            Visible = true,
            ContextMenuStrip = menu
        };
        icon.DoubleClick += (_, _) => ShowFromTray();
        return icon;
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            _webView.CoreWebView2.NavigationCompleted += async (_, _) =>
            {
                await InjectCorePanelAsync();
                SendStateToDashboard();
            };
            LoadDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"WebView2 初始化失败：{ex.Message}", "缺少 WebView2 Runtime", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadDashboard()
    {
        if (_webView.CoreWebView2 is null)
        {
            return;
        }

        var apiUrl = Uri.EscapeDataString(_settings.DashboardApiUrl.TrimEnd('/'));
        var secret = Uri.EscapeDataString(_settings.Secret);
        var uri = new Uri(_dashboardUri, $"?hostname={apiUrl}&secret={secret}");
        _webView.CoreWebView2.Navigate(uri.ToString());
    }

    private async Task InjectCorePanelAsync()
    {
        if (_webView.CoreWebView2 is null)
        {
            return;
        }

        const string script = """
(() => {
  if (window.__mihomoControlInstalled) return;
  window.__mihomoControlInstalled = true;

  const css = document.createElement('style');
  css.textContent = `
    #mihomo-core-widget {
      position: fixed;
      top: 12px;
      right: 86px;
      z-index: 2147483647;
      font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
      color: #171717;
    }
    #mihomo-core-widget * { box-sizing: border-box; }
    #mihomo-core-widget .mc-pill {
      height: 36px;
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 0 10px 0 14px;
      border-radius: 12px;
      border: 1px solid rgba(15, 23, 42, .08);
      background: rgba(255, 255, 255, .88);
      box-shadow: 0 12px 30px rgba(15, 23, 42, .10);
      backdrop-filter: blur(16px);
    }
    #mihomo-core-widget .mc-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: #f97316;
      box-shadow: 0 0 0 3px rgba(249, 115, 22, .16);
    }
    #mihomo-core-widget[data-running="true"] .mc-dot {
      background: #22c55e;
      box-shadow: 0 0 0 3px rgba(34, 197, 94, .16);
    }
    #mihomo-core-widget .mc-title {
      font-size: 13px;
      font-weight: 700;
      white-space: nowrap;
    }
    #mihomo-core-widget .mc-sub {
      color: #737373;
      font-size: 12px;
      white-space: nowrap;
    }
    #mihomo-core-widget button {
      height: 28px;
      border: 0;
      border-radius: 9px;
      padding: 0 11px;
      font: inherit;
      font-size: 12px;
      font-weight: 700;
      cursor: pointer;
      background: #18181b;
      color: #fff;
    }
    #mihomo-core-widget button.secondary {
      background: #f4f4f5;
      color: #27272a;
    }
    #mihomo-core-widget button.danger {
      background: #fff7ed;
      color: #c2410c;
    }
    #mihomo-core-widget button.icon {
      width: 28px;
      padding: 0;
      display: grid;
      place-items: center;
      font-size: 16px;
      background: transparent;
      color: #52525b;
    }
    #mihomo-core-widget .mc-panel {
      display: none;
      width: min(420px, calc(100vw - 32px));
      margin-top: 10px;
      margin-left: auto;
      border-radius: 16px;
      border: 1px solid rgba(15, 23, 42, .08);
      background: rgba(255, 255, 255, .96);
      box-shadow: 0 18px 55px rgba(15, 23, 42, .18);
      backdrop-filter: blur(18px);
      overflow: hidden;
    }
    #mihomo-core-widget[data-open="true"] .mc-panel { display: block; }
    #mihomo-core-widget .mc-section {
      padding: 14px;
      border-top: 1px solid #f1f5f9;
    }
    #mihomo-core-widget .mc-section:first-child { border-top: 0; }
    #mihomo-core-widget label {
      display: block;
      margin-bottom: 6px;
      color: #71717a;
      font-size: 12px;
      font-weight: 700;
    }
    #mihomo-core-widget .mc-field {
      display: grid;
      grid-template-columns: 1fr auto;
      gap: 8px;
      margin-bottom: 10px;
    }
    #mihomo-core-widget input[type="text"] {
      width: 100%;
      height: 34px;
      border: 1px solid #e4e4e7;
      border-radius: 10px;
      padding: 0 10px;
      outline: none;
      background: #fafafa;
      color: #18181b;
      font-size: 12px;
    }
    #mihomo-core-widget .mc-options {
      display: grid;
      gap: 8px;
      margin-top: 8px;
      color: #3f3f46;
      font-size: 12px;
    }
    #mihomo-core-widget .mc-options label {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
      color: inherit;
      font-weight: 600;
    }
    #mihomo-core-widget .mc-log {
      max-height: 160px;
      overflow: auto;
      margin: 0;
      padding: 10px;
      border-radius: 10px;
      background: #0f172a;
      color: #dbeafe;
      font: 11px/1.45 "Cascadia Mono", Consolas, monospace;
      white-space: pre-wrap;
    }
    #mihomo-core-widget .mc-toast {
      display: none;
      margin: 10px 14px 0;
      padding: 9px 10px;
      border-radius: 10px;
      background: #ecfeff;
      color: #0e7490;
      font-size: 12px;
      font-weight: 700;
    }
    #mihomo-core-widget .mc-toast.show { display: block; }
  `;
  document.head.appendChild(css);

  const root = document.createElement('div');
  root.id = 'mihomo-core-widget';
  root.innerHTML = `
    <div class="mc-pill">
      <span class="mc-dot"></span>
      <span class="mc-title">Mihomo Core</span>
      <span class="mc-sub" data-role="status">未运行</span>
      <button data-action="start">启动</button>
      <button class="danger" data-action="stop">停止</button>
      <button class="icon" data-action="toggle" title="内核设置">⚙</button>
    </div>
    <div class="mc-panel">
      <div class="mc-section">
        <label>内核路径</label>
        <div class="mc-field">
          <input type="text" data-field="corePath" />
          <button class="secondary" data-action="browseCore">选择</button>
        </div>
        <label>配置文件</label>
        <div class="mc-field">
          <input type="text" data-field="configPath" />
          <button class="secondary" data-action="browseConfig">选择</button>
        </div>
        <label>API 地址</label>
        <div class="mc-field">
          <input type="text" data-field="apiUrl" />
          <button class="secondary" data-action="reload">刷新 UI</button>
        </div>
        <label>Secret</label>
        <div class="mc-field">
          <input type="text" data-field="secret" />
          <button data-action="save">保存</button>
        </div>
        <div class="mc-options">
          <label><input type="checkbox" data-field="startCoreOnLaunch" /> 启动软件时自动启动内核</label>
          <label><input type="checkbox" data-field="minimizeToTray" /> 关闭窗口时最小化到托盘</label>
          <label><input type="checkbox" data-field="autostart" /> 开机自启</label>
        </div>
      </div>
      <div class="mc-section">
        <label>内核日志</label>
        <pre class="mc-log" data-role="log">暂无日志</pre>
      </div>
      <div class="mc-toast" data-role="toast"></div>
    </div>
  `;
  document.body.appendChild(root);

  const post = (message) => window.chrome?.webview?.postMessage(message);
  const collect = () => ({
    type: 'save',
    corePath: root.querySelector('[data-field="corePath"]').value,
    configPath: root.querySelector('[data-field="configPath"]').value,
    apiUrl: root.querySelector('[data-field="apiUrl"]').value,
    secret: root.querySelector('[data-field="secret"]').value,
    startCoreOnLaunch: root.querySelector('[data-field="startCoreOnLaunch"]').checked,
    minimizeToTray: root.querySelector('[data-field="minimizeToTray"]').checked,
    autostart: root.querySelector('[data-field="autostart"]').checked
  });

  root.addEventListener('click', (event) => {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    const action = button.dataset.action;

    if (action === 'toggle') {
      root.dataset.open = root.dataset.open === 'true' ? 'false' : 'true';
      return;
    }
    if (action === 'save') return post(collect());
    if (action === 'start') return post({ ...collect(), type: 'start' });
    if (action === 'stop') return post({ type: 'stop' });
    if (action === 'reload') return post({ ...collect(), type: 'reload' });
    if (action === 'browseCore') return post({ type: 'browseCore' });
    if (action === 'browseConfig') return post({ type: 'browseConfig' });
  });

  window.__mihomoControlSetState = (state) => {
    root.dataset.running = state.isRunning ? 'true' : 'false';
    root.querySelector('[data-role="status"]').textContent = state.isRunning
      ? `运行中${state.processId ? ` · PID ${state.processId}` : ''}`
      : '未运行';
    root.querySelector('[data-action="start"]').disabled = state.isRunning;
    root.querySelector('[data-action="stop"]').disabled = !state.isRunning;
    root.querySelector('[data-field="corePath"]').value = state.corePath ?? '';
    root.querySelector('[data-field="configPath"]').value = state.configPath ?? '';
    root.querySelector('[data-field="apiUrl"]').value = state.apiUrl ?? '';
    root.querySelector('[data-field="secret"]').value = state.secret ?? '';
    root.querySelector('[data-field="startCoreOnLaunch"]').checked = !!state.startCoreOnLaunch;
    root.querySelector('[data-field="minimizeToTray"]').checked = !!state.minimizeToTray;
    root.querySelector('[data-field="autostart"]').checked = !!state.autostart;
    root.querySelector('[data-role="log"]').textContent = state.logText || '暂无日志';
  };

  window.__mihomoControlNotice = (message) => {
    const toast = root.querySelector('[data-role="toast"]');
    toast.textContent = message;
    toast.classList.add('show');
    window.clearTimeout(window.__mihomoControlNoticeTimer);
    window.__mihomoControlNoticeTimer = window.setTimeout(() => toast.classList.remove('show'), 2400);
  };
})();
""";

        await _webView.CoreWebView2.ExecuteScriptAsync(script);
    }

    private async void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var document = JsonDocument.Parse(e.WebMessageAsJson);
            var root = document.RootElement;
            var type = root.GetProperty("type").GetString();

            switch (type)
            {
                case "save":
                    SaveSettingsFromMessage(root, showMessage: true);
                    break;
                case "start":
                    SaveSettingsFromMessage(root, showMessage: false);
                    StartCore();
                    break;
                case "stop":
                    StopCore();
                    break;
                case "reload":
                    SaveSettingsFromMessage(root, showMessage: false);
                    LoadDashboard();
                    break;
                case "browseCore":
                    BrowseCorePath();
                    break;
                case "browseConfig":
                    BrowseConfigPath();
                    break;
            }

            SendStateToDashboard();
        }
        catch (Exception ex)
        {
            await ShowDashboardNoticeAsync($"操作失败：{ex.Message}");
        }
    }

    private void SaveSettingsFromMessage(JsonElement root, bool showMessage)
    {
        _settings.CorePath = GetString(root, "corePath", _settings.CorePath).Trim();
        _settings.ConfigPath = GetString(root, "configPath", _settings.ConfigPath).Trim();
        _settings.DashboardApiUrl = GetString(root, "apiUrl", _settings.DashboardApiUrl).Trim();
        _settings.Secret = GetString(root, "secret", _settings.Secret);
        _settings.StartCoreOnLaunch = GetBool(root, "startCoreOnLaunch", _settings.StartCoreOnLaunch);
        _settings.MinimizeToTray = GetBool(root, "minimizeToTray", _settings.MinimizeToTray);
        _settings.Save();

        if (root.TryGetProperty("autostart", out var autostart) && autostart.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            AutostartManager.SetEnabled(autostart.GetBoolean());
        }

        if (showMessage)
        {
            _ = ShowDashboardNoticeAsync("设置已保存。");
        }
    }

    private static string GetString(JsonElement root, string propertyName, string fallback)
    {
        return root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? fallback
            : fallback;
    }

    private static bool GetBool(JsonElement root, string propertyName, bool fallback)
    {
        return root.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : fallback;
    }

    private void StartCore()
    {
        try
        {
            _mihomo.Start(_settings);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SendStateToDashboard();
        }
    }

    private void StopCore()
    {
        try
        {
            _mihomo.Stop();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "停止失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SendStateToDashboard();
        }
    }

    private void RefreshStatus()
    {
        var running = _mihomo.IsRunning;
        _trayIcon.Text = running ? "Mihomo Dashboard - 运行中" : "Mihomo Dashboard - 未运行";
        SendStateToDashboard();
    }

    private void SendStateToDashboard()
    {
        if (_webView.CoreWebView2 is null)
        {
            return;
        }

        var state = new
        {
            isRunning = _mihomo.IsRunning,
            processId = _mihomo.ProcessId,
            corePath = _settings.CorePath,
            configPath = _settings.ConfigPath,
            apiUrl = _settings.DashboardApiUrl,
            secret = _settings.Secret,
            startCoreOnLaunch = _settings.StartCoreOnLaunch,
            minimizeToTray = _settings.MinimizeToTray,
            autostart = AutostartManager.IsEnabled(),
            logText = TrimLog(_mihomo.LogText)
        };
        var json = JsonSerializer.Serialize(state);
        _ = _webView.CoreWebView2.ExecuteScriptAsync($"window.__mihomoControlSetState && window.__mihomoControlSetState({json});");
    }

    private static string TrimLog(string log)
    {
        const int maxLength = 8000;
        return log.Length <= maxLength ? log : log[^maxLength..];
    }

    private async Task ShowDashboardNoticeAsync(string message)
    {
        if (_webView.CoreWebView2 is null)
        {
            return;
        }

        var text = JsonSerializer.Serialize(message);
        await _webView.CoreWebView2.ExecuteScriptAsync($"window.__mihomoControlNotice ? window.__mihomoControlNotice({text}) : alert({text});");
    }

    private void BrowseCorePath()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择 mihomo.exe",
            Filter = "Mihomo executable|mihomo*.exe;clash*.exe|Executable|*.exe|All files|*.*"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settings.CorePath = dialog.FileName;
            _settings.Save();
        }
    }

    private void BrowseConfigPath()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "选择 config.yaml",
            Filter = "YAML config|*.yaml;*.yml|All files|*.*"
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _settings.ConfigPath = dialog.FileName;
            _settings.Save();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized && _settings.MinimizeToTray)
        {
            HideToTray();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_allowClose && _settings.MinimizeToTray)
        {
            e.Cancel = true;
            HideToTray();
            return;
        }

        base.OnFormClosing(e);
    }

    private void HideToTray()
    {
        Hide();
        ShowInTaskbar = false;
    }

    private void ShowFromTray()
    {
        ShowInTaskbar = true;
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private void ExitApplication()
    {
        _allowClose = true;
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _mihomo.Dispose();
            _dashboardServer.Dispose();
            _webView.Dispose();
        }

        base.Dispose(disposing);
    }
}
