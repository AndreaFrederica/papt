using Json5;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace Papt
{
    public static class ConfigTools
    {
        private static readonly string ConfigFilePath = GetConfigFilePath();
        public static Dictionary<string, int>? AurHelpersPriority { get; private set; }

        /// <summary>
        /// 获取配置文件路径，根据操作系统和用户适配
        /// </summary>
        private static string GetConfigFilePath()
        {
            string configFileName = "papt.json5";
            string configDirectory;
            string homeDirectory;

            if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                // 检查是否为 root 用户
                if (Environment.GetEnvironmentVariable("USER") == "root")
                {
                    configDirectory = "/etc/papt";
                }
                else
                {
                    homeDirectory = Environment.GetEnvironmentVariable("HOME")!;
                    if (string.IsNullOrEmpty(homeDirectory))
                    {
                        throw new InvalidOperationException("Unable to determine the home directory.");
                    }
                    configDirectory = Path.Combine(homeDirectory, ".config", "papt");
                }
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                homeDirectory = Environment.GetEnvironmentVariable("USERPROFILE")!;
                if (string.IsNullOrEmpty(homeDirectory))
                {
                    throw new InvalidOperationException("Unable to determine the home directory.");
                }
                configDirectory = Path.Combine(homeDirectory, "papt");
            }
            else
            {
                throw new InvalidOperationException("Unsupported platform");
            }

            return Path.Combine(configDirectory, configFileName);
        }

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public static void InitializeConfig()
        {
            try
            {
                // 检查配置文件是否存在，不存在则创建默认文件夹
                if (!File.Exists(ConfigFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath)!);
                    File.WriteAllText(ConfigFilePath, RawDefaultConfig.DefaultConfigContent);
                    Logger.Info($"Default config file created at: {ConfigFilePath}");
                }

                // 加载配置文件
                LoadConfig();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error during config initialization: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载配置文件并解析
        /// </summary>
        private static void LoadConfig()
        {
            try
            {
                string json5Content = File.ReadAllText(ConfigFilePath);
                var json5Object = Json5.Json5.Parse(json5Content) as Json5Object;

                if (json5Object == null)
                {
                    Logger.Warning("Invalid config file format. Unable to parse.");
                    return;
                }

                AurHelpersPriority = new Dictionary<string, int>();

                // 从 JSON5 配置中读取 AUR Helper 优先级
                foreach (var key in json5Object.Keys)
                {
                    var value = json5Object[key];
                    if (value is Json5Number numberValue)
                    {
                        AurHelpersPriority[key] = (int)(double)numberValue;
                    }
                    else
                    {
                        Logger.Warning($"Invalid value type for key '{key}', expected a number.");
                    }
                }

                Logger.Info("Config file successfully loaded.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while loading config file: {ex.Message}");
            }
        }
    }
}
