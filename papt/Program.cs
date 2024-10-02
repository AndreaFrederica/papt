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
				HandleShowUsage();
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
					HandleShowUsage();
					return PROG_EXIT_ERROR;
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
					HandleShowUsage();
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
				HandleShowUsage();
				break;

			default:
				if (command != null)
				{
					HandleError(command);
				}
				else
				{
					HandleShowUsage();
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
			if (string.IsNullOrEmpty(shell))
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

	private static void HandleShowUsage()
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
			HandleShowUsage();
			return;
		}
		RunCommand($"{packageManager} -S {packageString} {confirmFlag}");
	}

	private static void HandleRemove(string packageManager, bool havePackage, string packageString, string confirmFlag)
	{
		if (!havePackage)
		{
			HandleShowUsage();
			return;
		}
		RunCommand($"{packageManager} -R {packageString} {confirmFlag}");
	}

	private static void HandleSearch(string packageManager, bool havePackage, string packageString)
	{
		if (!havePackage)
		{
			HandleShowUsage();
			return;
		}
		RunCommand($"{packageManager} -Ss {packageString}");
	}

	private static void HandleShow(string packageManager, bool havePackage, string packageString)
	{
		if (!havePackage)
		{
			HandleShowUsage();
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
		HandleShowUsage();
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
					CreateNoWindow = true
				}
			};
			if(IsWindows()){
				if (shell.Contains("cmd.exe"))
				{
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
							
                            FileName = shell,
                            Arguments = $"/C \"{command}\"",
                            //Arguments = $"-h",
                            UseShellExecute = false,
							//RedirectStandardInput = true, // 重定向标准输入
							//RedirectStandardOutput = true, // 重定向标准输出
							//RedirectStandardError = true, // 重定向标准错误
							CreateNoWindow = false // 不创建新窗口
                        }
                    };
					//process.OutputDataReceived += (sender, args) =>
					//{
					//	if (!string.IsNullOrEmpty(args.Data))
					//	{
					//		Console.WriteLine(args.Data); // 输出数据
					//	}
					//};

					//process.ErrorDataReceived += (sender, args) =>
					//{
					//	if (!string.IsNullOrEmpty(args.Data))
					//	{
					//		Console.Error.WriteLine(args.Data); // 输出错误
					//	}
					//};
				}
				//? 如果shell是pwsh等 则和Linux调用方式一样
				//TODO 但是还会创建一个新窗口
				// 不使用shell创建现在直接没反应了

			}

			process.Start();
			//if (IsWindows())
			//{
			//	process.BeginOutputReadLine(); // 开始异步读取输出
			//	process.BeginErrorReadLine(); // 开始异步读取错误

			//	// 允许从标准输入写入数据
			//	using (var writer = process.StandardInput)
			//	{
			//		if (writer.BaseStream.CanWrite)
			//		{
			//			string userInput;
			//			while ((userInput = Console.ReadLine()) != null) // 从控制台读取输入
			//			{
			//				writer.WriteLine(userInput); // 将输入写入进程
			//			}
			//		}
			//	}
			//}
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