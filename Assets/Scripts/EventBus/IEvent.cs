namespace Core.Events
{
	/// <summary>
	/// Marker interface for all EventBus payloads.
	/// Prefer readonly struct events to minimize GC pressure.
	/// </summary>
	public interface IEvent
	{
	}
}
