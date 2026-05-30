using System;
using System.IO;

namespace Xinchuan.U8Bridge.Services
{
    public static class BridgeRuntimeEnvironment
    {
        private const string TempDirectoryName = "temp";
        private static readonly object SyncRoot = new object();
        private static bool initialized;

        public static string BaseDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string TempDirectory
        {
            get { return Path.Combine(BaseDirectory, TempDirectoryName); }
        }

        public static void InitializeForProcess()
        {
            lock (SyncRoot)
            {
                if (initialized)
                {
                    return;
                }

                SetCurrentDirectory(BaseDirectory, "startup");
                EnsureTempDirectory();
                SetProcessEnvironment("TEMP", TempDirectory);
                SetProcessEnvironment("TMP", TempDirectory);
                initialized = true;
                BridgeLogger.Info(
                    "Bridge runtime environment initialized. "
                    + "CurrentDirectory=BaseDirectory; TEMP/TMP=application temp directory.");
            }
        }

        public static IDisposable EnterBrokerInvocationScope()
        {
            return new CurrentDirectoryScope(BaseDirectory);
        }

        public static void AssertInitializedForMapperSnapshot()
        {
            AssertDirectoryExists(TempDirectory, "Bridge temp directory");
            AssertSamePath("TEMP", Environment.GetEnvironmentVariable("TEMP"), TempDirectory);
            AssertSamePath("TMP", Environment.GetEnvironmentVariable("TMP"), TempDirectory);
            AssertSamePath("CurrentDirectory", Environment.CurrentDirectory, BaseDirectory);
        }

        private static void EnsureTempDirectory()
        {
            try
            {
                Directory.CreateDirectory(TempDirectory);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Bridge temp directory initialization failed.", ex);
            }
        }

        private static void SetProcessEnvironment(string name, string value)
        {
            try
            {
                Environment.SetEnvironmentVariable(name, value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Bridge process environment setup failed: " + name, ex);
            }
        }

        private static void SetCurrentDirectory(string directory, string phase)
        {
            try
            {
                Environment.CurrentDirectory = directory;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Bridge current directory setup failed during " + phase + ".", ex);
            }
        }

        private static void AssertDirectoryExists(string directory, string name)
        {
            if (!Directory.Exists(directory))
            {
                throw new InvalidOperationException(name + " does not exist.");
            }
        }

        private static void AssertSamePath(string name, string actual, string expected)
        {
            if (!SamePath(actual, expected))
            {
                throw new InvalidOperationException(name + " is not using the Bridge application directory.");
            }
        }

        private static bool SamePath(string actual, string expected)
        {
            if (string.IsNullOrWhiteSpace(actual) || string.IsNullOrWhiteSpace(expected))
            {
                return false;
            }

            return string.Equals(Normalize(actual), Normalize(expected), StringComparison.OrdinalIgnoreCase);
        }

        private static string Normalize(string path)
        {
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private sealed class CurrentDirectoryScope : IDisposable
        {
            private readonly string previousDirectory;
            private bool disposed;

            public CurrentDirectoryScope(string directory)
            {
                previousDirectory = Environment.CurrentDirectory;
                SetCurrentDirectory(directory, "U8 broker invocation");
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;
                SetCurrentDirectory(previousDirectory, "U8 broker invocation restore");
            }
        }
    }
}
