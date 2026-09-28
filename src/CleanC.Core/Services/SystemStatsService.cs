using System;
using System.IO;
using CleanC.Core.Models;
using CleanC.Core.Native;

namespace CleanC.Core.Services
{
    public class SystemStatsService
    {
        private static long _prevIdleTime;
        private static long _prevKernelTime;
        private static long _prevUserTime;
        private static bool _firstCpuSample = true;

        public static SystemStats GetCurrentStats()
        {
            var stats = new SystemStats();

            // 1. RAM via GlobalMemoryStatusEx (0.001 ms)
            var memStatus = new Win32.MEMORYSTATUSEX();
            if (Win32.GlobalMemoryStatusEx(memStatus))
            {
                stats.TotalRamBytes = (long)memStatus.ullTotalPhys;
                stats.FreeRamBytes = (long)memStatus.ullAvailPhys;
                stats.UsedRamBytes = stats.TotalRamBytes - stats.FreeRamBytes;
                stats.RamPercent = Math.Round(((double)stats.UsedRamBytes / stats.TotalRamBytes) * 100.0, 1);
            }

            // 2. CPU via GetSystemTimes
            stats.CpuPercent = GetCpuUsage();

            // 3. Drive C: via DriveInfo
            try
            {
                var driveC = new DriveInfo("C");
                if (driveC.IsReady)
                {
                    stats.DiskTotalBytes = driveC.TotalSize;
                    stats.DiskFreeBytes = driveC.AvailableFreeSpace;
                    stats.DiskUsedBytes = stats.DiskTotalBytes - stats.DiskFreeBytes;
                    stats.DiskPercent = Math.Round(((double)stats.DiskUsedBytes / stats.DiskTotalBytes) * 100.0, 1);
                }
            }
            catch { }

            return stats;
        }

        private static double GetCpuUsage()
        {
            try
            {
                if (!Win32.GetSystemTimes(out long idleTime, out long kernelTime, out long userTime))
                    return 0;

                if (_firstCpuSample)
                {
                    _prevIdleTime = idleTime;
                    _prevKernelTime = kernelTime;
                    _prevUserTime = userTime;
                    _firstCpuSample = false;
                    return 5.0; // Typical baseline initial estimate
                }

                long usr = userTime - _prevUserTime;
                long ker = kernelTime - _prevKernelTime;
                long idl = idleTime - _prevIdleTime;

                long sys = ker + usr;
                if (sys <= 0) return 0;

                double cpu = (double)(sys - idl) * 100.0 / sys;
                _prevIdleTime = idleTime;
                _prevKernelTime = kernelTime;
                _prevUserTime = userTime;

                return Math.Max(0.0, Math.Min(100.0, Math.Round(cpu, 1)));
            }
            catch
            {
                return 0;
            }
        }
    }
}
