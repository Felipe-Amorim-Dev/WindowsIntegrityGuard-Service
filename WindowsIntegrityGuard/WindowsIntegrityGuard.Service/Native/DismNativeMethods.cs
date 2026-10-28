using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsIntegrityGuard.Service.Native
{
    internal static class DismNativeMethods
    {
        internal const string OnlineImage = "DISM_{53BFAE52-B167-4E2F-A258-0A37B57FF845}";
        internal const uint DefaultSession = 0;

        [DllImport("DismApi.dll", CharSet = CharSet.Unicode)]
        internal static extern int DismInitialize(DismLogLevel logLevel, string? logFilePath, string? scratchDirectory);

        [DllImport("DismApi.dll", CharSet = CharSet.Unicode)]
        internal static extern int DismOpenSession(string imagePath, string? windowsDirectory, string? systemDrive, out uint session);

        [DllImport("DismApi.dll")]
        internal static extern int DismCheckImageHealth(uint session, [MarshalAs(UnmanagedType.Bool)] bool scanImage, IntPtr cancelEvent, IntPtr progress, IntPtr userData, out DismImageHealthState imageHealth);

        [DllImport("DismApi.dll")]
        internal static extern int DismCloseSession(uint session);

        [DllImport("DismApi.dll")]
        internal static extern int DismShutdown();
    }

    internal enum DismLogLevel
    {
        Errors = 0,
        ErrorsWarnings = 1,
        ErrorsWarningsInfo = 2
    }

    internal enum DismImageHealthState
    {
        Healthy = 0,
        Repairable = 1,
        NonRepairable = 2
    }
}
