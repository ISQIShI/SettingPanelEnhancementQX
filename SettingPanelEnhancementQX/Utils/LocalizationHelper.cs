using Newtonsoft.Json;
using SettingPanelEnhancementQX.ModSetting.OptionsProvider;
using SodaCraft.Localizations;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace SettingPanelEnhancementQX.Utils
{
    public class LocalizationHelper
    {
        public const string KeyPrefix = nameof(SettingPanelEnhancementQX) + ".";

        public static Dictionary<string, string> OriginalLocalizationDict { get; } = new Dictionary<string, string>()
        {
            [$"{KeyPrefix}ModSettingButton"] = "[Mod]SettingPanelEnhancement",
            [$"{KeyPrefix}Option_Off"] = "Close",
            [$"{KeyPrefix}Option_On"] = "Open",
            [GetDropdownOptionLabelKey(SelectPageNumber.KEY)] = "Page Number",
        };

        public static void Init()
        {
            EnsureOriginalLanguageFileExist();
            LoadLanguageFile(LocalizationManager.CurrentLanguage);
            LocalizationManager.OnSetLanguage += OnLanguageChanged;
        }
        public static void Release()
        {
            LocalizationManager.OnSetLanguage -= OnLanguageChanged;
        }

        public static string GetDropdownOptionLabelKey(string optionKey)
        {
            return $"Options_{optionKey}";
        }

        private static void EnsureOriginalLanguageFileExist()
        {
            var path = GetLocalizationFilePath("English");
            if (!File.Exists(path))
            {
                // 获取文件所在目录
                string directory = Path.GetDirectoryName(path);

                // 自动创建所有不存在的文件夹
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 创建文件并写入初始内容
                File.WriteAllText(path, JsonConvert.SerializeObject(OriginalLocalizationDict, Formatting.Indented));
            }
        }

        private static void OnLanguageChanged(SystemLanguage language)
        {
            LoadLanguageFile(language);
        }

        private static void LoadLanguageFile(SystemLanguage language)
        {
            string languageName = language.ToString();
            LogHelper.Instance.LogTest($"开始加载语言文件，语言名：{languageName}");
            string path = GetLocalizationFilePath(languageName);
            if (!File.Exists(path))
            {
                languageName = "English";
                path = GetLocalizationFilePath(languageName);
                if (!File.Exists(path))
                {
                    return;
                }
            }
            LogHelper.Instance.LogTest($"正在加载语言文件：{path}");
            foreach (KeyValuePair<string, string> keyValuePair in JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path)))
            {
                LocalizationManager.SetOverrideText(keyValuePair.Key, keyValuePair.Value);
            }
            LogHelper.Instance.LogTest($"成功加载语言文件，语言名：{languageName}");
        }

        private static string GetLocalizationFilePath(string languageName)
        {
            // 获取当前执行程序集所在文件夹路径
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (directoryName == null) return null;
            return Path.Combine(directoryName, "Localization", $"{languageName}.json");
        }
    }
}
