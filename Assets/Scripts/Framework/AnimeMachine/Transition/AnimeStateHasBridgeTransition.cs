using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	public class AnimeStateHasBridgeTransition : IAnimeStateTransition
	{
		public string FromState
		{
			get; private set;
		}

		public string ToState 
		{
			get; private set;
		}

		public string BridgeState
		{
			get; private set;
		}

		private Func<bool> _transitionAction;
		public Func<bool> TransitionCondition => _transitionAction;

		// 需要播放完成后退出
		private bool _canComplateExit;
		public bool CanComplateExit => _canComplateExit;

		public float MixDuration 
		{
			get; private set;
		}

		public AnimeStateHasBridgeTransition(string from, string to, string bridge, Func<bool> action = null, bool canExit = false, float mix = 0)
		{
			FromState = from;
			BridgeState = bridge;
			MixDuration = 0;
			ToState = to;
			_canComplateExit = canExit;
			_transitionAction = action;	
		}
	}

}
