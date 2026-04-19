using System;
using Core.Events;
using NUnit.Framework;

namespace Tests.Editor
{
    public class EventBusConcurrencyTests
    {
        private EventBus _eventBus;

        [SetUp]
        public void Setup()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void Publish_ListenerUnsubscribesDuringSelf_Callback_NoCrash()
        {
            EventSubscription sub1 = null;
            EventSubscription sub2 = null;
            int callCount1 = 0;
            int callCount2 = 0;

            sub1 = _eventBus.Subscribe<TestEvent>(_ => 
            {
                callCount1++;
                sub1.Dispose(); // Unsubscribe self
            });

            sub2 = _eventBus.Subscribe<TestEvent>(_ => callCount2++);

            Assert.DoesNotThrow(() => _eventBus.Publish(new TestEvent()));

            Assert.AreEqual(1, callCount1);
            Assert.AreEqual(1, callCount2);
        }

        [Test]
        public void Publish_ListenerSubscribesNewListener_NoCrash()
        {
            int originalCalls = 0;

            _eventBus.Subscribe<TestEvent>(_ => 
            {
                originalCalls++;
                _eventBus.Subscribe<TestEvent>(_ => { }); // Subscribes another listener
            });

            Assert.DoesNotThrow(() => _eventBus.Publish(new TestEvent()));

            Assert.AreEqual(1, originalCalls);
        }

        [Test]
        public void Publish_ManyListeners_AllCalled()
        {
            const int listenerCount = 50;
            int totalCalls = 0;

            for (int i = 0; i < listenerCount; i++)
            {
                _eventBus.Subscribe<TestEvent>(_ => totalCalls++);
            }

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(listenerCount, totalCalls);
        }

        [Test]
        public void Publish_ExceptionInCallback_PropagatesNormally()
        {
            _eventBus.Subscribe<TestEvent>(_ => throw new InvalidOperationException("Test Exception"));

            // An exception inside a listener callback should surface to the publisher,
            // but the finally block in EventBus handles returning the array to the pool.
            Assert.Throws<InvalidOperationException>(() => _eventBus.Publish(new TestEvent()));
        }
    }
}
