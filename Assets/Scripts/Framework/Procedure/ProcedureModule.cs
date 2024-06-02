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
        // ��ʼ�� procedures �ֵ䣬���ڴ洢���е��������ͼ���ʵ��
        procedures = new Dictionary<Type, BaseProcedure>();
        // ����Ƿ��ҵ�Ĭ��״̬������
        bool findDefaultState = false;

        // ����������������
        for (int i = 0; i < proceduresNames.Length; i++)
        {
            // ��ȡ��ǰ��������
            string procedureTypeName = proceduresNames[i];

            // �����������Ϊ�գ�������ǰѭ��
            if (string.IsNullOrEmpty(procedureTypeName))
                continue;

            // ��ȡ����ָ�����Ƶ� Type��ָ���Ƿ�ִ�����ִ�Сд���������Լ����Ҳ�������ʱ�Ƿ������쳣
            Type procedureType = Type.GetType(procedureTypeName, true);

            // ���û���ҵ���Ӧ�� Type�����������־��������ǰѭ��
            if (procedureType == null)
            {
                Debug.LogError($"Can't find procedure:`{procedureTypeName}`");
                continue;
            }

            // ʹ�÷��䴴������ʵ��
            BaseProcedure procedure = Activator.CreateInstance(procedureType) as BaseProcedure;

            // �жϵ�ǰ�����Ƿ�ΪĬ��״̬
            bool isDefaultState = procedureTypeName == defaultProcedureName;

            // ���������ͺ�ʵ����ӵ� procedures �ֵ���
            procedures.Add(procedureType, procedure);

            // �����ǰ������Ĭ��״̬������ defaultProcedure ��������ҵ�Ĭ��״̬
            if (isDefaultState)
            {
                defaultProcedure = procedure;
                findDefaultState = true;
            }
        }

        // ���û���ҵ�Ĭ��״̬�����̣����������־
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
    /// �ı�ģ��
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
            //�첽�л�
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
/// �л�ģ������
/// </summary>
public class ChangeProcedureRequest
{
    public BaseProcedure TargetProcedure { get; set; }
    public object Value { get; set; }
}
