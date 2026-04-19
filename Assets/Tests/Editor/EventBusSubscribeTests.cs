using System;
using Core.Events;
using NUnit.Framework;

namespace Tests.Editor
{
    public class EventBusSubscribeTests
    {
        private EventBus _eventBus;

        [SetUp]
        public void Setup()
        {
            _eventBus = new EventBus();
        }

        [Test]
        public void Subscribe_NullCallback_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _eventBus.Subscribe<TestEvent>(null));
        }

        [Test]
        public void Subscribe_Returns_NonNullSubscription()
        {
            var sub = _eventBus.Subscribe<TestEvent>(_ => { });
            Assert.IsNotNull(sub);
        }

        [Test]
        public void Unsubscribe_Via_Dispose_StopsCallbacks()
        {
            int callCount = 0;
            var sub = _eventBus.Subscribe<TestEvent>(_ => callCount++);

            _eventBus.Publish(new TestEvent());
            Assert.AreEqual(1, callCount);

            sub.Dispose();
            _eventBus.Publish(new TestEvent());
            
            Assert.AreEqual(1, callCount, "Callback should not be invoked after dispose");
        }

        [Test]
        public void Unsubscribe_IsIdempotent()
        {
            var sub = _eventBus.Subscribe<TestEvent>(_ => { });
            
            Assert.DoesNotThrow(() => 
            {
                sub.Dispose();
                sub.Dispose();
            });
        }

        [Test]
        public void Subscribe_After_Unsubscribe_WorksCorrectly()
        {
            int callCount1 = 0;
            int callCount2 = 0;

            var sub1 = _eventBus.Subscribe<TestEvent>(_ => callCount1++);
            sub1.Dispose();

            var sub2 = _eventBus.Subscribe<TestEvent>(_ => callCount2++);
            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(0, callCount1);
            Assert.AreEqual(1, callCount2);
        }

        [Test]
        public void Unsubscribe_During_Publish_DoesNotThrow()
        {
            EventSubscription sub = null;
            int callCount = 0;

            sub = _eventBus.Subscribe<TestEvent>(_ => 
            {
                callCount++;
                sub.Dispose(); // Unsubscribe self
            });

            Assert.DoesNotThrow(() => _eventBus.Publish(new TestEvent()));
            Assert.AreEqual(1, callCount);

            _eventBus.Publish(new TestEvent());
            Assert.AreEqual(1, callCount, "Should not be called again");
        }

        [Test]
        public void MultipleSubscribers_AllReceiveEvent()
        {
            int a = 0, b = 0, c = 0;
            
            _eventBus.Subscribe<TestEvent>(_ => a++);
            _eventBus.Subscribe<TestEvent>(_ => b++);
            _eventBus.Subscribe<TestEvent>(_ => c++);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(1, a);
            Assert.AreEqual(1, b);
            Assert.AreEqual(1, c);
        }

        [Test]
        public void Subscription_IsDisposed_Property()
        {
            var sub = _eventBus.Subscribe<TestEvent>(_ => { });
            
            Assert.IsFalse(sub.IsDisposed);
            
            sub.Dispose();
            
            Assert.IsTrue(sub.IsDisposed);
        }
    }
}
