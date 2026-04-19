using Core.Events;

namespace Tests.Editor
{
    internal struct TestEvent : IEvent
    {
        public int Value { get; set; }
    }

    internal struct OtherEvent : IEvent
    {
        public string Tag { get; set; }
    }
}
