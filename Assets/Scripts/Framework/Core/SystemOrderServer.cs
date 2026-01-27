using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frame.Core
{
    public interface ISystemServer
    {
        Type SysID { get; set; }
        int SysOrder { get; }
        int SysFixOrder { get; }
        int SysLateOrder { get; }
        int RefCount { get; set; }

        void Update(float delta);
        void FixUpdate(float delta);
        void LateUpdate(float delta);
        void Dispose(bool waitUpdate = false);
    }

    public static class SystemOrderServer
    {
        private static readonly Dictionary<Type, ISystemServer> Systems = new();
        private static readonly SortedDictionary<int, List<ISystemServer>> UpdateOrders = new();
        private static readonly SortedDictionary<int, List<ISystemServer>> FixUpdateOrders = new();
        private static readonly SortedDictionary<int, List<ISystemServer>> LateUpdateOrders = new();

        public static void RegisterSystem<T>(T system) where T : class, ISystemServer
        {
            if (system == null)
            {
                LogError("RegisterSystem failed: system is null.");
                return;
            }

            var key = typeof(T);
            if (Systems.TryGetValue(key, out var existing))
            {
                if (ReferenceEquals(existing, system))
                {
                    LogError($"RegisterSystem failed: duplicate register for {key.Name} instance.");
                }
                else
                {
                    LogError($"RegisterSystem failed: type {key.Name} already registered with another instance.");
                }
                return;
            }

            system.RefCount = 1;
            system.SysID = key;
            Systems[key] = system;

            AddToOrderTree(UpdateOrders, system.SysOrder, system);
            AddToOrderTree(FixUpdateOrders, system.SysFixOrder, system);
            AddToOrderTree(LateUpdateOrders, system.SysLateOrder, system);
        }

        public static void UnRegisterSystem(ISystemServer system)
        {
            if (system == null)
            {
                LogError("UnRegisterSystem failed: system is null.");
                return;
            }
            
            
            var key = FindKey(system);
            if (key == null)
            {
                LogError("UnRegisterSystem failed: system is not registered.");
                return;
            }

            if (system.RefCount != 0)
            {
                LogWarning($"UnRegisterSystem warning: RefCount = {system.RefCount} for {key.Name}.");
            }

            Systems.Remove(key);
            RemoveFromOrderTree(UpdateOrders, system.SysOrder, system);
            RemoveFromOrderTree(FixUpdateOrders, system.SysFixOrder, system);
            RemoveFromOrderTree(LateUpdateOrders, system.SysLateOrder, system);
        }

        public static T GetSystem<T>() where T : class, ISystemServer
        {
            var key = typeof(T);
            if (Systems.TryGetValue(key, out var system))
            {
                system.RefCount += 1;
                return (T)system;
            }

            LogError($"GetSystem failed: {key.Name} is not registered.");
            return null;
        }

        public static void Release(ISystemServer system)
        {
            if (system == null)
            {
                LogError("Release failed: system is null.");
                return;
            }

            if (system.RefCount <= 0)
            {
                LogError("Release failed: RefCount is already 0.");
                return;
            }

            system.RefCount -= 1;
            if (system.RefCount == 0)
            {
                system.Dispose();

                var key = FindKey(system);
                if (key != null)
                {
                    Systems.Remove(key);
                }

                RemoveFromOrderTree(UpdateOrders, system.SysOrder, system);
                RemoveFromOrderTree(FixUpdateOrders, system.SysFixOrder, system);
                RemoveFromOrderTree(LateUpdateOrders, system.SysLateOrder, system);
            }
        }

        public static IEnumerator GetSystemAsync<T>(Action<T> callback) where T : class, ISystemServer
        {
            yield return null;
            var system = GetSystem<T>();
            callback?.Invoke(system);
        }

        public static IEnumerator WaitUtilGetSystem<T>(Action<T> callback) where T : class, ISystemServer
        {
            var key = typeof(T);
            while (true)
            {
                if (Systems.TryGetValue(key, out var system))
                {
                    system.RefCount += 1;
                    callback?.Invoke((T)system);
                    yield break;
                }

                yield return null;
            }
        }

        public static void UpdateSystemServer(float delta)
        {
            InvokeUpdates(UpdateOrders, system => system.Update(delta));
        }

        public static void FixUpdateSystemServer(float delta)
        {
            InvokeUpdates(FixUpdateOrders, system => system.FixUpdate(delta));
        }

        public static void LateUpdateSystemServer(float delta)
        {
            InvokeUpdates(LateUpdateOrders, system => system.LateUpdate(delta));
        }

        private static void InvokeUpdates(SortedDictionary<int, List<ISystemServer>> table, Action<ISystemServer> update)
        {
            foreach (var pair in table)
            {
                var list = pair.Value;
                for (var i = 0; i < list.Count; i += 1)
                {
                    update(list[i]);
                }
            }
        }

        private static void AddToOrderTree(SortedDictionary<int, List<ISystemServer>> table, int order, ISystemServer system)
        {
            if (order == 0)
            {
                return;
            }

            if (!table.TryGetValue(order, out var list))
            {
                list = new List<ISystemServer>();
                table[order] = list;
            }

            list.Add(system);
        }

        private static void RemoveFromOrderTree(SortedDictionary<int, List<ISystemServer>> table, int order, ISystemServer system)
        {
            if (order == 0)
            {
                return;
            }

            if (!table.TryGetValue(order, out var list))
            {
                return;
            }

            list.Remove(system);
            if (list.Count == 0)
            {
                table.Remove(order);
            }
        }

        private static Type FindKey(ISystemServer system)
        {
            if (system == null)
            {
                return null;
            }

            var key = system.SysID;
            if (key == null)
            {
                return null;
            }

            return Systems.ContainsKey(key) ? key : null;
        }

        private static void LogError(string message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#endif
        }

        private static void LogWarning(string message)
        {
#if UNITY_EDITOR
            Debug.LogWarning(message);
#endif
        }
    }
}
