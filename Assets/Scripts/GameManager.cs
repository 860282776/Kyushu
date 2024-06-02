 using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TGame.UI;
using TGame.Asset;
using TGame.ECS;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 资源组件
    /// </summary>
    [Module(1)]
    public static AssetModule Asset { get => TGameFramework.Instance.GetModule<AssetModule>(); }
    /// <summary>
    /// 流程组件
    /// </summary>
    [Module(2)]
    public static ProcedureModule Procedure { get => TGameFramework.Instance.GetModule<ProcedureModule>(); }

    [Module(3)]
    public static UIModule UI { get => TGameFramework.Instance.GetModule<UIModule>(); }

    //[Module(4)]
    //public static TimeModule Time { get => TGameFramework.Instance.GetModule<TimeModule>(); }
    //[Module(5)]
    //public static AudioModule Audio { get => TGameFramework.Instance.GetModule<AudioModule>(); }

    [Module(6)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }
    [Module(7)]
    public static ECSModule ECS { get => TGameFramework.Instance.GetModule<ECSModule>(); }

    private bool activing;

    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
        if (TGameFramework.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Application.logMessageReceived += OnReceiveLog;
        TGameFramework.Initialize();
        StartupModules();
        TGameFramework.Instance.InitModules();

    }


    private void Start()
    {
        TGameFramework.Instance.StartModules();
        Procedure.StartProcedure().Coroutine();
    }

    private void Update()
    {
        TGameFramework.Instance.Update();
    }

    private void LateUpdate()
    {
        TGameFramework.Instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        TGameFramework.Instance.FixedUpdate();
    }

    private void OnDestroy()
    {
        if (activing)
        {
            Application.logMessageReceived -= OnReceiveLog;
            TGameFramework.Instance.Destroy();
        }
    }

    private void OnApplicationQuit()
    {
        //UnityLog.Teardown();
    }

    public void StartupModules()
    {
        // 初始化一个列表用于存储 ModuleAttribute 实例
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();

        // 获取当前类型的所有属性，包括公有、非公有和静态属性
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        // 获取 BaseGameModule 类型
        Type baseCompType = typeof(BaseGameModule);

        // 遍历所有属性信息
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];

            // 检查属性的类型是否为 BaseGameModule 或其子类
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
                continue;

            // 获取属性上的所有 ModuleAttribute 特性
            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);

            // 如果没有 ModuleAttribute 特性，跳过当前属性
            if (attrs.Length == 0)
                continue;

            // 在子对象中获取属性类型的组件
            Component comp = GetComponentInChildren(property.PropertyType);

            // 如果组件未找到，输出错误日志并跳过当前属性
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule: {property.PropertyType}");
                continue;
            }

            // 获取第一个 ModuleAttribute 特性并设置其 Module 属性
            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;

            // 将特性添加到 moduleAttrs 列表中
            moduleAttrs.Add(moduleAttr);
        }

        moduleAttrs.Sort((a, b) =>
        {
            return a.Priority - b.Priority;
        });

        for (int i = 0; i < moduleAttrs.Count; i++)
        {
            TGameFramework.Instance.AddModule(moduleAttrs[i].Module);
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; private set; }
        /// <summary>
        /// 模块
        /// </summary>
        public BaseGameModule Module { get; set; }

        /// <summary>
        /// 添加该特性才会被当作模块
        /// </summary>
        /// <param name="priority">控制器优先级,数值越小越先执行</param>
        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }

        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    private void OnReceiveLog(string condition, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
            if (type == LogType.Exception)
            {
                UnityLog.Fatal($"{condition}\n{stackTrace}");
            }
#endif
    }
}

