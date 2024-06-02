using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TGame.Asset
{
    public class GameObjectPool<T> where T : GameObjectPoolAsset
    {
        private readonly Dictionary<int, Queue<T>> gameObjectPool = new Dictionary<int, Queue<T>>();
        private readonly List<GameObjectLoadRequest<T>> requests = new List<GameObjectLoadRequest<T>>();
        private readonly Dictionary<int, GameObject> usingObjects = new Dictionary<int, GameObject>();

        public T LoadGameObject(string path, Action<GameObject> createNewCallback = null)
        {
            // 获取路径的哈希值
            int hash = path.GetHashCode();

            // 尝试从 gameObjectPool 中获取哈希值对应的队列
            if (!gameObjectPool.TryGetValue(hash, out Queue<T> q))
            {
                // 如果没有找到对应的队列，则创建一个新的队列并添加到 gameObjectPool 中
                q = new Queue<T>();
                gameObjectPool.Add(hash, q);
            }

            // 如果队列中没有可用的对象
            if (q.Count == 0)
            {
                // 同步加载指定路径的预制体
                GameObject prefab = Addressables.LoadAssetAsync<GameObject>(path).WaitForCompletion();

                // 实例化预制体对象
                GameObject go = UnityEngine.Object.Instantiate(prefab);

                // 给实例化的对象添加组件 T，并获取该组件
                T asset = go.AddComponent<T>();

                // 如果创建新对象时有回调函数，则调用回调函数
                createNewCallback?.Invoke(go);

                // 设置组件的 ID 为路径的哈希值
                asset.ID = hash;

                // 将对象设置为不活跃状态
                go.SetActive(false);

                // 将新创建的对象加入队列中
                q.Enqueue(asset);
            }

            // 从队列中取出一个对象
            {
                T asset = q.Dequeue();

                // 调用 OnGameObjectLoaded 方法，表示对象已加载
                OnGameObjectLoaded(asset);

                // 返回对象
                return asset;
            }
        }

        public void LoadGameObjectAsync(string path, Action<T> callback, Action<GameObject> createNewCallback = null)
        {
            GameObjectLoadRequest<T> request = new GameObjectLoadRequest<T>(path, callback, createNewCallback);
            requests.Add(request);
        }

        public void UnloadAllGameObjects()
        {
            // 先将所有Request加载完毕
            while (requests.Count > 0)
            {
                //GameManager.Asset.UpdateLoader();
                UpdateLoadRequests();
            }

            // 将所有using Objects 卸载
            if (usingObjects.Count > 0)
            {
                List<int> list = new List<int>();
                foreach (var id in usingObjects.Keys)
                {
                    list.Add(id);
                }
                foreach (var id in list)
                {
                    GameObject obj = usingObjects[id];
                    UnloadGameObject(obj);
                }
            }

            // 将所有缓存清掉
            if (gameObjectPool.Count > 0)
            {
                foreach (var q in gameObjectPool.Values)
                {
                    foreach (var asset in q)
                    {
                        UnityEngine.Object.Destroy(asset.gameObject);
                    }
                    q.Clear();
                }
                gameObjectPool.Clear();
            }
        }

        public void UnloadGameObject(GameObject go)
        {
            if (go == null)
                return;

            T asset = go.GetComponent<T>();
            if (asset == null)
            {
                //UnityLog.Warn($"Unload GameObject失败，找不到GameObjectAsset:{go.name}");
                UnityEngine.Object.Destroy(go);
                return;
            }

            if (!gameObjectPool.TryGetValue(asset.ID, out Queue<T> q))
            {
                q = new Queue<T>();
                gameObjectPool.Add(asset.ID, q);
            }
            q.Enqueue(asset);
            usingObjects.Remove(go.GetInstanceID());
            go.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().releaseObjectRoot);
            go.gameObject.SetActive(false);
        }

        public void UpdateLoadRequests()
        {
            if (requests.Count > 0)
            {
                foreach (var request in requests)
                {
                    int hash = request.Path.GetHashCode();
                    if (!gameObjectPool.TryGetValue(hash, out Queue<T> q))
                    {
                        q = new Queue<T>();
                        gameObjectPool.Add(hash, q);
                    }

                    if (q.Count == 0)
                    {
                        Addressables.LoadAssetAsync<GameObject>(request.Path).Completed += (obj) =>
                        {
                            GameObject go = UnityEngine.Object.Instantiate(obj.Result);
                            T asset = go.AddComponent<T>();
                            request.CreateNewCallback?.Invoke(go);
                            asset.ID = hash;
                            go.SetActive(false);

                            OnGameObjectLoaded(asset);
                            request.LoadFinish(asset);
                        };
                    }
                    else
                    {
                        T asset = q.Dequeue();
                        OnGameObjectLoaded(asset);
                        request.LoadFinish(asset);
                    }
                }

                requests.Clear();
            }
        }

        private void OnGameObjectLoaded(T asset)
        {
            asset.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().usingObjectRoot);
            int id = asset.gameObject.GetInstanceID();
            usingObjects.Add(id, asset.gameObject);
        }
    }
}
