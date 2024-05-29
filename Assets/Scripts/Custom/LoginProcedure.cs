using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginProcedure : BaseProcedure
{
    public override async Task OnEnterProcedure(object value)
    {
        GameManager.UI.OpenUI(Koakuma.Game.UI.UIViewID.LoginUI);
        await Task.Yield();
    }
    public override async Task OnLeaveProcedure()
    {
        GameManager.UI.CloseUI(Koakuma.Game.UI.UIViewID.LoginUI);
        await Task.Yield();
    }
}
