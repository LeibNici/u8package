using System;
using System.Diagnostics;
using System.IO;

namespace Xinchuan.U8Bridge.Manager.Services
{
    public sealed class BridgeProcessService
    {
        private Process process;

        public bool IsRunning => process != null && !process.HasExited;

        public event Action<string> OutputReceived;

        public void Start(string baseDirectory, string configPath)
        {
            if (IsRunning)
            {
                return;
            }

            string exePath = Path.Combine(baseDirectory, "Xinchuan.U8Bridge.exe");
            if (!File.Exists(exePath))
            {
                throw new FileNotFoundException("Bridge executable was not found.", exePath);
            }

            StopExistingProcesses(exePath);

            process = new Process();
            process.StartInfo = CreateStartInfo(exePath, configPath);
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += OnDataReceived;
            process.ErrorDataReceived += OnDataReceived;
            process.Exited += OnExited;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                StopExistingProcesses(null);
                return;
            }

            process.Kill();
            process.WaitForExit(5000);
            process.Dispose();
            process = null;
            StopExistingProcesses(null);
            OutputReceived?.Invoke("Bridge process stopped.");
        }

        private void StopExistingProcesses(string exePath)
        {
            foreach (Process candidate in Process.GetProcessesByName("Xinchuan.U8Bridge"))
            {
                TryStopProcess(candidate, exePath);
            }
        }

        private void TryStopProcess(Process candidate, string exePath)
        {
            try
            {
                if (candidate.Id == Process.GetCurrentProcess().Id || !MatchesPath(candidate, exePath))
                {
                    return;
                }

                candidate.Kill();
                candidate.WaitForExit(5000);
                OutputReceived?.Invoke("Stopped existing Bridge process. Pid=" + candidate.Id);
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke("Stop existing Bridge process failed: " + ex.Message);
            }
            finally
            {
                candidate.Dispose();
            }
        }

        private static bool MatchesPath(Process candidate, string exePath)
        {
            if (string.IsNullOrWhiteSpace(exePath))
            {
                return true;
            }

            string candidatePath = candidate.MainModule?.FileName;
            return string.Equals(candidatePath, exePath, StringComparison.OrdinalIgnoreCase);
        }

        private static ProcessStartInfo CreateStartInfo(string exePath, string configPath)
        {
            return new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = Quote(configPath) + " --service",
                WorkingDirectory = Path.GetDirectoryName(exePath),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                OutputReceived?.Invoke(e.Data);
            }
        }

        private void OnExited(object sender, EventArgs e)
        {
            OutputReceived?.Invoke("Bridge process exited.");
        }
    }
}
