using papt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

internal class PackageManagerScript
{
	private static string shell = "/bin/bash";
	private static bool flag_debug_mode = false;

	private static void Main(string[] args)
	{
		// 参数解析
		var command = args.FirstOrDefault();
		var commandArgs = args.Skip(1).ToList();

		// 获取目前使用的shell
		shell = GetCurrentShell();

		// 检查是否使用 Pacman
		bool pacmanFlag = commandArgs.Contains("-pacman");
		string packageManager = CheckPackageManager(pacmanFlag);

		// 处理确认标志
		string confirmFlag = commandArgs.Contains("--noconfirm") || commandArgs.Contains("-y") ? "--noconfirm" : "";

		// 处理debug标志
		flag_debug_mode= commandArgs.Contains("-debug");

		// 移除已处理的标志
		commandArgs = commandArgs.Where(arg => arg != "--noconfirm" && arg != "-y" && arg != "-pacman" && arg != "-debug").ToList();

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
				ShowUsage();
				return;
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
					ShowUsage();
					return;
				}

				havePackage = true;
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
					ShowUsage();
					return;
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
				HandleUpdate(packageManager);
				break;

			case "upgrade":
			case "-Syu":
				HandleUpgrade(packageManager, havePackage, packageString, confirmFlag);
				break;

			case "install":
			case "-S":
				HandleInstall(packageManager, havePackage, packageString, confirmFlag);
				break;

			case "remove":
			case "-R":
				HandleRemove(packageManager, havePackage, packageString, confirmFlag);
				break;

			case "search":
			case "-Ss":
				HandleSearch(packageManager, havePackage, packageString);
				break;

			case "show":
			case "-Qi":
				HandleShow(packageManager, havePackage, packageString);
				break;

			case "list":
			case "-Q":
				HandleList(packageManager);
				break;

			case "clean":
			case "-Sc":
				HandleClean(packageManager, confirmFlag);
				break;

			case "help":
			case "-h":
				ShowUsage();
				break;

			default:
				HandleError(command);
				break;
		}
	}

	private static string GetCurrentShell()
	{
		// 从环境变量中获取当前 Shell
		string shell = Environment.GetEnvironmentVariable("SHELL");
		if (string.IsNullOrEmpty(shell))
		{
			// 如果没有找到 SHELL 环境变量，使用默认的 bash
			shell = "/bin/bash";
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
			Process.Start(new ProcessStartInfo
			{
				FileName = "which",
				Arguments = command,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			})?.WaitForExit();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static bool IsPacmanCommand(string cmd)
	{
		return cmd.StartsWith("-S") || cmd.StartsWith("-R") || cmd.StartsWith("-Q") || cmd.StartsWith("-Syu") || cmd.StartsWith("-Sy") || cmd.StartsWith("-Sc") || cmd.StartsWith("-Ss") || cmd.StartsWith("-Qi") || cmd.StartsWith("-h");
	}

	private static void ShowUsage()
	{
		//Console.WriteLine("Usage: <command> [options] [packages]");
		// 显示更多帮助信息
		HelpUtility.ShowUsage();
	}

	private static void HandleUpdate(string packageManager)
	{
		RunCommand($"{packageManager} -Sy");
	}

	private static void HandleUpgrade(string packageManager, bool havePackage, string packageString, string confirmFlag)
	{
		if (havePackage)
		{
			RunCommand($"{packageManager} -Syu {packageString} {confirmFlag}");
		}
		else
		{
			RunCommand($"{packageManager} -Su {confirmFlag}");
		}
	}

	private static void HandleInstall(string packageManager, bool havePackage, string packageString, string confirmFlag)
	{
		if (!havePackage)
		{
			ShowUsage();
			return;
		}
		RunCommand($"{packageManager} -S {packageString} {confirmFlag}");
	}

	private static void HandleRemove(string packageManager, bool havePackage, string packageString, string confirmFlag)
	{
		if (!havePackage)
		{
			ShowUsage();
			return;
		}
		RunCommand($"{packageManager} -R {packageString} {confirmFlag}");
	}

	private static void HandleSearch(string packageManager, bool havePackage, string packageString)
	{
		if (!havePackage)
		{
			ShowUsage();
			return;
		}
		RunCommand($"{packageManager} -Ss {packageString}");
	}

	private static void HandleShow(string packageManager, bool havePackage, string packageString)
	{
		if (!havePackage)
		{
			ShowUsage();
			return;
		}
		RunCommand($"{packageManager} -Qi {packageString}");
	}

	private static void HandleList(string packageManager)
	{
		RunCommand($"{packageManager} -Q");
	}

	private static void HandleClean(string packageManager, string confirmFlag)
	{
		RunCommand($"{packageManager} -Sc {confirmFlag}");
	}

	private static void HandleError(string command)
	{
		Logger.Error($"Unknown command '{command}'");
		ShowUsage();
	}

	//static void RunCommand(string command)
	//{
	//	try
	//	{
	//		var process = new Process
	//		{
	//			StartInfo = new ProcessStartInfo
	//			{
	//				FileName = "/bin/bash",
	//				Arguments = $"-c \"{command}\"",
	//				RedirectStandardOutput = true,
	//				UseShellExecute = false,
	//				CreateNoWindow = true
	//			}
	//		};

	//		process.Start();
	//		string result = process.StandardOutput.ReadToEnd();
	//		process.WaitForExit();

	//		Console.WriteLine(result);
	//	}
	//	catch (Exception ex)
	//	{
	//		Console.WriteLine($"Error: {ex.Message}");
	//	}
	//}
	private static void RunCommand(string command)
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
					UseShellExecute = true,
					CreateNoWindow = false
				}
			};

			process.Start();
			process.WaitForExit();
		}
		catch (Exception ex)
		{
			Logger.Error($"{ex.Message}");
		}
	}
}