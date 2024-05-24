using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Module(7)]
    public static MessageModule Message { get; }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ModuleAttribute : Attribute, IComparable<ModuleAttribute>
    {
        public int Priority { get; private set; }
        public BaseGameModule Module { get; set; }

        public ModuleAttribute(int priority)
        {
            Priority = priority;
        }

        int IComparable<ModuleAttribute>.CompareTo(ModuleAttribute other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}
