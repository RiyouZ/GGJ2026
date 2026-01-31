using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	public class AnimeStateTransition : IAnimeStateTransition
	{
		public string FromState
		{
			get; private set;
		}

		public string ToState 
		{
			get; private set;
		}

		private Func<bool> _transitionAction;
		public Func<bool> TransitionCondition => _transitionAction;

		// 需要播放完成后退出
		private bool _canComplateExit;
		public bool CanComplateExit => _canComplateExit;

		public bool IsValid
		{
			private set; get;
		}

		public float MixDuration
		{
			private set; get;
		}

		public AnimeStateTransition (string from, string to, Func<bool> action = null, bool canExit = false, float mixDuration = 0.2f)
		{
			IsValid = true;
			FromState = from;
			ToState = to;
			_canComplateExit = canExit;
			_transitionAction = action;
			MixDuration = mixDuration;
		}
	}
}

