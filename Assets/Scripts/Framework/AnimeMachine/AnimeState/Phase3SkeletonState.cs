using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	// TODO 动画优先级
	public class Phase3SkeletonState : SkeletonState
	{
		public enum Phase
		{
			Any = 0,
			Begin = 1,
			Loop = 2,
			End = 3
		}

		protected class InternalPhaseState : SkeletonState
		{
			public Phase phase;
			public Phase3SkeletonState parent;

			public override void OnStart(TrackEntry trackEntry)
			{
				if(phase != Phase.Begin)
				{
					return;
				}
				parent._onStart?.Invoke(this, trackEntry);
			}

			public override void OnEnd(TrackEntry trackEntry)
			{
				if(phase != Phase.End)
				{
					return;
				}
				parent._onEnd?.Invoke(this, trackEntry);
			}

			public override void OnComplete(TrackEntry trackEntry)
			{
				// 存在转换条件并到达当前的打断阶段
				if(parent._nextStateTransition != null && parent._interrupt <= this.phase)
				{
					return;
				}

				switch (phase)
				{
					case Phase.Begin:
						parent.PlayPhase(Phase.Loop);
						return;
					case Phase.Loop:
						// 循环时满足条件退出
						if(parent._nextStateTransition != null)
						{
							parent.PlayPhase(Phase.End);
						}
						return;
				}
			}

			public override string NextStateOnUpdate()
			{
				return parent.NextStateOnUpdate();
			}

			// 延迟状态出口
			public override string NextStateOnComplete()
			{
				if(parent._nextStateTransition == null || parent._interrupt > this.phase)
				{
					return null;
				}
				// 存在转换条件并到达当前的打断阶段
				var toState = parent._nextStateTransition.ToState;
				parent._nextStateTransition = null;
				return toState;
			}

			// 外部状态进入时播放
			public override TrackEntry PlayAnime()
			{
				return _machine.PlayTrackAnimation(this.Layer, this.AnimeName, parent._nextStateTransition == null ? 0 : parent._nextStateTransition.MixDuration, this._isLoop);
			}
		}

		public override string AnimeName
		{
			set => _animeName = value;
			get => _phaseMap[_current].AnimeName;
		}

		private Action<ISkeletonState, TrackEntry> _onComplate;
		private Action<ISkeletonState, TrackEntry> _onStart;
		private Action<ISkeletonState, TrackEntry> _onEnd;
		private Action<ISkeletonState, TrackEntry> _onInterrupt;


		private Dictionary<Phase, InternalPhaseState> _phaseMap = new Dictionary<Phase, InternalPhaseState>(3);

		private Phase _interrupt;
		private Phase _current;
		public Phase3SkeletonState() {}

		public Phase3SkeletonState(SkeletonStateMachine machine, SkeletonLayer layer, string stateIndex, string loop, string begin, string end)
		{
			this.AnimeName = $"Phase{stateIndex}";
			this.Layer = layer;
			Initialize(machine, loop, begin, end);
		}

		private void Initialize(SkeletonStateMachine machine, string loop, string begin, string end)
		{	
			if(machine == null)
			{
				return;
			}

			_machine = machine;
			var loopState = machine.RegisterState<InternalPhaseState>(this.Layer, loop, true);
			loopState.parent = this;
			loopState.phase = Phase.Loop;
			_phaseMap.Add(Phase.Loop, loopState);

			if(begin != null)
			{
				var beginState = _machine.RegisterState<InternalPhaseState>(this.Layer, begin, false);
				beginState.parent = this;
				beginState.phase = Phase.Begin;
				_phaseMap.Add(Phase.Begin, beginState);
			}

			if(end != null)
			{
				var endState = _machine.RegisterState<InternalPhaseState>(this.Layer, end, false);
				endState.parent = this;
				endState.phase = Phase.End;
				_phaseMap.Add(Phase.End, endState);
			}

			if(begin != null)
			{
				_current = Phase.Begin;
			}
			else
			{
				_current = Phase.Loop;
			}
		}
		
		public Phase3SkeletonState OnAnimationStart (Action<ISkeletonState, TrackEntry> action)
		{
			_onStart = action;
			return this;
		}

		public Phase3SkeletonState OnAnimationEnd (Action<ISkeletonState, TrackEntry> action)
		{

			_onEnd = action;
			return this;
		}

		protected void PlayPhase(Phase phase)
		{
			_current = phase;
			if(!_phaseMap.TryGetValue(phase, out var state))
			{
				return;
			}

			_machine.ChangeState(state);
		}

		public Phase3SkeletonState Interrupt(Phase phase = Phase.Any)
		{
			_interrupt = phase;
			return this;
		}

		public override bool AllowInterrupt()
		{
			return _interrupt == Phase.Any;
		}

		public override string NextStateOnUpdate()
		{
			// 已经存在转换条件
			if(_nextStateTransition != null)
			{
				return null;
			}

			foreach(var cond in StateTransitions)
			{
				var canTrans = cond.TransitionCondition?.Invoke();
				if(canTrans == true)
				{
					// 直接打断动画并转换
					if(_interrupt == Phase.Any)
					{
						_machine.ClearTrackForLayer(this.Layer);
						return cond.ToState;
					}

					// 缓存转换条件
					_nextStateTransition = cond;
					return null;
				}
			}
			return null;
		}

		public override void Dispose()
		{	
			if(_phaseMap.TryGetValue(Phase.Begin, out var beginState))
			{
				_machine.UnRegisterState(beginState.AnimeName);
			}

			if(_phaseMap.TryGetValue(Phase.Loop, out var loopState))
			{
				_machine.UnRegisterState(loopState.AnimeName);
			}

			if(_phaseMap.TryGetValue(Phase.End, out var endState))
			{
				_machine.UnRegisterState(endState.AnimeName);
			}

			_phaseMap.Clear();
			_phaseMap = null;

			base.Dispose();
		}

		// 由子节点播放
		public override TrackEntry PlayAnime()
		{
			return null;
		}
	}

}
