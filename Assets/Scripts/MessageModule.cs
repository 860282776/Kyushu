using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public class MessageModule : BaseGameModule
{
    private Dictionary<Type, List<object>> globalMessageHandlers;
    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        LoadAllMessageHandlers();
    }

    private void LoadAllMessageHandlers()
    {
        globalMessageHandlers = new Dictionary<Type, List<object>>();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            if (type.IsAbstract)
                continue;
            MessageHandlerAttribute messageHandler = type.GetCustomAttribute<MessageHandlerAttribute>();
            if (messageHandler != null)
            {
                IMessageHandler handler = Activator.CreateInstance(type) as IMessageHandler;
                if (!globalMessageHandlers.ContainsKey(handler.GetHandlerType()))
                    globalMessageHandlers.Add(handler.GetHandlerType(), new List<object>());
                globalMessageHandlers[handler.GetHandlerType()].Add(handler);
            }
        }
    }
    public async Task Post<T>(T arg)where T : struct
    {
        if(globalMessageHandlers.TryGetValue(typeof(T),out List<object> globalHandlerList))
        {
            foreach (var handler in globalHandlerList)
            {
                if (!(handler is MessageHandler<T> messageHandler))
                    continue;
                await messageHandler.HandleMessage(arg);
            }
        }
    }
}
