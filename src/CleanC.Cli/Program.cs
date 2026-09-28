using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CleanC.Core.Models;
using CleanC.Core.Services;

namespace CleanC.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "";

            if (mode == "--silent" || mode == "silent")
            {
                return await RunSilentCleanAsync();
            }

            if (mode == "--cli" || mode == "cli")
            {
                return await RunCliMenuAsync();
            }

            // Default: Launch GUI application
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string guiExe = Path.Combine(baseDir, "CleanC.UI.exe");
            if (!File.Exists(guiExe))
            {
                // Also check sibling build folder if running in development
                string devGui = Path.Combine(baseDir, @"..\CleanC.UI\CleanC.UI.exe");
                if (File.Exists(devGui)) guiExe = devGui;
            }

            if (File.Exists(guiExe))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = guiExe,
                    UseShellExecute = true
                });
                return 0;
            }

            // If GUI not found, fallback to CLI menu
            return await RunCliMenuAsync();
        }

        static async Task<int> RunSilentCleanAsync()
        {
            try
            {
                var scan = await SystemScanService.ScanAsync();
                var items = scan.Groups.Find(g => g.Id == "recommended")?.Items;
                if (items != null)
                {
                    await CleanService.CleanItemsAsync(items);
                }
                await OptimizerService.BoostRamAsync();
                OptimizerService.FlushDns();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        static async Task<int> RunCliMenuAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=================================================");
                Console.WriteLine("    CLEANC - WINDOWS 11 NATIVE PC OPTIMIZER      ");
                Console.WriteLine("=================================================");
                Console.ResetColor();

                var stats = SystemStatsService.GetCurrentStats();
                Console.WriteLine($" [RAM]  {stats.UsedRamFormatted} / {stats.TotalRamFormatted} ({stats.RamPercent}%)");
                Console.WriteLine($" [DISK] {stats.DiskFreeFormatted} آزاد از {stats.DiskTotalFormatted} ({stats.DiskPercent}% پر)");
                Console.WriteLine($" [CPU]  {stats.CpuPercent}%\n");

                Console.WriteLine(" 1. بهینه‌سازی سریع ۱-کلیکی (پاکسازی فایل‌های موقت + بوست رم)");
                Console.WriteLine(" 2. آزادسازی و بوست حافظه رم (Empty Working Set)");
                Console.WriteLine(" 3. تخلیه کش آدرس‌های شبکه (Flush DNS)");
                Console.WriteLine(" 4. اجرای محیط گرافیکی ویندوز ۱۱ (GUI)");
                Console.WriteLine(" 5. خروج");
                Console.WriteLine("-------------------------------------------------");
                Console.Write(" لطفا یک گزینه را انتخاب کنید (1-5): ");

                var key = Console.ReadLine()?.Trim();
                switch (key)
                {
                    case "1":
                        Console.WriteLine("\n[..] در حال اسکن و پاکسازی...");
                        var scan = await SystemScanService.ScanAsync();
                        var recItems = scan.Groups.Find(g => g.Id == "recommended")?.Items;
                        if (recItems != null)
                        {
                            var clean = await CleanService.CleanItemsAsync(recItems);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"[OK] پاکسازی تکمیل شد: {clean.FormattedFreed} آزاد شد.");
                            Console.ResetColor();
                        }
                        break;

                    case "2":
                        Console.WriteLine("\n[..] در حال آزادسازی رم پردازه‌ها...");
                        var boost = await OptimizerService.BoostRamAsync();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"[OK] مقدار {boost.FormattedFreed} از حافظه رم آزاد شد. (تعداد {boost.ProcessCount} پردازش)");
                        Console.ResetColor();
                        break;

                    case "3":
                        bool ok = OptimizerService.FlushDns();
                        Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.WriteLine(ok ? "\n[OK] کش DNS با موفقیت نوسازی شد." : "\n[FAIL] خطا در تخلیه کش DNS.");
                        Console.ResetColor();
                        break;

                    case "4":
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        string guiExe = Path.Combine(baseDir, "CleanC.UI.exe");
                        if (File.Exists(guiExe))
                        {
                            Process.Start(new ProcessStartInfo { FileName = guiExe, UseShellExecute = true });
                            return 0;
                        }
                        Console.WriteLine("فایل GUI یافت نشد.");
                        break;

                    case "5":
                        return 0;

                    default:
                        break;
                }

                Console.WriteLine("\nبرای ادامه هر کلیدی را فشار دهید...");
                Console.ReadKey(true);
            }
        }
    }
}
