using System;
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
                updateCurrentVersionTextBox.Text = result.CurrentVersion;
                AppendLog("UPDATE:" + Environment.NewLine + FormatUpdateResult(result));
                SetStatus(result.HasUpdate ? "发现新版本: " + result.LatestVersion : result.ConfigMessage);
            }
            catch (Exception ex)
            {
                SetStatus("更新检测失败: " + ex.Message);
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
