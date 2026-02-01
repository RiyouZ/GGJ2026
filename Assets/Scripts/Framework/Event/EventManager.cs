using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace RuGameFramework.Event
{
	public static class EventManager
	{
		private static int DefaultActionCapacity = 32;
		private static readonly int MaxCapacity;
		private static int _autoId;

		private static Stack<int> _recycleIdStack = null;
		// TODO 换成链表
		private static Dictionary<string, List<Action<IGameEventArgs>>> _actionMap = new Dictionary<string, List<Action<IGameEventArgs>>>(DefaultActionCapacity);
		// 事件ID 和 事件名的双向索引
		private static Dictionary<int, string> _eventIdDic = new Dictionary<int, string>(DefaultActionCapacity);
		private static Dictionary<string, List<int>> _eventNameToEventId = new Dictionary<string, List<int>>(DefaultActionCapacity);
		// 事件的引用计数
		private static Dictionary<string, int> _eventCountDic = new Dictionary<string, int>(DefaultActionCapacity);

		private static string GetEventNameByEventId (int id)
		{
			if (!_eventIdDic.TryGetValue(id, out string evenName))
			{
				return null;
			}


			return evenName;
		}

		private static void TryAddCount (string eventName)
		{
			if (!_eventCountDic.TryGetValue(eventName, out int count))
			{
				_eventCountDic.Add(eventName, 0);
				return;
			}

			_eventCountDic[eventName]++;
		}

		private static void TryRemoveCount (string eventName)
		{
			if (!_eventCountDic.TryGetValue(eventName, out int count))
			{
				return;
			}

			_eventCountDic[eventName]--;

			// 事件监听计数为0时
			if (_eventCountDic[eventName] <= 0)
			{
				// 清除事件缓存
				RemoveEvent(eventName);
				// 清除id映射
				RemoveNameIdMapping(eventName);

				_eventCountDic.Remove(eventName);
			}
		}

		private static void AddNameIdMapping (string eventName, int eventId)
		{
			if (!_eventIdDic.TryGetValue(eventId, out string name))
			{
				_eventIdDic.Add(eventId, eventName);
			}

			if (!_eventNameToEventId.TryGetValue(eventName, out List<int> eventIdList))
			{
				var list = new List<int>();
				list.Add(eventId);
				_eventNameToEventId.Add(eventName, list);
				return;
			}

			eventIdList.Add(eventId);
		}

		private static void RemoveNameIdMapping (string eventName)
		{
			if (!_eventNameToEventId.TryGetValue(eventName, out List<int> eventIdList))
			{
				return;
			}

			foreach (int id in eventIdList)
			{
				_eventIdDic[id] = null;
				RecycleAutoId(id);
			}

			_eventNameToEventId.Remove(eventName);
		}

		private static void RemoveEvent (string eventName)
		{
			if (eventName == null)
			{
				return;
			}

			if (!_actionMap.ContainsKey(eventName))
			{
				return;
			}

			_actionMap[eventName].Clear();
			_actionMap.Remove(eventName);
		}

		public static EventHandle<Action<IGameEventArgs>> AddListener (string eventName, Action<IGameEventArgs> action)
		{
			int eventId = GetAutoId();

			var handle = new EventHandle<Action<IGameEventArgs>>(eventId, action);

			// 添加映射
			AddNameIdMapping(eventName, handle.eventId);

			// 增加引用计数
			TryAddCount(eventName);

			if (!_actionMap.TryGetValue(eventName, out List<Action<IGameEventArgs>> list))
			{
				var actionList = new List<Action<IGameEventArgs>>
				{
					action
				};

				_actionMap.Add(eventName, actionList);

				return handle;
			}

			list.Add(action);

			return handle;
		}

		//
		public static void RemoveListener (string eventName, Action<IGameEventArgs> action)
		{
			if (!_actionMap.ContainsKey(eventName))
			{
				return;
			}

			_actionMap[eventName].Remove(action);

			// 减少引用计数
			TryRemoveCount(eventName);
		}

		public static void RemoveListener (EventHandle<Action<IGameEventArgs>> handle)
		{
			string eventName = GetEventNameByEventId(handle.eventId);

			if (eventName == null)
			{
				return;
			}

			RemoveListener(eventName, handle.action);
		}

		public static void InvokeEvent (string eventName, IGameEventArgs args)
		{
			if (!_actionMap.TryGetValue(eventName, out List<Action<IGameEventArgs>> actionList))
			{
				return;
			}

			for (int i = 0; i < actionList.Count; i++)
			{
				actionList[i]?.Invoke(args);
			}

			args?.Dispose();
		}

		public static int GetAutoId ()
		{
			if (_recycleIdStack == null)
			{
				_recycleIdStack = new Stack<int>(MaxCapacity / 2);
			}

			if (_recycleIdStack.Count <= 0)
			{
				_autoId++;
				return _autoId;
			}
			int id = _recycleIdStack.Pop();
			return id;
		}
		public static void RecycleAutoId (int id)
		{
			if (_recycleIdStack == null)
			{
				return;
			}
			_recycleIdStack.Push(id);
		}
	}
}
