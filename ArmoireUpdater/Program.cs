// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
internal class Program
{
    private static void Main(string[] args)
    {
        string updateDir = AppDomain.CurrentDomain.BaseDirectory + "\\update";
        if (Directory.Exists(updateDir) && File.Exists(updateDir + "\\Armoire.exe"))
        {
            Thread.Sleep(1000);
            foreach (string file in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                if (!Path.GetFileNameWithoutExtension(file).Contains("updater"))
                {
                    File.Delete(file);
                }
            }
            foreach (string dir in Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory))
            {
                if (!dir.EndsWith("update"))
                {
                    foreach(string file in Directory.EnumerateFiles(dir))
                    {
                        File.Delete(file);
                    }
                    Directory.Delete(dir, true);
                }
            }
            foreach (string file in Directory.EnumerateFiles(updateDir))
            {
                if (!Path.GetFileNameWithoutExtension(file).Contains("updater"))
                {
                    Console.WriteLine(file);
                    File.Move(file, AppDomain.CurrentDomain.BaseDirectory + "\\" + Path.GetFileName(file));
                }
                else
                {
                    Console.WriteLine(file + " to be deleted");
                    File.Delete(file);
                    Console.WriteLine(file + " deleted");
                }
            }
            foreach (var dir in Directory.GetDirectories(updateDir))
            {
                string[] split = dir.Split('\\');
                Directory.Move(dir, AppDomain.CurrentDomain.BaseDirectory + "\\" + split.Last());
            }
            Directory.Delete(updateDir, true);
            Console.WriteLine("Update complete.\nLaunching Armoire.");
            ProcessStartInfo start = new ProcessStartInfo();
            start.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            start.FileName = "Armoire.exe";
            start.UseShellExecute = false;
            using (var process = new Process())
            {
                process.StartInfo = start;
                process.Start();
            }
            return;
            Console.WriteLine("the program shouldn't be open to write this :(");
        }
        else { Console.OutputEncoding = Encoding.UTF8; Console.WriteLine("No update folder detected.\nPlease do not run the updater manually.\nアップデートのフォルダは見つかりませんでした。\nアップデーターには、手動の使用はご遠慮ください。"); Thread.Sleep(3000); Environment.Exit(0); }
    }
}