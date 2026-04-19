using System;
using System.Buffers;
using System.Collections.Generic;

namespace Core.Events
{
	public sealed class EventBus : IEventBus
	{
		private readonly Dictionary<Type, object> _listenersByType = new();

		public EventSubscription Subscribe<T>(
			Action<T> callback,
			EventPriority priority = EventPriority.Normal) where T : struct, IEvent
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			var collection = GetOrCreateListenerCollection<T>();
			var listener = collection.Add(callback, (int)priority);

			return new EventSubscription(() => Unsubscribe(typeof(T), collection, listener));
		}

		public void Publish<T>(T eventData) where T : struct, IEvent
		{
			if (!_listenersByType.TryGetValue(typeof(T), out var collectionObject))
			{
				return;
			}

			if (collectionObject is not ListenerCollection<T> collection)
			{
				return;
			}

			collection.Publish(eventData);
		}

		private ListenerCollection<T> GetOrCreateListenerCollection<T>() where T : struct, IEvent
		{
			var eventType = typeof(T);
			if (_listenersByType.TryGetValue(eventType, out var collectionObject) &&
				collectionObject is ListenerCollection<T> existingCollection)
			{
				return existingCollection;
			}

			var createdCollection = new ListenerCollection<T>();
			_listenersByType[eventType] = createdCollection;
			return createdCollection;
		}

		private void Unsubscribe<T>(
			Type eventType,
			ListenerCollection<T> collection,
			EventListener<T> listener) where T : struct, IEvent
		{
			listener.Deactivate();
			collection.Remove(listener);

			if (!collection.isEmpty)
			{
				return;
			}

			_listenersByType.Remove(eventType);
		}

		private sealed class ListenerCollection<T> where T : struct, IEvent
		{
			private readonly List<EventListener<T>> _listeners = new();

			public bool isEmpty => _listeners.Count == 0;

			public EventListener<T> Add(Action<T> callback, int priority)
			{
				var listener = new EventListener<T>(callback, priority);
				_listeners.Add(listener);
				_listeners.Sort(static (left, right) => right.Priority.CompareTo(left.Priority));
				return listener;
			}

			public void Remove(EventListener<T> listener)
			{
				_listeners.Remove(listener);
			}

			public void Publish(T eventData)
			{
				var listenerCount = _listeners.Count;
				if (listenerCount == 0)
				{
					return;
				}

				// Snapshot avoids mutation during dispatch without a per-publish ToArray() allocation.
				var pool = ArrayPool<EventListener<T>>.Shared;
				var snapshot = pool.Rent(listenerCount);
				try
				{
					for (var i = 0; i < listenerCount; i++)
					{
						snapshot[i] = _listeners[i];
					}

					for (var index = 0; index < listenerCount; index++)
					{
						var listener = snapshot[index];
						if (!listener.IsActive)
						{
							continue;
						}

						listener.Invoke(eventData);
					}
				}
				finally
				{
					pool.Return(snapshot, clearArray: false);
				}
			}
		}
	}
}
