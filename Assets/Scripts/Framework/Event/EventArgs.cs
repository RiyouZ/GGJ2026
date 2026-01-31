using System;


namespace RuGameFramework.Event
{
	public interface IGameEventArgs
	{
		public void Dispose ();
	}

	public class NullArgs : IGameEventArgs
	{
		public void Dispose ()
		{

		}
	}

}
