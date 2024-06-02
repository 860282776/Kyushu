using Config;
using Koakuma.Game.UI;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.UI;

namespace TGame.UI
{
    public partial class UIModule : BaseGameModule
    {
        public Transform normalUIRoot;
        public Transform modalUIRoot;
        public Transform closeUIRoot;
        public Image imgMask;
        public QuantumConsole prefabQuantumConsole;

        private static Dictionary<UIViewID, Type> MEDIATOR_MAPPING;
        private static Dictionary<UIViewID, Type> ASSET_MAPPING;

        private readonly List<UIMediator> usingMediators = new List<UIMediator>();
        private readonly Dictionary<Type, Queue<UIMediator>> freeMediators = new Dictionary<Type, Queue<UIMediator>>();
        private readonly GameObjectPool<GameObjectAsset> uiObjectPool = new GameObjectPool<GameObjectAsset>();
        private QuantumConsole quantumConsole;

        protected internal override void OnModuleInit()
        {
            base.OnModuleInit();
            //quantumConsole = Instantiate(prefabQuantumConsole);
            //quantumConsole.transform.SetParentAndResetAll(transform);
            //quantumConsole.OnActivate += OnConsoleActive;
            //quantumConsole.OnDeactivate += OnConsoleDeactive;
        }

        protected internal override void OnModuleStop()
        {
            base.OnModuleStop();
            //quantumConsole.OnActivate -= OnConsoleActive;
            //quantumConsole.OnDeactivate -= OnConsoleDeactive;
        }
        /// <summary>
        /// 缓存ui视图和mediator及资源的映射关系
        /// </summary>
        private static void CacheUIMapping()
        {

            // 如果 MEDIATOR_MAPPING 已经初始化，不再继续执行
            if (MEDIATOR_MAPPING != null) return;

            // 初始化 MEDIATOR_MAPPING 和 ASSET_MAPPING 字典
            MEDIATOR_MAPPING = new Dictionary<UIViewID, Type>();
            ASSET_MAPPING = new Dictionary<UIViewID, Type>();

            // 获取 UIView 类型
            Type baseViewType = typeof(UIView);

            // 遍历 UIView 所在程序集中的所有类型
            foreach (var type in baseViewType.Assembly.GetTypes())
            {
                // 如果类型是抽象的，跳过
                if (type.IsAbstract)
                    continue;

                // 判断当前类型是否是 UIView 的子类或本身
                if (baseViewType.IsAssignableFrom(type))
                {
                    // 获取当前类型的所有 UIViewAttribute 特性
                    object[] attrs = type.GetCustomAttributes(typeof(UIViewAttribute), false);

                    // 如果没有 UIViewAttribute 特性，输出错误日志并跳过
                    if (attrs.Length == 0)
                    {
                        UnityLog.Error($"{type.FullName} 没有绑定 Mediator，请使用 UIMediatorAttribute 绑定一个 Mediator 以正确使用");
                        // 输出错误信息，提示用户绑定 Mediator
                        continue;
                    }

                    // 遍历所有 UIViewAttribute 特性
                    foreach (UIViewAttribute attr in attrs)
                    {
                        // 将视图 ID 与对应的 Mediator 类型进行映射
                        MEDIATOR_MAPPING.Add(attr.ID, attr.MediatorType);
                        // 将视图 ID 与对应的视图类型进行映射
                        ASSET_MAPPING.Add(attr.ID, type);
                        // 由于一个类型可能有多个特性，但我们只需要一个，跳出循环
                        break;
                    }
                }
            }

        }

        protected internal override void OnModuleUpdate(float deltaTime)
        {
            base.OnModuleUpdate(deltaTime);
            uiObjectPool.UpdateLoadRequests();
            foreach (var mediator in usingMediators)
            {
                mediator.Update(deltaTime);
            }
            UpdateMask(deltaTime);
        }

        private void OnConsoleActive()
        {
            //GameManager.Input.SetEnable(false);
        }

        private void OnConsoleDeactive()
        {
            //GameManager.Input.SetEnable(true);
        }
        /// <summary>
        /// 得到此模式最顶层的ui
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private int GetTopMediatorSortingOrder(UIMode mode)
        {
            // 初始化 lastIndexMediatorOfMode 变量为 -1，用于记录最后一个符合条件的 Mediator 的索引
            int lastIndexMediatorOfMode = -1;

            // 从 usingMediators 列表的最后一个元素开始，向前遍历
            for (int i = usingMediators.Count - 1; i >= 0; i--)
            {
                // 获取当前遍历到的 Mediator
                UIMediator mediator = usingMediators[i];

                // 如果当前 Mediator 的 UIMode 不等于指定的 mode，继续下一个循环
                if (mediator.UIMode != mode)
                    continue;

                // 如果找到符合条件的 Mediator，记录其索引
                lastIndexMediatorOfMode = i;
                // 由于是从后向前遍历，第一个找到的即为最后一个符合条件的 Mediator，跳出循环
                break;
            }

            // 如果没有找到符合条件的 Mediator
            if (lastIndexMediatorOfMode == -1)
            {
                // 如果 mode 是 UIMode.Normal，返回 0
                // 否则返回 1000
                return mode == UIMode.Normal ? 0 : 1000;
            }

            // 返回最后一个符合条件的 Mediator 的 SortingOrder
            return usingMediators[lastIndexMediatorOfMode].SortingOrder;
        }

        private UIMediator GetMediator(UIViewID id)
        {
            CacheUIMapping();

            if (!MEDIATOR_MAPPING.TryGetValue(id, out Type mediatorType))
            {
                //UnityLog.Error($"找不到 {id} 对应的Mediator");
                return null;
            }

            if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))
            {
                mediatorQ = new Queue<UIMediator>();
                freeMediators.Add(mediatorType, mediatorQ);
            }

            UIMediator mediator;
            if (mediatorQ.Count == 0)
            {
                mediator = Activator.CreateInstance(mediatorType) as UIMediator;
            }
            else
            {
                mediator = mediatorQ.Dequeue();
            }

            return mediator;
        }

        private void RecycleMediator(UIMediator mediator)
        {
            if (mediator == null)
                return;

            Type mediatorType = mediator.GetType();
            if (!freeMediators.TryGetValue(mediatorType, out Queue<UIMediator> mediatorQ))
            {
                mediatorQ = new Queue<UIMediator>();
                freeMediators.Add(mediatorType, mediatorQ);
            }
            mediatorQ.Enqueue(mediator);
        }

        public UIMediator GetOpeningUIMediator(UIViewID id)
        {
            UIConfig uiConfig = UIConfig.ByID((int)id);
            if (uiConfig.IsNull)
                return null;

            UIMediator mediator = GetMediator(id);
            if (mediator == null)
                return null;

            Type requiredMediatorType = mediator.GetType();
            foreach (var item in usingMediators)
            {
                if (item.GetType() == requiredMediatorType)
                    return item;
            }
            return null;
        }

        public void BringToTop(UIViewID id)
        {
            // 获取指定 ID 的正在打开的 UIMediator
            UIMediator mediator = GetOpeningUIMediator(id);

            // 如果没有找到对应的 UIMediator，直接返回
            if (mediator == null)
                return;

            // 获取当前 UIMode 下的最高排序顺序
            int topSortingOrder = GetTopMediatorSortingOrder(mediator.UIMode);

            // 如果当前 Mediator 的排序顺序已经是最高的，直接返回
            if (mediator.SortingOrder == topSortingOrder)
                return;

            // 将排序顺序设置为当前最高顺序加 10
            int sortingOrder = topSortingOrder + 10;
            mediator.SortingOrder = sortingOrder;

            // 从 usingMediators 列表中移除当前 Mediator，并重新添加到列表末尾
            usingMediators.Remove(mediator);
            usingMediators.Add(mediator);

            // 获取当前 Mediator 视图对象的 Canvas 组件
            Canvas canvas = mediator.ViewObject.GetComponent<Canvas>();

            // 如果视图对象有 Canvas 组件，更新其 sortingOrder 属性
            if (canvas != null)
            {
                canvas.sortingOrder = sortingOrder;
            }
        }

        public bool IsUIOpened(UIViewID id)
        {
            return GetOpeningUIMediator(id) != null;
        }

        public UIMediator OpenUISingle(UIViewID id, object arg = null)
        {
            UIMediator mediator = GetOpeningUIMediator(id);
            if (mediator != null)
                return mediator;

            return OpenUI(id, arg);
        }

        public UIMediator OpenUI(UIViewID id, object arg = null)
        {
            UIConfig uiConfig = UIConfig.ByID((int)id);
            if (uiConfig.IsNull)
                return null;

            UIMediator mediator = GetMediator(id);
            if (mediator == null)
                return null;

            GameObject uiObject = (uiObjectPool.LoadGameObject(uiConfig.Asset, (obj) =>
            {
                UIView newView = obj.GetComponent<UIView>();
                mediator.InitMediator(newView);
            })).gameObject;
            return OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
        }

        public IEnumerator OpenUISingleAsync(UIViewID id, object arg = null)
        {
            if (!IsUIOpened(id))
            {
                yield return OpenUIAsync(id, arg);
            }
        }

        public IEnumerator OpenUIAsync(UIViewID id, object arg = null)
        {
            UIConfig uiConfig = UIConfig.ByID((int)id);
            if (uiConfig.IsNull)
                yield break;

            UIMediator mediator = GetMediator(id);
            if (mediator == null)
                yield break;

            bool loadFinish = false;
            uiObjectPool.LoadGameObjectAsync(uiConfig.Asset, (asset) =>
            {
                GameObject uiObject = asset.gameObject;
                OnUIObjectLoaded(mediator, uiConfig, uiObject, arg);
                loadFinish = true;
            }, (obj) =>
            {
                UIView newView = obj.GetComponent<UIView>();
                mediator.InitMediator(newView);
            });
            while (!loadFinish)
            {
                yield return null;
            }
            yield return null;
            yield return null;
        }

        private UIMediator OnUIObjectLoaded(UIMediator mediator, UIConfig uiConfig, GameObject uiObject, object obj)
        {
            // 如果 uiObject 为空，表示加载 UI 失败
            if (uiObject == null)
            {
                // 输出错误日志，记录失败的 UI 资源
                UnityLog.Error($"加载UI失败: {uiConfig.Asset}");
                // 回收 mediator 实例
                RecycleMediator(mediator);
                // 返回 null，表示加载失败
                return null;
            }

            // 获取 uiObject 上的 UIView 组件
            UIView view = uiObject.GetComponent<UIView>();
            // 如果没有找到 UIView 组件
            if (view == null)
            {
                // 输出错误日志，记录缺少 UIView 组件的 UI 资源
                UnityLog.Error($"UI Prefab不包含UIView脚本: {uiConfig.Asset}");
                // 回收 mediator 实例
                RecycleMediator(mediator);
                // 从对象池中卸载该游戏对象
                uiObjectPool.UnloadGameObject(view.gameObject);
                // 返回 null，表示加载失败
                return null;
            }

            // 设置 mediator 的 UIMode 为配置中的 Mode
            mediator.UIMode = uiConfig.Mode;
            // 获取当前模式下的最高排序顺序，并加 10 作为新的排序顺序
            int sortingOrder = GetTopMediatorSortingOrder(uiConfig.Mode) + 10;

            // 将 mediator 添加到正在使用的 mediator 列表中
            usingMediators.Add(mediator);

            // 获取 uiObject 上的 Canvas 组件
            Canvas canvas = uiObject.GetComponent<Canvas>();
            // 设置 Canvas 的渲染模式为 ScreenSpaceCamera
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            // 设置 Canvas 的摄像机（暂时注释掉具体设置）
            // canvas.worldCamera = GameManager.Camera.uiCamera;

            // 根据 UI 模式设置 UI 的父节点和排序层级
            if (uiConfig.Mode == UIMode.Normal)
            {
                // 设置 UI 对象的父节点为 normalUIRoot，并重置其所有变换属性
                uiObject.transform.SetParentAndResetAll(normalUIRoot);
                // 设置 Canvas 的排序层名称为 "NormalUI"
                canvas.sortingLayerName = "NormalUI";
            }
            else
            {
                // 设置 UI 对象的父节点为 modalUIRoot，并重置其所有变换属性
                uiObject.transform.SetParentAndResetAll(modalUIRoot);
                // 设置 Canvas 的排序层名称为 "ModalUI"
                canvas.sortingLayerName = "ModalUI";
            }

            // 设置 mediator 的 SortingOrder 属性
            mediator.SortingOrder = sortingOrder;
            // 设置 Canvas 的排序顺序
            canvas.sortingOrder = sortingOrder;

            // 激活 UI 对象
            uiObject.SetActive(true);
            // 调用 mediator 的 Show 方法，显示 UI 并传递参数对象
            mediator.Show(uiObject, obj);
            // 返回 mediator 实例
            return mediator;
        }

        public void CloseUI(UIMediator mediator)
        {
            if (mediator != null)
            {
                // 回收View
                uiObjectPool.UnloadGameObject(mediator.ViewObject);
                mediator.ViewObject.transform.SetParentAndResetAll(closeUIRoot);

                // 回收Mediator
                mediator.Hide();
                RecycleMediator(mediator);

                usingMediators.Remove(mediator);
            }
        }

        public void CloseAllUI()
        {
            for (int i = usingMediators.Count - 1; i >= 0; i--)
            {
                CloseUI(usingMediators[i]);
            }
        }

        public void CloseUI(UIViewID id)
        {
            UIMediator mediator = GetOpeningUIMediator(id);
            if (mediator == null)
                return;

            CloseUI(mediator);
        }

        public void SetAllNormalUIVisibility(bool visible)
        {
            normalUIRoot.gameObject.SetActive(visible);
        }

        public void SetAllModalUIVisibility(bool visible)
        {
            modalUIRoot.gameObject.SetActive(visible);
        }

        public void ShowMask(float duration = 0.5f)
        {
            destMaskAlpha = 1;
            maskDuration = duration;
        }

        public void HideMask(float? duration = null)
        {
            destMaskAlpha = 0;
            if (duration.HasValue)
            {
                maskDuration = duration.Value;
            }
        }

        private float destMaskAlpha = 0;
        private float maskDuration = 0;
        private void UpdateMask(float deltaTime)
        {
            Color c = imgMask.color;
            c.a = maskDuration > 0 ? Mathf.MoveTowards(c.a, destMaskAlpha, 1f / maskDuration * deltaTime) : destMaskAlpha;
            c.a = Mathf.Clamp01(c.a);
            imgMask.color = c;
            imgMask.enabled = imgMask.color.a > 0;
        }

        public void ShowConsole()
        {
            quantumConsole.Activate();
        }
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
sealed class UIViewAttribute : Attribute
{
    public UIViewID ID { get; }
    public Type MediatorType { get; }

    public UIViewAttribute(Type mediatorType, UIViewID id)
    {
        ID = id;
        MediatorType = mediatorType;
    }
}
