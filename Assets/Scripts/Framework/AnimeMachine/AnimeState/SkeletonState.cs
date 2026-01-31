using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	public abstract class SkeletonState : ISkeletonState
	{
		protected string _animeName;

		protected SkeletonStateMachine _machine;

		public virtual string AnimeName
		{
			get => _animeName;
			set => _animeName = value;
		}

		protected bool _isLoop;
		public bool IsLoop
		{
			get => _isLoop;
			set => _isLoop = value;
		}

		protected SkeletonLayer _layer;
		public SkeletonLayer Layer
		{
			set => _layer = value;
			get => _layer;
		}

		private List<IAnimeStateTransition> _stateTransitions = new List<IAnimeStateTransition>();
		public List<IAnimeStateTransition> StateTransitions
		{
			get => _stateTransitions;
			set => _stateTransitions = value;
		}

		protected Dictionary<string, Action<TrackEntry, Spine.Event>> _eventDic = new Dictionary<string, Action<TrackEntry, Spine.Event>>();

		protected IAnimeStateTransition _nextStateTransition;

		public virtual void Initialize(SkeletonStateMachine machine)
		{
			this._machine = machine;
		}

		public virtual bool AllowInterrupt() => true;

		public virtual void OnComplete (TrackEntry track) {}

		public virtual void OnEnd (TrackEntry track) {}

		public virtual void OnEvent (TrackEntry track, Spine.Event e)
		{
			if (_eventDic.TryGetValue(e.Data.Name, out Action<TrackEntry, Spine.Event> action))
			{
				action?.Invoke(track, e);
			}
		}

		public virtual void OnStart (TrackEntry track) {}

		public virtual void Dispose ()
		{
			_eventDic.Clear ();
			_eventDic = null;

			_stateTransitions.Clear();
			_stateTransitions = null;
			
			_machine = null;
		}

		public virtual string NextStateOnUpdate()
		{
			// 已经存在转换条件
			if(_nextStateTransition != null)
			{
				return null;
			}
			_nextStateTransition = null;
			// 检测条件并转换
			foreach (var cond in StateTransitions)
			{
				var canTrans = cond.TransitionCondition?.Invoke();
				// 需播放完动画先记录下一个状态
				if (cond.CanComplateExit && canTrans == true)
				{
					_nextStateTransition = cond;
					return null;
				}

				// 直接转换
				if (!cond.CanComplateExit && canTrans == true)
				{
					return cond.ToState;
				}
			}

			return null;
		}

		public virtual string NextStateOnComplete()
		{
			if(_nextStateTransition == null)
			{
				return null;
			}
			var toState = _nextStateTransition.ToState;
			_nextStateTransition = null;
			return toState;
		}

		public virtual ISkeletonState AddTransition (string toStateIndex, Func<bool> cond, bool canExit = false, float mix = 0.2f)
		{
			_machine.AddTransition(this.AnimeName, toStateIndex, cond, canExit, mix);
			return this;
		}

		public virtual ISkeletonState AddAnimationEvent (string eventName, Action<TrackEntry, Spine.Event> action)
		{
			if (_eventDic.TryGetValue(eventName, out Action<TrackEntry, Spine.Event> e))
			{
				return this;
			}

			_eventDic.Add(eventName, action);
			return this;
		}

        public abstract TrackEntry PlayAnime();
    }

}

