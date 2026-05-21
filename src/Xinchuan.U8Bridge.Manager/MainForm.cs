using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Xinchuan.U8Bridge.Manager.Services;

namespace Xinchuan.U8Bridge.Manager
{
    public partial class MainForm : Form
    {
        private readonly ConfigService configService = new ConfigService();
        private readonly BridgeProcessService processService = new BridgeProcessService();
        private readonly BridgeHttpClient httpClient = new BridgeHttpClient();
        private readonly UrlAclService urlAclService = new UrlAclService();
        private bool allowExit;

        public MainForm()
        {
            InitializeComponent();
            processService.OutputReceived += AppendLogSafe;
            LoadConfigToForm();
            RefreshLog();
        }

        private void LoadConfigToForm()
        {
            try
            {
                BindConfig(configService.LoadOrDefault());
                SetStatus("配置已加载");
            }
            catch (Exception ex)
            {
                SetStatus("配置加载失败: " + ex.Message);
            }
        }

        private void SaveConfigFromForm()
        {
            try
            {
                BridgeConfig config = BuildConfig();
                configService.Save(config);
                SetStatus("配置已保存: " + configService.ConfigPath);
            }
            catch (Exception ex)
            {
                SetStatus("配置保存失败: " + ex.Message);
            }
        }

        private void StartBridge()
        {
            try
            {
                SaveConfigFromForm();
                processService.Start(configService.BaseDirectory, configService.ConfigPath);
                SetStatus("Bridge 已启动");
            }
            catch (Exception ex)
            {
                SetStatus("Bridge 启动失败: " + ex.Message);
            }
        }

        private void StopBridge()
        {
            try
            {
                processService.Stop();
                SetStatus("Bridge 已停止");
            }
            catch (Exception ex)
            {
                SetStatus("Bridge 停止失败: " + ex.Message);
            }
        }

        private void ConfigureUrlAcl()
        {
            try
            {
                urlAclService.Configure(baseUrlTextBox.Text);
                SetStatus("已打开管理员授权窗口，请确认后再启动服务");
            }
            catch (Exception ex)
            {
                SetStatus("监听权限配置失败: " + ex.Message);
            }
        }

        private async void TestHealth()
        {
            try
            {
                string result = await httpClient.TestHealthAsync(baseUrlTextBox.Text);
                AppendLog("HEALTH: " + result);
                SetStatus("Health 测试完成");
            }
            catch (Exception ex)
            {
                SetStatus("Health 测试失败: " + ex.Message);
            }
        }

        private async void TestLogin()
        {
            try
            {
                string result = await httpClient.TestLoginAsync(BuildConfig());
                AppendLog("LOGIN TEST:" + Environment.NewLine + result);
                SetStatus("U8 登录测试完成");
            }
            catch (Exception ex)
            {
                SetStatus("U8 登录测试失败: " + ex.Message);
            }
        }

        private void OpenLogDirectory()
        {
            Directory.CreateDirectory(configService.LogDirectory);
            Process.Start("explorer.exe", configService.LogDirectory);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (allowExit)
            {
                processService.Stop();
                trayIcon.Visible = false;
                return;
            }

            e.Cancel = true;
            HideToTray();
        }

        private void OnFormResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                SetStatus("窗口已最小化");
            }
        }

        private void HideToTray()
        {
            Hide();
            ShowInTaskbar = false;
            SetStatus("管理器已驻留到任务栏通知区域");
        }

        private void RestoreFromTray()
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Activate();
            SetStatus("管理器已打开");
        }

        private void ExitApplication()
        {
            allowExit = true;
            Close();
        }

        private void RefreshLog()
        {
            string path = configService.GetTodayLogPath();
            if (!File.Exists(path))
            {
                return;
            }

            logTextBox.Text = File.ReadAllText(path);
            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.ScrollToCaret();
        }

        private BridgeConfig BuildConfig()
        {
            var config = new BridgeConfig
            {
                BaseUrl = baseUrlTextBox.Text.Trim(),
                ApiKey = apiKeyTextBox.Text.Trim(),
                DefaultProfileName = profileNameTextBox.Text.Trim(),
                U8Mode = Convert.ToString(modeComboBox.SelectedItem),
                AllowedSourceIps = SplitLines(allowedIpsTextBox.Text),
                Profiles = new Dictionary<string, U8ProfileConfig>()
            };
            config.Profiles[config.DefaultProfileName] = BuildProfile();
            return config;
        }

        private U8ProfileConfig BuildProfile()
        {
            return new U8ProfileConfig
            {
                SubId = subIdTextBox.Text.Trim(),
                AccountId = accountIdTextBox.Text.Trim(),
                Year = yearTextBox.Text.Trim(),
                UserId = userIdTextBox.Text.Trim(),
                Password = passwordTextBox.Text,
                LoginDate = loginDateTextBox.Text.Trim(),
                Server = serverTextBox.Text.Trim(),
                Serial = serialTextBox.Text.Trim()
            };
        }

        private void BindConfig(BridgeConfig config)
        {
            U8ProfileConfig profile = config.Profiles.Values.FirstOrDefault() ?? new U8ProfileConfig();
            baseUrlTextBox.Text = config.BaseUrl;
            apiKeyTextBox.Text = config.ApiKey;
            profileNameTextBox.Text = config.DefaultProfileName;
            modeComboBox.SelectedItem = config.U8Mode == "official" ? "official" : "dryRun";
            allowedIpsTextBox.Text = string.Join(Environment.NewLine, config.AllowedSourceIps);
            subIdTextBox.Text = profile.SubId;
            accountIdTextBox.Text = profile.AccountId;
            yearTextBox.Text = profile.Year;
            userIdTextBox.Text = profile.UserId;
            passwordTextBox.Text = profile.Password;
            loginDateTextBox.Text = profile.LoginDate;
            serverTextBox.Text = profile.Server;
            serialTextBox.Text = profile.Serial;
        }

        private static IList<string> SplitLines(string value)
        {
            return value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();
        }

        private void AppendLogSafe(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            AppendLog(message);
        }

        private void AppendLog(string message)
        {
            logTextBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + " " + message + Environment.NewLine);
            logTextBox.ScrollToCaret();
        }

        private void SetStatus(string message)
        {
            statusLabel.Text = message;
            AppendLog(message);
        }
    }
}
