using Serilog;
using Serilog.Configuration;
using Serilog.Debugging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace papt
{

    public static class Logger
    {
        public static void Init(bool debug_flag = false)
        {
            if (debug_flag)
            {
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .CreateLogger();
            }
            else
            {
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
                .CreateLogger();
            }
        }
        public static void Debug(string message)
        {
            Log.Debug(message);
        }

        public static void Info(string message)
        {
            Log.Information(message);
        }

        public static void Warning(string message)
        {
            Log.Warning(message);
        }

        public static void Error(string message)
        {
            Log.Error(message);
        }
    }
}