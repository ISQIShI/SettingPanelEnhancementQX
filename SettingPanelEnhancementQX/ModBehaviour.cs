using SettingPanelEnhancementQX.ModSetting;
using SettingPanelEnhancementQX.Utils;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SettingPanelEnhancementQX
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public const string HarmonyId = nameof(SettingPanelEnhancementQX);

        public static ModBehaviour Instance { get; private set; }

        public HarmonyHelper HarmonyHelperObj { get; private set; }

        public LinkedList<GameObject> ChildGameObject { get; } = new LinkedList<GameObject>();
        protected override void OnAfterSetup()
        {
            base.OnAfterSetup();

            LocalizationHelper.Init();
            ModSettingCenter.Init();

            // 加载 Harmony 补丁
            var assembly = Assembly.GetExecutingAssembly();
            HarmonyHelper.CheckHarmonyVersion();
            HarmonyHelper.CheckHarmonyPatchClasses(assembly);
            if (HarmonyHelperObj == null) HarmonyHelperObj = new HarmonyHelper(HarmonyId);

            ChildGameObject.AddLast(new GameObject(nameof(SettingUIHelper), typeof(SettingUIHelper)));

            HarmonyHelperObj.HarmonyInstance.PatchAll(assembly);

            foreach (var gameObject in ChildGameObject)
            {
                gameObject.SetActive(false);
                gameObject.transform.SetParent(transform);
            }

            Instance = this;

            LogHelper.Instance.LogTest($"{nameof(SettingPanelEnhancementQX)} 已加载");
        }
        protected override void OnBeforeDeactivate()
        {
            base.OnBeforeDeactivate();

            LocalizationHelper.Release();
            ModSettingCenter.Release();

            // 卸载 Harmony 补丁
            HarmonyHelperObj.HarmonyInstance.UnpatchAll(HarmonyId);

            foreach (var gameObject in ChildGameObject)
            {
                if (gameObject != null) Destroy(gameObject);
            }
            ChildGameObject.Clear();

            Instance = null;

            LogHelper.Instance.LogTest($"{nameof(SettingPanelEnhancementQX)} 已卸载");
        }
    }
}
