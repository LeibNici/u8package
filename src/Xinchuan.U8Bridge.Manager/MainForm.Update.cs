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
                UpdateCheckResult result = await updateCheckService
                    .CheckAsync(BuildConfig().Update, configService.BaseDirectory)
                    .ConfigureAwait(true);
                AppendLog("UPDATE:" + Environment.NewLine + FormatUpdateResult(result));
                SetStatus(result.HasUpdate ? "发现新版本: " + result.LatestVersion : "当前已是最新版本");
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
