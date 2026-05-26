using System;
using System.Windows.Forms;
using Xinchuan.U8Bridge.Manager.Services;

namespace Xinchuan.U8Bridge.Manager
{
    public partial class MainForm
    {
        private async void CheckUpdate()
        {
            try
            {
                SaveConfigFromForm();
                BridgeConfig config = BuildConfig();
                UpdateCheckResult result = await updateCheckService
                    .CheckAsync(config.Update, configService.BaseDirectory, config.ConfigSchemaVersion)
                    .ConfigureAwait(true);
                lastUpdateResult = result;
                updateCurrentVersionTextBox.Text = result.CurrentVersion;
                RefreshInstallButton(result);
                AppendLog("UPDATE:" + Environment.NewLine + FormatUpdateResult(result));
                SetStatus(result.HasUpdate ? "发现新版本: " + result.LatestVersion : result.ConfigMessage);
            }
            catch (Exception ex)
            {
                SetStatus("更新检测失败: " + ex.Message);
            }
        }

        private async void InstallUpdate()
        {
            if (!CanInstall(lastUpdateResult))
            {
                SetStatus("没有可安装的兼容更新，请先检查更新");
                return;
            }

            if (!ConfirmInstall(lastUpdateResult))
            {
                return;
            }

            try
            {
                updateNowButton.Enabled = false;
                SetStatus("正在准备更新升级包...");
                SaveConfigFromForm();
                string script = await updateInstallService
                    .PrepareAsync(lastUpdateResult, configService.BaseDirectory)
                    .ConfigureAwait(true);
                updateInstallService.Launch(script);
                allowExit = true;
                Close();
            }
            catch (Exception ex)
            {
                RefreshInstallButton(lastUpdateResult);
                SetStatus("更新升级准备失败: " + ex.Message);
            }
        }

        private UpdateCheckConfig BuildUpdate()
        {
            return new UpdateCheckConfig
            {
                Enabled = updateEnabledCheckBox.Checked,
                SourceType = Convert.ToString(updateSourceTypeComboBox.SelectedItem),
                CheckUrl = updateUrlTextBox.Text.Trim(),
                CurrentVersion = updateCurrentVersionTextBox.Text.Trim(),
                IncludePrerelease = updatePrereleaseCheckBox.Checked
            };
        }

        private static string FormatUpdateResult(UpdateCheckResult result)
        {
            return "当前版本: " + result.CurrentVersion + Environment.NewLine
                + "最新版本: " + result.LatestVersion + Environment.NewLine
                + "是否需要更新: " + (result.HasUpdate ? "是" : "否") + Environment.NewLine
                + "当前配置Schema: " + result.CurrentConfigSchemaVersion + Environment.NewLine
                + "最新配置Schema: " + result.LatestConfigSchemaVersion + Environment.NewLine
                + "最低配置Schema: " + result.MinConfigSchemaVersion + Environment.NewLine
                + "配置兼容: " + (result.ConfigCompatible ? "是" : "否") + Environment.NewLine
                + "配置提示: " + result.ConfigMessage + Environment.NewLine
                + "Release: " + result.ReleaseUrl + Environment.NewLine
                + "Download: " + result.DownloadUrl + Environment.NewLine
                + "Notes: " + Truncate(result.Notes, 500);
        }

        private void RefreshInstallButton(UpdateCheckResult result)
        {
            updateNowButton.Enabled = CanInstall(result);
        }

        private static bool CanInstall(UpdateCheckResult result)
        {
            return result != null
                && result.HasUpdate
                && result.ConfigCompatible
                && !string.IsNullOrWhiteSpace(result.DownloadUrl);
        }

        private static bool ConfirmInstall(UpdateCheckResult result)
        {
            string message = "将下载并安装版本 " + result.LatestVersion
                + Environment.NewLine
                + "升级前会备份当前目录，并保留 appsettings.json。是否继续？";
            return MessageBox.Show(message, "确认更新升级", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength) + "...";
        }
    }
}
