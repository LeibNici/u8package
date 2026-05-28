using System;
using System.Windows.Forms;

namespace Xinchuan.U8Bridge.Manager
{
    public partial class MainForm
    {
        private const int MillisecondsPerSecond = 1000;
        private const int SecondsPerMinute = 60;
        private const int MinimumTimerSeconds = 1;

        private void ScheduleAutoUpdate(UpdateCheckConfig config)
        {
            EnsureAutoUpdateTimers();
            StopAutoUpdateSchedule();
            if (config == null || !config.Enabled || !config.AutoCheckEnabled)
            {
                SetStatus("自动检查未启用");
                return;
            }

            int startupDelaySeconds = Math.Max(
                config.AutoCheckStartupDelaySeconds,
                UpdateCheckConfig.MinAutoCheckStartupDelaySeconds);
            autoUpdateIntervalTimer.Interval = ToMilliseconds(
                config.AutoCheckIntervalMinutes,
                SecondsPerMinute * MillisecondsPerSecond,
                UpdateCheckConfig.MinAutoCheckIntervalMinutes);
            StartAutoUpdateTimer(startupDelaySeconds);
            SetStatus("自动检查已启用: 启动延迟 "
                + startupDelaySeconds
                + " 秒，间隔 "
                + config.AutoCheckIntervalMinutes
                + " 分钟");
        }

        private void StopAutoUpdateSchedule()
        {
            autoUpdateStartupTimer?.Stop();
            autoUpdateIntervalTimer?.Stop();
        }

        private void EnsureAutoUpdateTimers()
        {
            if (autoUpdateStartupTimer != null)
            {
                return;
            }

            autoUpdateStartupTimer = new Timer();
            autoUpdateIntervalTimer = new Timer();
            autoUpdateStartupTimer.Tick += (s, e) =>
            {
                autoUpdateStartupTimer.Stop();
                autoUpdateIntervalTimer.Start();
                RunAutoUpdateCheck();
            };
            autoUpdateIntervalTimer.Tick += (s, e) => RunAutoUpdateCheck();
        }

        private void StartAutoUpdateTimer(int startupDelaySeconds)
        {
            if (startupDelaySeconds <= 0)
            {
                autoUpdateIntervalTimer.Start();
                RunAutoUpdateCheck();
                return;
            }

            autoUpdateStartupTimer.Interval = ToMilliseconds(
                startupDelaySeconds,
                MillisecondsPerSecond,
                MinimumTimerSeconds);
            autoUpdateStartupTimer.Start();
        }

        private async void RunAutoUpdateCheck()
        {
            await CheckUpdateAsync(false).ConfigureAwait(true);
        }

        private void ReportUpdateBusy(bool manual, string message)
        {
            if (manual)
            {
                SetStatus(message);
                return;
            }

            AppendLog("AUTO UPDATE: " + message);
        }

        private static int ToMilliseconds(int value, int factor, int minimumValue)
        {
            int normalized = Math.Max(value, minimumValue);
            long milliseconds = (long)normalized * factor;
            return milliseconds > int.MaxValue ? int.MaxValue : (int)milliseconds;
        }
    }
}
