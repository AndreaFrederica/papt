﻿using papt;
using System.Diagnostics;

#nullable enable
namespace Papt;

public class PackageManagerScript
{
    const int PROG_EXIT_SUCCESS = 0;
    const int PROG_EXIT_ERROR = 1;
    private static string? shell = null;

    //! 增加对多种AurHelper的支持
    //private static Dictionary<string, int> aur_helpers_priority = new()
    //{
    //	//! 请从1开始0被内部使用
    //	["yay"] = 2,
    //	["paru"] = 1
    //};

    // 初始化配置文件
    private static Dictionary<string, int>? aur_helpers_priority;
    private static List<string>? aur_helpers;

    private static bool flag_debug_mode = false;

    private static int Main(string[] args)
    {
        try
        {
            ConfigTools.InitializeConfig();

            // Check for null and assign value
            if (ConfigTools.AurHelpersPriority != null)
            {
                aur_helpers_priority = ConfigTools.AurHelpersPriority;
            }
            else
            {
                Logger.Error("Failed to load the config file.");
                return PROG_EXIT_ERROR;
            }
            aur_helpers = aur_helpers_priority.Keys.ToList();
        }
        catch (Exception ex)
        {
            Logger.Error($"An exception occurred while initializing the config: {ex.Message}");
            Logger.Error($"Stack Trace: {ex.StackTrace}");
            return PROG_EXIT_ERROR;
        }
        // 参数解析
        var command = args.FirstOrDefault();
        var commandArgs = args.Skip(1).ToList();

        // 获取目前使用的shell
        shell = GetCurrentShell();
        // 检查是否使用 Pacman
        bool pacmanFlag = commandArgs.Contains("-pacman");
        //string packageManager = CheckPackageManager(pacmanFlag);
        // 处理确认标志
        string confirmFlag = commandArgs.Contains("--noconfirm") || commandArgs.Contains("-y") ? "--noconfirm" : "";

        // 处理debug标志
        flag_debug_mode = commandArgs.Contains("-debug");

        // 移除已处理的标志
        commandArgs = commandArgs.Where(arg => arg != "--noconfirm" && arg != "-y" && arg != "-pacman" && arg != "-debug").ToList();

        Logger.Init(flag_debug_mode);

        for (int i = 0; i < commandArgs.Count; i++)
        {
            if (commandArgs[i] == "-helper")
            {
                string helper = commandArgs[i + 1];
                if (!aur_helpers.Contains(helper))
                {
                    aur_helpers.Add(helper);
                }
                aur_helpers_priority[helper] = 0;
            }
        }

        aur_helpers.Add("pacman");
        Dictionary<string, bool> aur_helpers_result = Utilities.AreCommandsAvailable(aur_helpers);
        if (aur_helpers_result["pacman"] is false)
        {
            Logger.Error("Can't find pacman in system.");
            return PROG_EXIT_ERROR;
        }
        aur_helpers.Remove("pacman");

        // 根据优先级对 AUR 助手进行排序
        var sortedHelpers = aur_helpers
            .Where(helper => aur_helpers_result[helper]) // 只保留存在的助手
            .OrderByDescending(helper => aur_helpers_priority[helper])
            .ToList();

        string packageManager = "pacman";
        if (sortedHelpers.Count <= 0)
        {
            Logger.Warning("Can't find any aur_helper in system");
        }
        else
        {
            packageManager = sortedHelpers[0];
        }
        // 如果未提供 Command，尝试将第一个 CommandArgs 作为 Command
        if (string.IsNullOrEmpty(command) && commandArgs.Count > 0)
        {
            if (IsPacmanCommand(commandArgs[0]))
            {
                command = commandArgs[0];
                commandArgs = commandArgs.Skip(1).ToList();
            }
            else
            {
                Handles.HandleShowUsage();
                return PROG_EXIT_ERROR;
            }
        }

        // 处理命令
        bool havePackage = false;
        string packageString = string.Empty;

        switch (command)
        {
            case "install":
            case "remove":
            case "-S":
            case "-R":
            case "-SyS":
                if (commandArgs.Count == 0)
                {
                    Logger.Error($"No package specified for {command}");
                    Handles.HandleShowUsage();
                    return PROG_EXIT_ERROR;
                }

                havePackage = true;
                commandArgs = PackageTranslate.TranslatePackage(commandArgs, confirmFlag != "");
                //? 对包名进行转译 替换Deb系的包名到Aur包名
                // "build-essential" -->"base-devel"
                packageString = string.Join(" ", commandArgs);
                Logger.Info($"Packages = {packageString}");
                break;

            case "search":
            case "show":
            case "-Ss":
            case "-Qi":
                if (commandArgs.Count == 0)
                {
                    Logger.Error($"No package specified for {command}");
                    Handles.HandleShowUsage();
                    return PROG_EXIT_ERROR;
                }

                havePackage = true;
                packageString = commandArgs[0];

                if (commandArgs.Count > 1)
                {
                    Logger.Error("Too many packages specified");
                    Logger.Warning($"Packages = {packageString}");
                }
                break;

            case "upgrade":
            case "-Syu":
                if (commandArgs.Count > 0)
                {
                    havePackage = true;
                    packageString = string.Join(" ", commandArgs);
                    Logger.Info($"Packages = {packageString}");
                }
                break;
        }

        // 执行命令
        switch (command)
        {
            case "update":
            case "-Sy":
                Handles.HandleUpdate(packageManager);
                break;

            case "upgrade":
            case "-Syu":
                Handles.HandleUpgrade(packageManager, havePackage, packageString, confirmFlag);
                break;

            case "install":
            case "-S":
                Handles.HandleInstall(packageManager, havePackage, packageString, confirmFlag);
                break;

            case "remove":
            case "-R":
                Handles.HandleRemove(packageManager, havePackage, packageString, confirmFlag);
                break;

            case "search":
            case "-Ss":
                Handles.HandleSearch(packageManager, havePackage, packageString);
                break;

            case "show":
            case "-Qi":
                Handles.HandleShow(packageManager, havePackage, packageString);
                break;

            case "list":
            case "-Q":
                Handles.HandleList(packageManager);
                break;

            case "clean":
            case "-Sc":
                Handles.HandleClean(packageManager, confirmFlag);
                break;

            case "help":
            case "-h":
                Handles.HandleShowUsage();
                break;

            case "autoremove":
                Handles.HandleAutoRemove(packageManager, havePackage, packageString, confirmFlag);
                break;
            default:
                if (command != null)
                {
                    //? 长得像pacman的命令直接透传
                    //? (?!-h)(?!--help)(?!--version)(-[A-Za-z-]*)
                    //? 符合pacman指令格式
                    if (System.Text.RegularExpressions.Regex.IsMatch(command, "(?!-h)(?!--help)(?!--version)(-[A-Za-z-]*)"))
                    {
                        string raw_command = command + string.Join(" ", commandArgs);
                        Logger.Info($"Will run {packageManager} {raw_command}");
                        Handles.HandleRAWcommand(packageManager, raw_command);
                        break;
                    }
                    Handles.HandleError(command);
                }
                else
                {
                    Handles.HandleShowUsage();
                }
                break;
        }
        return PROG_EXIT_SUCCESS;
    }

    private static string GetCurrentShell()
    {
        string? shell;
        if (IsWindows())
        {
            shell = Environment.GetEnvironmentVariable("ComSpec"); // 通常指向 cmd.exe
            if (Utilities.IsCommandAvailable("pwsh"))
            {
                shell = "pwsh";
            }
            else if (Utilities.IsCommandAvailable("powershell"))
            {
                shell = "powershell";
            }
            else if (string.IsNullOrEmpty(shell))
            {
                // 你可以根据需要处理没有找到的情况
                shell = "cmd.exe"; // 默认值
            }
        }
        else
        {
            // 从环境变量中获取当前 Shell
            shell = Environment.GetEnvironmentVariable("SHELL");
            if (string.IsNullOrEmpty(shell))
            {
                // 如果没有找到 SHELL 环境变量，使用默认的 bash
                shell = "/bin/bash";
            }
        }
        return shell;
    }

    private static bool IsPacmanCommand(string cmd)
    {
        return cmd.StartsWith("-S") || cmd.StartsWith("-R") || cmd.StartsWith("-Q") || cmd.StartsWith("-Syu") || cmd.StartsWith("-Sy") || cmd.StartsWith("-Sc") || cmd.StartsWith("-Ss") || cmd.StartsWith("-Qi") || cmd.StartsWith("-h");
    }

    public static void RunCommand(string command)
    {
        try
        {
            if (flag_debug_mode)
            {
                Logger.Debug(command);
            }
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = shell,
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
            if (IsWindows())
            {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                if (shell.Contains("cmd.exe"))
                {
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {

                            FileName = shell,
                            Arguments = $"/C \"{command}\"",
                            UseShellExecute = false,
                            CreateNoWindow = false // 不创建新窗口
                        }
                    };
                }
#pragma warning restore CS8602 // 解引用可能出现空引用。
                //? 如果shell是pwsh等 则和Linux调用方式一样
            }

            process.Start();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Logger.Error($"{ex.Message}");
        }
    }
    static bool IsWindows()
    {
        return Environment.OSVersion.Platform == PlatformID.Win32NT;
    }

}
