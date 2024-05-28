using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginProcedure : BaseProcedure
{
    GameObject panel;
    public override async Task OnEnterProcedure(object value)
    {
        panel = GameObject.Instantiate(Resources.Load<GameObject>("Login"), GameObject.Find("Canvas").transform);
        GameManager.UI.OpenUI(Koakuma.Game.UI.UIViewID.LoginUI);
        await Task.Yield();
    }
    public override async Task OnLeaveProcedure()
    {
        GameObject.Destroy(panel);
        GameManager.UI.CloseUI(Koakuma.Game.UI.UIViewID.LoginUI);
        await Task.Yield();
    }
}
