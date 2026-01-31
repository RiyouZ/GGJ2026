using Spine;
using System;
using System.Collections.Generic;


namespace RuGameFramework.AnimeStateMachine
{
	public interface ISkeletonState
	{
		public string AnimeName
		{
			set;
			get;
		}

		public bool IsLoop
		{
			set;
			get;
		}

		public SkeletonLayer Layer
		{
			set;
			get;
		}

		public List<IAnimeStateTransition> StateTransitions
		{
			set;
			get;
		}


		public void Initialize(SkeletonStateMachine machine);

		public bool AllowInterrupt();
		public ISkeletonState AddTransition (string toStateIndex, Func<bool> cond, bool canExit, float mix);
		public TrackEntry PlayAnime(); 
		public string NextStateOnUpdate();
		public string NextStateOnComplete();
		public void OnStart (TrackEntry track);
		public void OnComplete (TrackEntry track);
		public void OnEnd (TrackEntry track);
		public void OnEvent (TrackEntry track, Spine.Event e);
		public void Dispose ();
	}
}


