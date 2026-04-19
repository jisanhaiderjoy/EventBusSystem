using System;

namespace Core.Events
{
	internal sealed class EventListener<T> where T : struct, IEvent
	{
		private readonly Action<T> _callback;

		public EventListener(Action<T> callback, int priority)
		{
			_callback = callback ?? throw new ArgumentNullException(nameof(callback));
			Priority = priority;
			IsActive = true;
		}

		public int Priority { get; }
		public bool IsActive { get; private set; }

		public void Invoke(T eventData)
		{
			if (!IsActive)
			{
				return;
			}

			_callback.Invoke(eventData);
		}

		public void Deactivate() => IsActive = false;
	}
}
