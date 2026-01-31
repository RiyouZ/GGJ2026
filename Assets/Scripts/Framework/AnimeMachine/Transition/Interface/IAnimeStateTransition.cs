using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	public interface IAnimeStateTransition
	{
		public string ToState {get;}
		public Func<bool> TransitionCondition {get;}
		public bool CanComplateExit {get;}
		public float MixDuration {get;}
	}

}
