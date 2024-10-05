using Microsoft.VisualBasic;
using papt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;




internal class PackageManagerScript
{
	const int PROG_EXIT_SUCCESS = 0;
	const int PROG_EXIT_ERROR = 1;
	private static string? shell = null;

	//! 增加对多种AurHelper的支持
	private static Dictionary<string, int> aur_helpers_priority = new()
	{
		//! 请从1开始0被内部使用
		["yay"] = 2,
		["paru"] = 1
	};
	private static List<string> aur_helpers = aur_helpers_priority.Keys.ToList();

	private static bool flag_debug_mode = false;

	private static int Main(string[] args)
	{
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
		Dictionary<string, bool> aur_helpers_result = AreCommandsAvailable(aur_helpers);
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
		if (sortedHelpers.Count > 0)
		{
			packageManager = sortedHelpers[0];
			Logger.Warning("Can't find any aur_helper in system");
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
				if (commandArgs.Contains("build-essential")){
					Logger.Warning("Can you want install base-devel? [Y/n]");
					if(confirmFlag != "" || ISConsoleInputY(true)){
						commandArgs.Remove("build-essential");
						commandArgs.Add("base-devel");
					}
				}
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
					if (System.Text.RegularExpressions.Regex.IsMatch(command, "(?!-h)(?!--help)(?!--version)(-[A-Za-z-]*)")){
						string raw_command = command + string.Join(" ",commandArgs);
						Logger.Info($"Will run {packageManager} {raw_command}");
						Handles.HandleRAWcommand(packageManager, raw_command);
					}
					Handles.HandleError(command);
					break;
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
			if (IsCommandAvailable("pwsh"))
			{
				shell = "pwsh";
			}
			else if (IsCommandAvailable("powershell"))
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

	private static string CheckPackageManager(bool pacmanFlag)
	{
		if (pacmanFlag || !IsCommandAvailable("yay"))
		{
			Logger.Warning("Using pacman instead of yay.");
			return "pacman";
		}
		return "yay";
	}

	private static bool IsCommandAvailable(string command)
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

	private static Dictionary<string, bool> AreCommandsAvailable(IEnumerable<string> commands)
	{
		var result = new Dictionary<string, bool>();

		try
		{
			using (var process = new Process())
			{
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
	static bool ISConsoleInputY(bool default_val){
		String? line = Console.ReadLine();
		if(line != null && (line == "Y" || line == "y")){
			return true;
		}else if(line == ""){
			return default_val;
		}
		return false;
	}
}
