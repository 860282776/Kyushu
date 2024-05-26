using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button button;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() =>
        {
            GameManager.Message.Post(new MessageType.Login()).Coroutine();
            GameManager.Message.Subscribe<MessageType.Login>(async (msg) =>
            {
                Debug.Log("本地消息触发");
            });
            GameManager.Procedure.ChangeProcedure<LoginProcedure>().Coroutine();
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
