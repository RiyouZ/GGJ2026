using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace RuGameFramework.Event
{
	public struct EventHandle<T> where T : Delegate
	{
		public int eventId;
		public T action;

		public EventHandle (int eventId,T action)
		{
			this.eventId = eventId;
			this.action = action;
		}
	}
}

