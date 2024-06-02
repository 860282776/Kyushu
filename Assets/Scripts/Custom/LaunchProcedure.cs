using Config;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LaunchProcedure : BaseProcedure
{
    GameObject panel;
    public override async Task OnEnterProcedure(object value)
    {
        await LoadConfigs();
        await ChangeProcedure<LoginProcedure>();
    }
    private async Task LoadConfigs()
    {
        //UnityLog.Info("===>º”‘ÿ≈‰÷√");
        ConfigManager.LoadAllConfigsByAddressable("Assets/BundleAssets/Config");
        //#if UNITY_EDITOR
        //            string path = "Assets/BundleAssets/Config";
        //            ConfigManager.LoadAllConfigsByFile(path);
        //            await Task.Yield();
        //#else
        //            string path = $"{UnityEngine.Application.streamingAssetsPath}/AssetBundles";
        //            string subFolder = $"Datas/Config";
        //            await ConfigManager.LoadAllConfigsByBundle(path, subFolder);
        //#endif
        //GlobalConfig.InitGlobalConfig();
        //BuffConfig.ParseConfig();
        //SkillConfig.ParseConfig();
        //BulletConfig.ParseConfig();
        //SpellFieldConfig.ParseConfig();
        //I18NConfig.ParseConfig();
        
        await Task.Yield();
        //UnityLog.Info("<===≈‰÷√º”‘ÿÕÍ±œ");
    }
}
