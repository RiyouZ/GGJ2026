using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

namespace RuGameFramework.AnimeStateMachine
{
	public class TransitionSkeletonState : SkeletonState
	{
		// 转换条件信息
		public string ToState
		{
			get; set;
		}

		public TransitionSkeletonState(SkeletonStateMachine machine, SkeletonLayer layer, string animeName)
		{
			this._layer = layer;
			this._machine = machine;
			this.AnimeName = animeName;
		}

		public override string NextStateOnComplete()
		{
			// 存在到其他条件
			if(_nextStateTransition != null)
			{
				return _nextStateTransition.ToState;
			}

			return ToState;
		}
		
		public override TrackEntry PlayAnime()
		{
			return _machine.PlayTrackAnimation(this.Layer, this.AnimeName, 0, false);
		}
	}

}
