using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGameFramework 
{
    public static TGameFramework Instance { get; private set; }
    public static bool Initialized { get; private set; }
    private Dictionary<Type, BaseGameModule> m_modules = new Dictionary<Type, BaseGameModule>();

    public static void Initialize()
    {
        Instance = new TGameFramework();
    }
    public T GetModule<T>()where T : BaseGameModule
    {
        if(m_modules.TryGetValue(typeof(T),out BaseGameModule module))
        {
            return module as T;
        }
        return default(T);
    }
    public void AddModule(BaseGameModule module)
    {
        Type moduleType = module.GetType();
        if (m_modules.ContainsKey(moduleType))
        {
            return;
        }
        m_modules.Add(moduleType, module);
    }
}
