using SettingPanelEnhancementQX.Utils;

namespace SettingPanelEnhancementQX.ModSetting.OptionsProvider
{
    public class SelectPageNumber : OptionsProviderBase
    {
        public const string KEY = LocalizationHelper.KeyPrefix + nameof(SelectPageNumber);
        public override string Key => KEY;
        private void Awake()
        {
            LevelManager.OnLevelInitialized += this.RefreshOnLevelInited;
        }

        private void OnDestroy()
        {
            LevelManager.OnLevelInitialized -= this.RefreshOnLevelInited;
        }
        private void RefreshOnLevelInited()
        {
            int index = ModSettingCenter.CurrentPageNumber;
            Set(index);
        }

        public override string GetCurrentOption()
        {
            return ModSettingCenter.CurrentPageNumber.ToString();
        }

        public override string[] GetOptions()
        {
            int targetNumber = ModSettingCenter.TotalPageNumber;
            string[] options = new string[targetNumber];
            for (int i = 0; i < targetNumber; i++)
            {
                options[i] = i.ToString();
            }
            return options;
        }

        public override void Set(int index)
        {
            ModSettingCenter.CurrentPageNumber = index;
        }
    }
}
