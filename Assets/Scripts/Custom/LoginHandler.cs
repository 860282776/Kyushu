using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginHandler : MessageHandler<MessageType.Login>
{
    public override async Task HandleMessage(MessageType.Login arg)
    {
        Debug.Log("全局消息触发");
        await Task.Yield();
    }
}
