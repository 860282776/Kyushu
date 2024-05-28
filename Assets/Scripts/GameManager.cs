 using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using TGame.UI;
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 资源组件
    /// </summary>
    //[Module(1)]
    //public static AssetModule Asset { get => TGameFramework.Instance.GetModule<AssetModule>(); }
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

    [Module(7)]
    public static MessageModule Message { get => TGameFramework.Instance.GetModule<MessageModule>(); }

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
        List<ModuleAttribute> moduleAttrs = new List<ModuleAttribute>();
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        Type baseCompType = typeof(BaseGameModule);
        for (int i = 0; i < propertyInfos.Length; i++)
        {
            PropertyInfo property = propertyInfos[i];
            if (!baseCompType.IsAssignableFrom(property.PropertyType))
                continue;

            object[] attrs = property.GetCustomAttributes(typeof(ModuleAttribute), false);
            if (attrs.Length == 0)
                continue;

            Component comp = GetComponentInChildren(property.PropertyType);
            if (comp == null)
            {
                Debug.LogError($"Can't Find GameModule:{property.PropertyType}");
                continue;
            }

            ModuleAttribute moduleAttr = attrs[0] as ModuleAttribute;
            moduleAttr.Module = comp as BaseGameModule;
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

