using System;

namespace Core.Events
{
	public sealed class EventSubscription : IDisposable
	{
		private Action _unsubscribe;

		public EventSubscription(Action unsubscribe)
		{
			_unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
		}

		public bool IsDisposed => _unsubscribe == null;

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			_unsubscribe.Invoke();
			_unsubscribe = null;
		}
	}
}
