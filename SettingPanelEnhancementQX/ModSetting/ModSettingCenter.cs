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

        private static int _currentPageNumber = -1;

        public static int CurrentPageNumber
        {
            get => _currentPageNumber;
            set
            {
                if (_currentPageNumber != value)
                {
                    _currentPageNumber = value;
                    int validIndex = 0;
                    if (_currentTabButtonsRef.TryGetTarget(out var tabButtons))
                    {
                        for (int i = 0; i < tabButtons.Count; i++)
                        {
                            var tabButton = tabButtons[i];

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
        }

        public static int TotalPageNumber { get; private set; } = 1;

        private static WeakReference<List<OptionsPanel_TabButton>> _currentTabButtonsRef = new WeakReference<List<OptionsPanel_TabButton>>(null);

        public static LinkedList<(OptionsPanel_TabButton tabButton, OptionsPanel optionsPanel)> TabButtons { get; } = new LinkedList<(OptionsPanel_TabButton tabButton, OptionsPanel optionsPanel)>();


        public static void Init()
        {
            _currentPageNumber = 0;
        }

        public static void Release()
        {
            var tabFieldAccess = AccessTools.FieldRefAccess<OptionsPanel_TabButton, GameObject>("tab");
            foreach (var (tabButton, optionsPanel) in TabButtons)
            {
                if (tabButton)
                {
                    if (optionsPanel)
                    {
                        var tabButtons = AccessTools.FieldRefAccess<OptionsPanel, List<OptionsPanel_TabButton>>(optionsPanel, "tabButtons");
                        tabButtons.Remove(tabButton);
                        if (optionsPanel.GetSelection() == tabButton)
                        {
                            optionsPanel.SetSelection(tabButtons.FirstOrDefault());
                        }
                    }

                    ref GameObject panel = ref tabFieldAccess(tabButton);
                    if (panel)
                    {
                        GameObject.Destroy(panel);
                        panel = null;
                    }
                    GameObject.Destroy(tabButton.gameObject);
                }
            }
            TabButtons.Clear();
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
            TabButtons.AddLast((tabButton, __instance));

            // test
            for (int i = 0; i < 12; i++)
            {
                settingPanelObj = SettingUIHelper.Instance.CreateSettingPanel(__instance, SettingUIHelper.DefaultSettingTabButtonName + i.ToString(), $"{LocalizationHelper.KeyPrefix}ModSettingButton", SettingUIHelper.DefaultSettingPanelName + i.ToString(), out tabButton);
                if (!settingPanelObj)
                {
                    LogHelper.Instance.LogError($"在 {nameof(ModSettingCenter)}.{nameof(InitSettingUI)} 中无法创建设置面板");
                    return;
                }
                SettingUIHelper.Instance.CreateDropdown<SelectPageNumber>(settingPanelObj);
                ___tabButtons.Add(tabButton);
                TabButtons.AddLast((tabButton, __instance));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsPanel), "Setup")]
        public static void AfterOptionsPanelOpen(OptionsPanel __instance, List<OptionsPanel_TabButton> ___tabButtons)
        {
            Debug.LogWarning("OptionsPanel OnOpen Postfix Triggered");
            _currentTabButtonsRef.SetTarget(___tabButtons);
            // 更新总页数
            TotalPageNumber = (int)Math.Ceiling((double)___tabButtons.Count / DisplayedTabCount);
            if (CurrentPageNumber >= TotalPageNumber || ___tabButtons.Count(tabButton => tabButton != null && tabButton.gameObject.activeSelf) > DisplayedTabCount || _currentPageNumber < 0)
            {
                Debug.LogWarning("OptionsPanel OnOpen Postfix Reset CurrentPageNumber to 0");
                CurrentPageNumber = 0;
                __instance.SetSelection(___tabButtons[0]);
            }
        }
    }
}
