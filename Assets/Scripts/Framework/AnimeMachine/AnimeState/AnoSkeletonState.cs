using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	public class AnoSkeletonState : SkeletonState
	{
		private Action<ISkeletonState, TrackEntry> _onComplate;
		private Action<ISkeletonState, TrackEntry> _onStart;
		private Action<ISkeletonState, TrackEntry> _onEnd;

		public AnoSkeletonState() {}
		
		public AnoSkeletonState AddAnoTransition (string toStateIndex, Func<bool> cond, bool canExit = false, Func<float> getMix = null)
		{
			return AddTransition(toStateIndex, cond, canExit, getMix == null ? 0.2f : getMix.Invoke()) as AnoSkeletonState;
		}

		public AnoSkeletonState AddAnoAnimationEvent (string eventName, Action<TrackEntry, Spine.Event> action)
		{
			return AddAnimationEvent(eventName, action) as AnoSkeletonState;
		}

		public AnoSkeletonState OnAnimationComplate (Action<ISkeletonState, TrackEntry> action)
		{
			_onComplate = action;
			return this;
		}

		public AnoSkeletonState OnAnimationStart (Action<ISkeletonState, TrackEntry> action)
		{
			_onStart = action;
			return this;
		}

		public AnoSkeletonState OnAnimationEnd (Action<ISkeletonState, TrackEntry> action)
		{

			_onEnd = action;
			return this;
		}

		public override void OnComplete (TrackEntry track)
		{
			_onComplate?.Invoke(this, track);
		}

		public override void OnStart (TrackEntry track)
		{
			_onStart?.Invoke(this, track);
		}

		public override void OnEnd (TrackEntry track)
		{
			_onEnd?.Invoke(this, track);
		}

		public override TrackEntry PlayAnime()
		{
			return _machine.PlayTrackAnimation(this.Layer, this.AnimeName, _nextStateTransition != null ? _nextStateTransition.MixDuration : 0.2f, _isLoop);
		}

		private bool _allowInterrupt = true;

		public AnoSkeletonState Interrupt(bool value)
        {
			_allowInterrupt = value;
           return this;
        }

		public override bool AllowInterrupt() => _allowInterrupt;
	}
}

