using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xinchuan.U8Bridge.Manager.Services;

namespace Xinchuan.U8Bridge.Manager
{
    public partial class MainForm
    {
        private Timer autoUpdateStartupTimer;
        private Timer autoUpdateIntervalTimer;
        private bool updateCheckRunning;
        private bool updateInstallRunning;

        private async void CheckUpdate()
        {
            await CheckUpdateAsync(true).ConfigureAwait(true);
        }

        private async Task<UpdateCheckResult> CheckUpdateAsync(bool manual)
        {
            try
            {
                if (updateCheckRunning || updateInstallRunning)
                {
                    ReportUpdateBusy(manual, "更新检测或安装正在执行，已跳过本次检查");
                    return null;
                }

                updateCheckRunning = true;
                BridgeConfig config = manual ? BuildManualUpdateConfig() : configService.LoadOrDefault();
                UpdateCheckResult result = await updateCheckService
                    .CheckAsync(config.Update, configService.BaseDirectory, config.ConfigSchemaVersion)
                    .ConfigureAwait(true);
                lastUpdateResult = result;
                updateCurrentVersionTextBox.Text = result.CurrentVersion;
                RefreshInstallButton(result);
                AppendLog("UPDATE:" + Environment.NewLine + FormatUpdateResult(result));
                SetStatus(result.HasUpdate ? "发现新版本: " + result.LatestVersion : result.ConfigMessage);
                await TryAutoInstallAsync(result, config.Update, manual).ConfigureAwait(true);
                return result;
            }
            catch (Exception ex)
            {
                SetStatus((manual ? "更新检测失败: " : "自动更新检测失败: ") + ex.Message);
                return null;
            }
            finally
            {
                updateCheckRunning = false;
                if (!updateInstallRunning)
                {
                    RefreshInstallButton(lastUpdateResult);
                }
            }
        }

        private async void InstallUpdate()
        {
            if (updateCheckRunning)
            {
                SetStatus("更新检测正在执行，请稍后再更新升级");
                return;
            }

            await InstallUpdateAsync(lastUpdateResult, true).ConfigureAwait(true);
        }

        private async Task InstallUpdateAsync(UpdateCheckResult result, bool manual)
        {
            string blockedReason = BuildInstallBlockReason(result);
            if (!string.IsNullOrWhiteSpace(blockedReason))
            {
                SetStatus((manual ? "不能更新升级: " : "自动部署跳过: ") + blockedReason);
                return;
            }

            if (manual && !ConfirmInstall(result))
            {
                return;
            }

            try
            {
                if (updateInstallRunning)
                {
                    SetStatus("更新安装正在执行，已跳过重复请求");
                    return;
                }

                updateInstallRunning = true;
                updateNowButton.Enabled = false;
                SetStatus(manual ? "正在准备更新升级包..." : "正在自动下载并部署更新...");
                if (manual)
                {
                    SaveConfigFromForm();
                }

                string script = await updateInstallService
                    .PrepareAsync(result, configService.BaseDirectory)
                    .ConfigureAwait(true);
                updateInstallService.Launch(script);
                allowExit = true;
                Close();
            }
            catch (Exception ex)
            {
                updateInstallRunning = false;
                RefreshInstallButton(lastUpdateResult);
                SetStatus((manual ? "更新升级准备失败: " : "自动部署准备失败: ") + ex.Message);
            }
        }

        private UpdateCheckConfig BuildUpdate()
        {
            return new UpdateCheckConfig
            {
                Enabled = updateEnabledCheckBox.Checked,
                SourceType = Convert.ToString(updateSourceTypeComboBox.SelectedItem),
                CheckUrl = updateUrlTextBox.Text.Trim(),
                AutoCheckEnabled = updateAutoCheckCheckBox.Checked,
                AutoInstallEnabled = updateAutoInstallCheckBox.Checked,
                AutoCheckIntervalMinutes = Convert.ToInt32(updateIntervalMinutesInput.Value),
                AutoCheckStartupDelaySeconds = Convert.ToInt32(updateStartupDelaySecondsInput.Value),
                DownloadProxyPrefix = updateProxyPrefixTextBox.Text.Trim(),
                IncludePrerelease = updatePrereleaseCheckBox.Checked
            };
        }

        private static string FormatUpdateResult(UpdateCheckResult result)
        {
            return "当前版本: " + result.CurrentVersion + Environment.NewLine
                + "最新版本: " + result.LatestVersion + Environment.NewLine
                + "是否需要更新: " + (result.HasUpdate ? "是" : "否") + Environment.NewLine
                + "自动部署条件: " + BuildAutoInstallConditionText(result) + Environment.NewLine
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
            updateNowButton.Enabled = !updateInstallRunning && CanClickInstall(result);
        }

        private static bool CanClickInstall(UpdateCheckResult result)
        {
            return result != null && result.HasUpdate;
        }

        private static string BuildInstallBlockReason(UpdateCheckResult result)
        {
            if (result == null)
            {
                return "请先检查更新";
            }

            if (!result.HasUpdate)
            {
                return "当前已是最新版本";
            }

            if (!result.ConfigCompatible)
            {
                return result.ConfigMessage;
            }

            return string.IsNullOrWhiteSpace(result.DownloadUrl) ? "新版本缺少下载地址" : null;
        }

        private static bool ConfirmInstall(UpdateCheckResult result)
        {
            string message = "将下载并安装版本 " + result.LatestVersion
                + Environment.NewLine
                + "升级前会备份当前目录，并保留 appsettings.json。是否继续？";
            return MessageBox.Show(message, "确认更新升级", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes;
        }

        private BridgeConfig BuildManualUpdateConfig()
        {
            SaveConfigFromForm();
            return BuildConfig();
        }

        private async Task TryAutoInstallAsync(
            UpdateCheckResult result,
            UpdateCheckConfig config,
            bool manual)
        {
            if (manual || result == null || !result.HasUpdate)
            {
                return;
            }

            if (config == null || !config.AutoInstallEnabled)
            {
                SetStatus("发现新版本，但自动部署未启用");
                return;
            }

            await InstallUpdateAsync(result, false).ConfigureAwait(true);
        }

        private void ScheduleAutoUpdateFromForm()
            => ScheduleAutoUpdate(BuildUpdate());

        private static string BuildAutoInstallConditionText(UpdateCheckResult result)
        {
            return result.HasUpdate
                && result.ConfigCompatible
                && !string.IsNullOrWhiteSpace(result.DownloadUrl)
                ? "满足"
                : "不满足";
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
