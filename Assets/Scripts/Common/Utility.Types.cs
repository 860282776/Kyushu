using System;
using System.Collections.Generic;
using System.Reflection;

namespace TGame.Common
{
	public static partial class Utility
    {
        public static class Types
        {
            public readonly static Assembly GAME_CSHARP_ASSEMBLY = Assembly.Load("Assembly-CSharp");
            public readonly static Assembly GAME_EDITOR_ASSEMBLY = Assembly.Load("Assembly-CSharp-Editor");

            /// <summary>
            /// 获取所有能从某个类型分配的属性列表
            /// 获取指定类型中所有可以赋值的属性
            /// </summary>
            public static List<PropertyInfo> GetAllAssignablePropertiesFromType(Type basePropertyType, Type objType, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            {
                List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
                PropertyInfo[] properties = objType.GetProperties(bindingFlags);
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    if (basePropertyType.IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        propertyInfos.Add(propertyInfo);
                    }
                }
                return propertyInfos;
            }

            /// <summary>
            /// 获取某个类型的所有子类型
            /// </summary>
            /// <param name="baseClass">父类</param>
            /// <param name="assemblies">程序集,如果为null则查找当前程序集</param>
            /// <returns></returns>
            public static List<Type> GetAllSubclasses(Type baseClass, bool allowAbstractClass, params Assembly[] assemblies)
            {
                // 初始化一个列表用于存储子类类型
                List<Type> subclasses = new List<Type>();

                // 如果 assemblies 为空，设置为调用该方法的程序集
                if (assemblies == null)
                {
                    assemblies = new Assembly[] { Assembly.GetCallingAssembly() };
                }

                // 遍历所有提供的程序集
                foreach (var assembly in assemblies)
                {
                    // 遍历当前程序集中的所有类型
                    foreach (var type in assembly.GetTypes())
                    {
                        // 检查当前类型是否是基类或基类的子类
                        if (!baseClass.IsAssignableFrom(type))
                            continue;

                        // 如果不允许抽象类，并且当前类型是抽象的，跳过
                        if (!allowAbstractClass && type.IsAbstract)
                            continue;

                        // 将符合条件的类型添加到子类列表中
                        subclasses.Add(type);
                    }
                }

                // 返回所有符合条件的子类类型列表
                return subclasses;
            }
        }
    }
}