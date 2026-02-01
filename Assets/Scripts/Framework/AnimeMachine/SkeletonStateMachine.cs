using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	public enum SkeletonLayer
	{
		Base = 0,
		Upper = 1,
		Lower = 2,
	}

	public class SkeletonStateMachine
	{
		private SkeletonAnimation _skeleton;

		private TrackEntry _track;
		public TrackEntry Track => _track;

		private Dictionary<SkeletonLayer, TrackEntry> _layerToTrack;

		private ISkeletonState _currentState;
		private ISkeletonState _defaultState;

		public string CurrentState => _currentState.AnimeName;

		private Dictionary<string, ISkeletonState> _stateDic;

		private bool _isRunning;

		// 动画触发器 用于逻辑驱动动画
		private Dictionary<string, bool> _trigger;

		public SkeletonStateMachine (SkeletonAnimation skeleton, int capacity = 2)
		{
			_isRunning = false;
			_skeleton = skeleton;
			_stateDic = new Dictionary<string, ISkeletonState>(capacity);
			_trigger = new Dictionary<string, bool>(capacity);

			_layerToTrack = new Dictionary<SkeletonLayer, TrackEntry>(3);
			_layerToTrack.Add(SkeletonLayer.Base, null);
			_layerToTrack.Add(SkeletonLayer.Upper, null);
			_layerToTrack.Add(SkeletonLayer.Lower, null);
		}

		private ISkeletonState GetState (string stateIndex)
		{
			if (!_stateDic.TryGetValue(stateIndex, out ISkeletonState state))
			{
				return null;
			}
			return state;
		}

		// 全部预注册过
		public IAnimeStateTransition AddTransition(ISkeletonState from, ISkeletonState to, Func<bool> cond = null, bool canExit = false, float mix = 0.2f)
		{
			if(!_stateDic.ContainsKey(from.AnimeName) || !_stateDic.ContainsKey(to.AnimeName))
			{
				# if UNITY_EDITOR
					Debug.LogError($"[SkeletonStateMachine.{_skeleton.gameObject.name}] 状态未注册");
				#endif
				return null;
			}

			var trans = new AnimeStateTransition(from.AnimeName, to.AnimeName, cond, canExit, mix);
			from.StateTransitions.Add(trans);
			_skeleton.AnimationState.Data.SetMix(from.AnimeName, to.AnimeName, mix);
			return trans;
		}

		public IAnimeStateTransition AddTransition (string from, string to, Func<bool> cond = null, bool canExit = false, float mix = 0.2f)
		{
			// From必须注册
			if (!_stateDic.TryGetValue(from, out ISkeletonState fromState))
			{
				return null;
			}

			var trans = new AnimeStateTransition(from, to, cond, canExit, mix);
			fromState.StateTransitions.Add(trans);
			_skeleton.AnimationState.Data.SetMix(from, to, mix);

			return trans;
		}
		
		// 含有过渡动画
		public IAnimeStateTransition AddTransition (string from, string to, string bridge, Func<bool> cond = null, bool canExit = false, float mix = 0)
		{
			// From必须注册
			if (!_stateDic.TryGetValue(from, out ISkeletonState fromState))
			{
				return null;
			}

			// 先构造Bridge状态
			if(!_stateDic.TryGetValue(bridge, out ISkeletonState bridgeState))
			{
				return null;
			}

			(bridgeState as TransitionSkeletonState).ToState = to;

			var trans = new AnimeStateTransition(from, bridge, cond, canExit, mix);
			fromState.StateTransitions.Add(trans);
			_skeleton.AnimationState.Data.SetMix(from, bridge, mix);

			return trans;
		}

		public IAnimeStateTransition AddTransition (ISkeletonState from, ISkeletonState to, TransitionSkeletonState bridge, Func<bool> cond = null, bool canExit = false, float mix = 0)
		{
			if(!_stateDic.ContainsKey(from.AnimeName) || !_stateDic.ContainsKey(to.AnimeName) || ! _stateDic.ContainsKey(bridge.AnimeName))
			{
				# if UNITY_EDITOR
					Debug.LogError($"[SkeletonStateMachine.{_skeleton.gameObject.name}] 状态未注册");
				#endif
				return null;
			}

			bridge.ToState = to.AnimeName;
			var trans = new AnimeStateTransition(from.AnimeName, bridge.AnimeName, cond, canExit, mix);
			from.StateTransitions.Add(trans);
			_skeleton.AnimationState.Data.SetMix(from.AnimeName, bridge.AnimeName, mix);

			return trans;
		}

		public T RegisterState<T> (SkeletonLayer layer, string stateIndex, bool isLoop = false) where T : ISkeletonState, new()
		{
			T state = new T();
			state.AnimeName = stateIndex;
			state.IsLoop = isLoop;
			state.Layer = layer;
			state.Initialize(this);

			if (_stateDic.TryGetValue(stateIndex, out ISkeletonState iState))
			{
				if (iState == null)
				{
					_stateDic[stateIndex] = state;
				}

				return state;
			}

			_stateDic.Add(stateIndex, state);

			return state;
		}

		// 注册匿名状态
		public AnoSkeletonState RegisterState (SkeletonLayer layer, string stateIndex, bool isLoop = false)
		{
			return RegisterState<AnoSkeletonState>(layer, stateIndex, isLoop);
		}

		public Phase3SkeletonState RegisterState(SkeletonLayer layer, string stateIndex, string loopState, string beginState, string endState)
		{
			Phase3SkeletonState state = new Phase3SkeletonState(this, layer, stateIndex, loopState, beginState, endState);

			if (_stateDic.TryGetValue(stateIndex, out ISkeletonState iState))
			{
				if (iState == null)
				{
					_stateDic[stateIndex] = state;
				}

				return state;
			}

			_stateDic.Add(stateIndex, state);
			return state;
		}

		public TransitionSkeletonState RegisterTransitionState(SkeletonLayer layer, string stateIndex)
		{
			TransitionSkeletonState state = new TransitionSkeletonState(this, layer, stateIndex);
			if (_stateDic.TryGetValue(stateIndex, out ISkeletonState iState))
			{
				if (iState == null)
				{
					_stateDic[stateIndex] = state;
				}

				return state;
			}

			_stateDic.Add(stateIndex, state);
			return state;
		}

		public TransitionSkeletonState RegisterTransitionState(SkeletonLayer layer, string stateIndex, string animeName)
		{
			TransitionSkeletonState state = new TransitionSkeletonState(this, layer, animeName);
			if (_stateDic.TryGetValue(stateIndex, out ISkeletonState iState))
			{
				if (iState == null)
				{
					_stateDic[stateIndex] = state;
				}

				return state;
			}

			_stateDic.Add(stateIndex, state);
			return state;
		}

		public void UnRegisterState (string stateIndex)
		{
			if (!_stateDic.TryGetValue(stateIndex, out ISkeletonState state))
			{
				return;
			}

			state.Dispose();
			_stateDic.Remove(stateIndex);
		}

		public void SetDefault(ISkeletonState state)
		{
			SetDefault(state.AnimeName);
		}

		public void SetDefault (string stateIndex)
		{
			if (!_stateDic.TryGetValue(stateIndex, out ISkeletonState state))
			{
				return;
			}

			if (_skeleton == null)
			{
#if UNITY_EDITOR
				Debug.LogError("Skeleton Animation Null");
#endif
				return;
			}

			_defaultState = state;
			_skeleton.AnimationState.Start += OnStart;
			_skeleton.AnimationState.Complete += OnComplete;
			_skeleton.AnimationState.End += OnEnd;
			_skeleton.AnimationState.Event += OnEvent;
		}

		public void BackDefault ()
		{
			if (_defaultState == null)
			{
				return;
			}

			ChangeState(_defaultState);
		}

		public void Dispose ()
		{
			if (_isRunning)
			{
				Stop();
			}

			_skeleton.AnimationState.Start -= OnStart;
			_skeleton.AnimationState.Complete -= OnComplete;
			_skeleton.AnimationState.End -= OnEnd;
			_skeleton.AnimationState.Event -= OnEvent;

			foreach (var state in _stateDic.Values)
			{
				state.Dispose();
			}

			_stateDic.Clear();
			_stateDic = null;
			_skeleton = null;
		}

		public void InvokeTrigger (string triggerName, bool value = true)
		{
			
			if (!_trigger.ContainsKey(triggerName))
            {
                _trigger.Add(triggerName, false);
            }
			_trigger[triggerName] = value;
		}

		public bool Trigger(string triggerName)
		{
			return _trigger.ContainsKey(triggerName) && _trigger[triggerName];
		}

		public void ClearTrigger()
        {
            _trigger.Clear();
        }

		private void OnStart (TrackEntry track)
		{
			var state = GetState(track.Animation.Name);
			if (state == null)
			{
				return;
			}

			if(Trigger(track.Animation.Name) == false)
            {
				InvokeTrigger(track.Animation.Name);
            }

			state.OnStart(track);
		}

		// 不要在该回调上SetAnimation 
		private void OnEnd (TrackEntry track)
		{
			var state = GetState(track.Animation.Name);
			if (state == null)
			{
				return;
			}

			// Complete没触发 由End复原
			if(Trigger(track.Animation.Name))
            {
				InvokeTrigger(track.Animation.Name, false);
            }

			state.OnEnd(track);
		}

		private void OnComplete (TrackEntry track)
		{
			var state = GetState(track.Animation.Name);
			if (state == null)
			{
				return;
			}

			// Once 直接触发
			if(track.Loop == false && Trigger(track.Animation.Name))
            {
				InvokeTrigger(track.Animation.Name, false);
            }

			// 调用该动画结束事件
			state.OnComplete(track);

			// 获取Complete后需要转换的状态
			var nextState = state.NextStateOnComplete();
			if(nextState == null)
			{
				return;
			}

			ChangeState(GetState(nextState));
		}

		private void OnEvent (TrackEntry track, Spine.Event e)
		{
			if (_currentState == null)
			{
				return;
			}
			
			_currentState.OnEvent(track, e);
		}

		public void StartMachine ()
		{
			Start(_defaultState);
		}

		private void Start (ISkeletonState defaultState)
		{
			if (defaultState == null)
			{
				return;
			}

			_isRunning = true;

			if (_defaultState != defaultState)
			{
				_defaultState = defaultState;
			}

			_currentState = defaultState;
			var trackEntry = _currentState.PlayAnime();
			if(trackEntry == null)
			{
				# if UNITY_EDITOR
					Debug.LogError($"[SkeletonStateMachine.{_skeleton.gameObject.name}] 未播放动画");
				#endif
				return;
			}
			
			_layerToTrack[defaultState.Layer] = trackEntry;
		}

		// 更新状态机
		public void UpdateMachine ()
		{
			if (!_isRunning)
			{
				return;
			}

			Update();
		}

		private void Update ()
		{
			if (_currentState == null)
			{
				return;
			}

			// TODO 上下文传递维护
			var nextState = _currentState.NextStateOnUpdate();
			if(nextState != null && _currentState.AllowInterrupt())
			{
				ChangeState(GetState(nextState));
				return;
			}
		}

		public void Resume()
        {
            Start(_currentState);
        }

		public void Stop ()
		{
			if (_isRunning)
			{
				_isRunning = false;
			}
		}

		// 重置状态机
		public void ResetMachine ()
		{
			Stop();
			ClearTrigger();
			ChangeState(_defaultState);
		}

		public void ChangeState (ISkeletonState state)
		{
			if (state == null)
			{
				return;
			}
			
			_currentState = state;
			var trackEntry = _currentState.PlayAnime();
			if(trackEntry == null)
			{
				# if UNITY_EDITOR
					Debug.LogError($"[SkeletonStateMachine.{_skeleton.gameObject.name}] 未播放动画");
				#endif
				return;
			}

			_layerToTrack[_currentState.Layer] = trackEntry;
			_skeleton.Update(0);
			_skeleton.LateUpdate();
		}

		public void ClearTrackForLayer (SkeletonLayer layer)
		{
			if (!_layerToTrack.TryGetValue(layer, out TrackEntry track))
			{
				return;
			}

			_skeleton.AnimationState.ClearTrack((int)layer);
		}


		// 持有空动画过度
		public void ClearTrackForLayer (SkeletonLayer layer, float mixDuration = 0)
		{
			if (!_layerToTrack.TryGetValue(layer, out TrackEntry track))
			{
				return;
			}

			var emptyTrack = _skeleton.AnimationState.SetEmptyAnimation((int)layer, mixDuration);
			emptyTrack.Complete += (track) =>
			{
				_skeleton.AnimationState.ClearTrack((int)layer);
			};
		}

		// 打断循环的轨道
		public void InterruptTrack (SkeletonLayer layer, float mixDuration = 0, float delay = 0)
		{
			if (!_layerToTrack.TryGetValue(layer, out TrackEntry track))
			{
				return;
			}

			_layerToTrack[layer] = _skeleton.AnimationState.SetEmptyAnimation((int)layer, mixDuration);
			_skeleton.Update(0);
			_skeleton.LateUpdate();
		}

		// 在轨道上播放动画
		public TrackEntry PlayTrackAnimation (SkeletonLayer layer, string animeName, float mix = 0, bool isLoop = false)
		{
			_layerToTrack[layer] = _skeleton.AnimationState.SetAnimation((int)layer, animeName, isLoop);
			_skeleton.AnimationState.Data.SetMix(_currentState.AnimeName, animeName, mix);
			_skeleton.Update(0);
			_skeleton.LateUpdate();
			return _layerToTrack[layer];
		}

		public void PlayTrackAnimation (string animeName)
		{
			var state = GetState(animeName);
			if (state == null)
			{
				return;
			}

			_layerToTrack[state.Layer] = _skeleton.AnimationState.SetAnimation((int)state.Layer, state.AnimeName, state.IsLoop);
			_skeleton.Update(0);
			_skeleton.LateUpdate();
		}

		public void PlayTrackAnimationForce(string animeName)
		{
			var state = GetState(animeName);
			if (state == null)
			{
				return;
			}

			ClearTrackForLayer(state.Layer);
			// 关闭混合
			_skeleton.AnimationState.Data.SetMix(_currentState.AnimeName, animeName, 0);
			_layerToTrack[state.Layer] = _skeleton.AnimationState.SetAnimation((int)state.Layer, state.AnimeName, state.IsLoop);
			_skeleton.Update(0);
			_skeleton.LateUpdate();
		}

		public void PlayTrackAnimationForce(ISkeletonState skeletonStatestate)
		{
			PlayTrackAnimationForce(skeletonStatestate.AnimeName);
		}
		 

	}

}
