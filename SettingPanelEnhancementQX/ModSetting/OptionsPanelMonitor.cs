using Duckov.Options.UI;
using HarmonyLib;
using SettingPanelEnhancementQX.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace SettingPanelEnhancementQX.ModSetting
{
    public class OptionsPanelMonitor : MonoBehaviour
    {
        public LinkedList<OptionsPanel_TabButton> ManagedTabButtons { get; } = new LinkedList<OptionsPanel_TabButton>();
        public List<OptionsPanel_TabButton> TabButtons { get; set; }

        public void Release()
        {
            if (TryGetComponent<OptionsPanel>(out var optionsPanel))
            {
                TabButtons.RemoveAll(tabButton => tabButton == null || ManagedTabButtons.Contains(tabButton));
                foreach (var tabButton in TabButtons)
                {
                    if (tabButton)
                    {
                        tabButton.gameObject.SetActive(true);
                    }
                }
                optionsPanel.SetSelection(TabButtons[0]);

                var tabFieldAccess = AccessTools.FieldRefAccess<OptionsPanel_TabButton, GameObject>("tab");
                foreach (var tabButton in ManagedTabButtons)
                {
                    if (tabButton)
                    {
                        ref GameObject panel = ref tabFieldAccess(tabButton);
                        if (panel)
                        {
                            GameObject.Destroy(panel);
                            panel = null;
                        }
                        GameObject.Destroy(tabButton.gameObject);
                    }
                }

                LogHelper.Instance.LogTest("OptionsPanelMonitor::Release，设置面板资源已释放完毕");
            }
        }

        private void OnEnable()
        {
            if (TabButtons == null) return;
            ModSettingCenter.CurrentOptionsPanelMonitor = this;
            ModSettingCenter.AfterOptionsPanelOpen(this);
        }

        private void OnDisable()
        {
            if (TabButtons == null) return;
            ModSettingCenter.CurrentOptionsPanelMonitor = null;
        }


        private void OnDestroy()
        {
            ModSettingCenter.Monitors.Remove(this);
        }
    }
}
