using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class ProcedureModule : BaseGameModule
{
    [SerializeField]
    private string[] proceduresNames = null;
    [SerializeField]
    private string defaultProcedureName = null;
    
    public BaseProcedure CurrentProcedure { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsChangingProcedure { get; private set; }

    private Dictionary<Type, BaseProcedure> procedures;
    private BaseProcedure defaultProcedure;
    private ObjectPool<ChangeProcedureRequest> changeProcedureRequestPool = new ObjectPool<ChangeProcedureRequest>(null);
    private Queue<ChangeProcedureRequest> changeProcedureQ = new Queue<ChangeProcedureRequest>();

    protected internal override void OnModuleInit()
    {
        base.OnModuleInit();
        procedures = new Dictionary<Type, BaseProcedure>();
        bool findDefaultState = false;
        for (int i = 0; i < proceduresNames.Length; i++)
        {
            string procedureTypeName = proceduresNames[i];
            if (string.IsNullOrEmpty(procedureTypeName))
                continue;
            //获取具有指定名称的 Type，指定是否执行区分大小写的搜索，以及在找不到类型时是否引发异常。
            Type procedureType = Type.GetType(procedureTypeName, true);
            if (procedureType == null)
            {
                Debug.LogError($"Can't find procedure:`{procedureTypeName}`");
                continue;
            }
            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure;
            bool isDefaultState = procedureTypeName == defaultProcedureName;
            procedures.Add(procedureType, procedure);

            if (isDefaultState)
            {
                defaultProcedure = procedure;
                findDefaultState = true;
            }
        }
        if (!findDefaultState)
        {
            Debug.LogError($"You have to set a correct default procedure to start game");
        }
    }

    protected internal override void OnModuleStart()
    {
        base.OnModuleStart();
    }

    protected internal override void OnModuleStop()
    {
        base.OnModuleStop();
        changeProcedureRequestPool.Clear();
        changeProcedureQ.Clear();
        IsRunning = false;
    }

    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime);
    }


    public async Task StartProcedure()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();
        changeProcedureRequest.TargetProcedure = defaultProcedure;
        changeProcedureQ.Enqueue(changeProcedureRequest);
        await ChangeProcedureInternal();
    }

    public async Task ChangeProcedure<T>() where T : BaseProcedure
    {
        await ChangeProcedure<T>(null);
    }

    public async Task ChangeProcedure<T>(object value) where T : BaseProcedure
    {
        if (!IsRunning)
            return;

        if (!procedures.TryGetValue(typeof(T), out BaseProcedure procedure))
        {
            //UnityLog.Error($"Change Procedure Failed, Can't find Proecedure:${typeof(T).FullName}");
            return;
        }

        ChangeProcedureRequest changeProcedureRequest = changeProcedureRequestPool.Obtain();
        changeProcedureRequest.TargetProcedure = procedure;
        changeProcedureRequest.Value = value;
        changeProcedureQ.Enqueue(changeProcedureRequest);

        if (!IsChangingProcedure)
        {
            await ChangeProcedureInternal();
        }
    }
    /// <summary>
    /// 改变模块
    /// </summary>
    /// <returns></returns>
    private async Task ChangeProcedureInternal()
    {
        if (IsChangingProcedure)
            return;

        IsChangingProcedure = true;
        while (changeProcedureQ.Count > 0)
        {
            ChangeProcedureRequest request = changeProcedureQ.Dequeue();
            if (request == null || request.TargetProcedure == null)
                continue;
            //异步切换
            if (CurrentProcedure != null)
            {
                await CurrentProcedure.OnLeaveProcedure();
            }
            CurrentProcedure = request.TargetProcedure;
            await CurrentProcedure.OnEnterProcedure(request.Value);
        }
        IsChangingProcedure = false;
    }
}

/// <summary>
/// 切换模块请求
/// </summary>
public class ChangeProcedureRequest
{
    public BaseProcedure TargetProcedure { get; set; }
    public object Value { get; set; }
}
