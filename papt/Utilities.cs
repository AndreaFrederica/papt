﻿namespace Papt
{
    using System.Diagnostics;

    /// <summary>
    /// Defines the <see cref="Utilities" />
    /// </summary>
    public class Utilities
    {
        /// <summary>
        /// The Which
        /// </summary>
        /// <param name="command">The command<see cref="string"/></param>
        /// <returns>The <see cref="string?"/></returns>
        public static string? Which(string command)
        {
            // 获取 PATH 环境变量
            var paths = Environment.GetEnvironmentVariable("PATH");
            if (paths == null)
            {
                return null;
            }

            // 分隔 PATH 中的不同目录
            var pathDirs = paths.Split(Path.PathSeparator);

            // 获取系统的可执行文件后缀（如 Windows 的 ".exe"）
            var extensions = Environment.OSVersion.Platform == PlatformID.Win32NT ?
                Environment.GetEnvironmentVariable("PATHEXT")?.Split(';') ?? [""] : [""];

            // 遍历 PATH 中的每个目录
            foreach (var dir in pathDirs)
            {
                foreach (var ext in extensions)
                {
                    var fullPath = Path.Combine(dir, command + ext);

                    // 检查文件是否存在且可执行
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            // 未找到可执行文件
            return null;
        }

        /// <summary>
        /// The IsCommandAvailable_SystemWhich
        /// </summary>
        /// <param name="command">The command<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsCommandAvailable_SystemWhich(string command)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = "which";
                    process.StartInfo.Arguments = command;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // 如果输出为空，表示命令不存在
                    return !string.IsNullOrEmpty(output.Trim());
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The AreCommandsAvailable_SystemWhich
        /// </summary>
        /// <param name="commands">The commands<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Dictionary{string, bool}"/></returns>
        public static Dictionary<string, bool> AreCommandsAvailable_SystemWhich(IEnumerable<string> commands)
        {
            var result = new Dictionary<string, bool>();

            try
            {
                using var process = new Process();
                process.StartInfo.FileName = "which";
                process.StartInfo.Arguments = string.Join(" ", commands);
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                foreach (var command in commands)
                {
                    result[command] = output.Contains(command);
                }
            }
            catch
            {
                foreach (var command in commands)
                {
                    result[command] = false; // 如果发生异常，默认设置为false
                }
            }

            return result;
        }

        // 判断单个命令是否存在

        /// <summary>
        /// The IsCommandAvailable
        /// </summary>
        /// <param name="command">The command<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsCommandAvailable(string command)
        {
            // 使用 Which 函数查找命令是否存在
            var result = Which(command);
            return !string.IsNullOrEmpty(result);
        }

        // 判断多个命令是否存在

        /// <summary>
        /// The AreCommandsAvailable
        /// </summary>
        /// <param name="commands">The commands<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Dictionary{string, bool}"/></returns>
        public static Dictionary<string, bool> AreCommandsAvailable(IEnumerable<string> commands)
        {
            var result = new Dictionary<string, bool>();

            foreach (var command in commands)
            {
                result[command] = !string.IsNullOrEmpty(Which(command));
            }

            return result;
        }

        /// <summary>
        /// The CheckPackageManager
        /// </summary>
        /// <param name="pacmanFlag">The pacmanFlag<see cref="bool"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string CheckPackageManager(bool pacmanFlag)
        {
            if (pacmanFlag || !Utilities.IsCommandAvailable("yay"))
            {
                Logger.Warning("Using pacman instead of yay.");
                return "pacman";
            }
            return "yay";
        }
        public static bool ISConsoleInputY(bool default_val)
        {
            String? line = Console.ReadLine();
            if (line != null && (line == "Y" || line == "y"))
            {
                return true;
            }
            else if (line == "")
            {
                return default_val;
            }
            return false;
        }
    }
}
