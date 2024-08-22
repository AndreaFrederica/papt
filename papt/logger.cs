using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace papt
{
	public static class Logger
	{
		public static void Trace(string message)
		{
			Log(message, ConsoleColor.Gray, "TRACE");
		}

		public static void Debug(string message)
		{
			Log(message, ConsoleColor.Blue, "DEBUG");
		}

		public static void Info(string message)
		{
			Log(message, ConsoleColor.White, "INFO");
		}

		public static void Warning(string message)
		{
			Log(message, ConsoleColor.Yellow, "WARNING");
		}

		public static void Error(string message)
		{
			Log(message, ConsoleColor.Red, "ERROR");
		}

		public static void Critical(string message)
		{
			Log(message, ConsoleColor.Magenta, "CRITICAL");
		}

		private static void Log(string message, ConsoleColor color, string level)
		{
			Console.ForegroundColor = color;
			Console.WriteLine($"[{DateTime.Now}] [{level}] {message}");
			Console.ResetColor();
		}
	}
}