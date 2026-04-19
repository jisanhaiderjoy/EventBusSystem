namespace Core.Events
{
	public readonly struct PlayerDiedEvent : IEvent
	{
		public PlayerDiedEvent(int playerId)
		{
			PlayerId = playerId;
		}

		public int PlayerId { get; }
	}
}
