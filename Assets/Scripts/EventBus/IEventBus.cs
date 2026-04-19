using System;

namespace Core.Events
{
	public interface IEventBus
	{
		EventSubscription Subscribe<T>(Action<T> callback, EventPriority priority = EventPriority.Normal) where T : struct, IEvent;

		void Publish<T>(T eventData) where T : struct, IEvent;
	}
}
