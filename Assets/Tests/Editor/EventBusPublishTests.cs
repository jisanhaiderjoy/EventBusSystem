using System.Collections.Generic;
using Core.Events;
using NUnit.Framework;

namespace Tests.Editor
{
    public class EventBusPublishTests
    {
        private EventBus _eventBus;

        [SetUp]
        public void Setup()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _eventBus.Publish(new TestEvent { Value = 42 }));
        }

        [Test]
        public void Publish_DeliversEventData_Correctly()
        {
            int receivedValue = 0;
            _eventBus.Subscribe<TestEvent>(e => receivedValue = e.Value);

            _eventBus.Publish(new TestEvent { Value = 99 });

            Assert.AreEqual(99, receivedValue);
        }

        [Test]
        public void Publish_HighPriority_CalledBeforeNormal()
        {
            var callOrder = new List<string>();

            _eventBus.Subscribe<TestEvent>(_ => callOrder.Add("Normal"), EventPriority.Normal);
            _eventBus.Subscribe<TestEvent>(_ => callOrder.Add("High"), EventPriority.High);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(2, callOrder.Count);
            Assert.AreEqual("High", callOrder[0]);
            Assert.AreEqual("Normal", callOrder[1]);
        }

        [Test]
        public void Publish_NormalPriority_CalledBeforeLow()
        {
            var callOrder = new List<string>();

            _eventBus.Subscribe<TestEvent>(_ => callOrder.Add("Low"), EventPriority.Low);
            _eventBus.Subscribe<TestEvent>(_ => callOrder.Add("Normal"), EventPriority.Normal);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(2, callOrder.Count);
            Assert.AreEqual("Normal", callOrder[0]);
            Assert.AreEqual("Low", callOrder[1]);
        }

        [Test]
        public void Publish_SamePriority_AllInvoked()
        {
            int calls = 0;
            _eventBus.Subscribe<TestEvent>(_ => calls++, EventPriority.Normal);
            _eventBus.Subscribe<TestEvent>(_ => calls++, EventPriority.Normal);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(2, calls);
        }

        [Test]
        public void Publish_DifferentEventTypes_AreIndependent()
        {
            int testEventCalls = 0;
            int otherEventCalls = 0;

            _eventBus.Subscribe<TestEvent>(_ => testEventCalls++);
            _eventBus.Subscribe<OtherEvent>(_ => otherEventCalls++);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(1, testEventCalls);
            Assert.AreEqual(0, otherEventCalls);
        }

        [Test]
        public void Publish_AfterAllUnsubscribed_DoesNotThrow()
        {
            var sub1 = _eventBus.Subscribe<TestEvent>(_ => { });
            var sub2 = _eventBus.Subscribe<TestEvent>(_ => { });

            sub1.Dispose();
            sub2.Dispose();

            // The underlying collection is removed from the dictionary
            Assert.DoesNotThrow(() => _eventBus.Publish(new TestEvent()));
        }

        [Test]
        public void Publish_ReentrantPublish_DoesNotThrow()
        {
            int testEventCalls = 0;
            int otherEventCalls = 0;

            _eventBus.Subscribe<TestEvent>(_ => 
            {
                testEventCalls++;
                _eventBus.Publish(new OtherEvent());
            });

            _eventBus.Subscribe<OtherEvent>(_ => otherEventCalls++);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(1, testEventCalls);
            Assert.AreEqual(1, otherEventCalls);
        }

        [Test]
        public void Publish_SubscribeDuringPublish_NewListenerNotCalledThisRound()
        {
            int originalCalls = 0;
            int newCalls = 0;

            _eventBus.Subscribe<TestEvent>(_ => 
            {
                originalCalls++;
                _eventBus.Subscribe<TestEvent>(_ => newCalls++);
            });

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(1, originalCalls);
            Assert.AreEqual(0, newCalls, "Newly subscribed listener should not be called in the current dispatch");

            // But it should be called on the next publish
            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(2, originalCalls);
            Assert.AreEqual(1, newCalls);
        }
    }
}
