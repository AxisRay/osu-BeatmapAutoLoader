using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using InjectLoader.Properties;

namespace InjectLoader
{
    static class Program
    {
        private readonly static string Path=Application.StartupPath + "\\";
        public readonly static string OsuPatch=FindPatch();

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            CreateFile(Path);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new InjectMonitor());
        }
        private static string FindPatch()
        {
            if (File.Exists(Path + "osu!.exe"))
            {
                return Path + "osu!.exe";
            }
            char[] alphabet = "CDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            string x86 = ":\\Program Files (x86)\\osu!\\";
            string x64 = ":\\Program Files\\osu!\\";
            foreach (char disk in alphabet)
            {
                if (File.Exists(disk + x86 + "osu!.exe"))
                    return disk + x86 + "osu!.exe";
                if (File.Exists(disk + x64 + "osu!.exe"))
                    return disk + x64 + "osu!.exe";
            }
            return null;
        }
        private static void CreateFile(string path)
        {
            if (!File.Exists(path + "EasyHook.dll")) //文件不存在
            {
                var fs = new FileStream(path + "EasyHook.dll", FileMode.CreateNew, FileAccess.Write);
                byte[] buffer = Resources.EasyHook;
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();
            }
            if (!File.Exists(path + "EasyHook32.dll")) //文件不存在
            {
                var fs = new FileStream(path + "EasyHook32.dll", FileMode.CreateNew, FileAccess.Write);
                byte[] buffer = Resources.EasyHook32;
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();
            }
            if (!File.Exists(path + "HtmlAgilityPack.dll")) //文件不存在
            {
                var fs = new FileStream(path + "HtmlAgilityPack.dll", FileMode.CreateNew, FileAccess.Write);
                byte[] buffer = Resources.HtmlAgilityPack;
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();
            }
            if (!File.Exists(path + "Injector.dll")) //文件不存在
            {
                var fs = new FileStream(path + "Injector.dll", FileMode.CreateNew, FileAccess.Write);
                byte[] buffer = Resources.Injector;
                fs.Write(buffer, 0, buffer.Length);
                fs.Close();
            }
        }

    }
}
