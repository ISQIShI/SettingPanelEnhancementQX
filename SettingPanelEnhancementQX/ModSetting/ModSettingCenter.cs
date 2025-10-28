using Duckov.Options.UI;
using HarmonyLib;
using SettingPanelEnhancementQX.ModSetting.OptionsProvider;
using SettingPanelEnhancementQX.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SettingPanelEnhancementQX.ModSetting
{
    [HarmonyPatch]
    public class ModSettingCenter
    {

        public static int DisplayedTabCount { get; } = 5;

        private static int _currentPageNumber = 0;

        public static int CurrentPageNumber
        {
            get => _currentPageNumber;
            set
            {
                _currentPageNumber = value;
                if (CurrentOptionsPanelMonitor)
                {
                    int validIndex = 0;
                    for (int i = 0; i < CurrentOptionsPanelMonitor.TabButtons.Count; i++)
                    {
                        var tabButton = CurrentOptionsPanelMonitor.TabButtons[i];

                        if (tabButton)
                        {
                            if (tabButton.gameObject.name == SettingUIHelper.DefaultSettingTabButtonName)
                            {
                                tabButton.gameObject.SetActive(true);
                                tabButton.transform.SetAsLastSibling();
                            }
                            else
                            {
                                tabButton.gameObject.SetActive((validIndex / (DisplayedTabCount - 1)) == _currentPageNumber);
                                validIndex++;
                            }
                        }
                    }
                }
            }
        }

        public static int TotalPageNumber { get; private set; } = 1;

        public static OptionsPanelMonitor CurrentOptionsPanelMonitor { get; set; }

        public static LinkedList<OptionsPanelMonitor> Monitors { get; } = new LinkedList<OptionsPanelMonitor>();


        public static void Init()
        {
            _currentPageNumber = 0;
        }

        public static void Release()
        {
            var tabFieldAccess = AccessTools.FieldRefAccess<OptionsPanel_TabButton, GameObject>("tab");
            foreach (var monitor in Monitors)
            {
                if (monitor)
                {
                    monitor.Release();
                    GameObject.Destroy(monitor);
                }
            }
            Monitors.Clear();
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionsPanel), "Setup")]
        public static void InitSettingUI(OptionsPanel __instance, List<OptionsPanel_TabButton> ___tabButtons)
        {

            if (___tabButtons.Any((tabButton) => tabButton != null && tabButton.gameObject.name == SettingUIHelper.DefaultSettingTabButtonName))
            {
                return;
            }

            if (!SettingUIHelper.Instance)
            {
                LogHelper.Instance.LogError($"在 {nameof(ModSettingCenter)}.{nameof(InitSettingUI)} 中无法获取到 {nameof(SettingUIHelper)} 实例");
                return;
            }
            if (!SettingUIHelper.Instance.InitSettingUITemplate(___tabButtons))
            {
                return;
            }
            GameObject settingPanelObj = SettingUIHelper.Instance.CreateSettingPanel(__instance, SettingUIHelper.DefaultSettingTabButtonName, $"{LocalizationHelper.KeyPrefix}ModSettingButton", SettingUIHelper.DefaultSettingPanelName, out var tabButton);
            if (!settingPanelObj)
            {
                LogHelper.Instance.LogError($"在 {nameof(ModSettingCenter)}.{nameof(InitSettingUI)} 中无法创建设置面板");
                return;
            }
            SettingUIHelper.Instance.CreateDropdown<SelectPageNumber>(settingPanelObj);

            ___tabButtons.Add(tabButton);
            var monitor = __instance.gameObject.AddComponent<OptionsPanelMonitor>();
            monitor.enabled = false;
            monitor.ManagedTabButtons.AddLast(tabButton);

            // test
            //for (int i = 0; i < 12; i++)
            //{
            //    settingPanelObj = SettingUIHelper.Instance.CreateSettingPanel(__instance, $"测试面板{i.ToString()}", $"测试面板{i.ToString()}", SettingUIHelper.DefaultSettingPanelName + i.ToString(), out tabButton);
            //    if (!settingPanelObj)
            //    {
            //        LogHelper.Instance.LogError($"在 {nameof(ModSettingCenter)}.{nameof(InitSettingUI)} 中无法创建设置面板");
            //        return;
            //    }
            //    SettingUIHelper.Instance.CreateDropdown<SelectPageNumber>(settingPanelObj);
            //    ___tabButtons.Add(tabButton);
            //    monitor.ManagedTabButtons.AddLast(tabButton);
            //}

            monitor.TabButtons = ___tabButtons;
            Monitors.AddLast(monitor);
            monitor.enabled = true;
        }


        [HarmonyFinalizer]
        [HarmonyPatch(typeof(OptionsPanel), "Setup")]
        public static void RefreshTabButtons(OptionsPanel __instance, List<OptionsPanel_TabButton> ___tabButtons)
        {
            // 更新总页数
            TotalPageNumber = (int)Math.Ceiling((double)(___tabButtons.Count - 1) / (DisplayedTabCount - 1));
            CurrentPageNumber = 0;
            __instance.SetSelection(___tabButtons[0]);
        }

        public static void AfterOptionsPanelOpen(OptionsPanelMonitor monitor)
        {
            // 更新总页数
            TotalPageNumber = (int)Math.Ceiling((double)(monitor.TabButtons.Count - 1) / (DisplayedTabCount - 1));
            if (CurrentPageNumber >= TotalPageNumber || monitor.TabButtons.Count(tabButton => tabButton != null && tabButton.gameObject.activeSelf) > DisplayedTabCount)
            {
                CurrentPageNumber = 0;
                monitor.GetComponent<OptionsPanel>()?.SetSelection(monitor.TabButtons[0]);
            }
        }
    }
}
