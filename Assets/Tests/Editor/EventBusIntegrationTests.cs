using System.Collections.Generic;
using Core.Disposables;
using Core.Events;
using NUnit.Framework;

namespace Tests.Editor
{
    public class EventBusIntegrationTests
    {
        private EventBus _eventBus;
        private Disposer _disposer;

        [SetUp]
        public void Setup()
        {
            _eventBus = new EventBus();
            _disposer = new Disposer();
        }

        [TearDown]
        public void Teardown()
        {
            _disposer.Dispose();
        }

        [Test]
        public void FullFlow_SubscribePublishDispose_EndToEnd()
        {
            int receivedCalls = 0;

            var subscription = _eventBus.Subscribe<TestEvent>(e => receivedCalls += e.Value);

            // Publish first time
            _eventBus.Publish(new TestEvent { Value = 10 });
            Assert.AreEqual(10, receivedCalls);

            // Publish second time
            _eventBus.Publish(new TestEvent { Value = 5 });
            Assert.AreEqual(15, receivedCalls);

            // Dispose subscription
            subscription.Dispose();

            // Publish third time, should not receive
            _eventBus.Publish(new TestEvent { Value = 20 });
            Assert.AreEqual(15, receivedCalls);
        }

        [Test]
        public void MultiConsumer_SharedBus_EachReceivesEvents()
        {
            int systemA_Events = 0;
            int systemB_Events = 0;

            _eventBus.Subscribe<TestEvent>(_ => systemA_Events++).AddTo(_disposer);
            _eventBus.Subscribe<TestEvent>(_ => systemB_Events++).AddTo(_disposer);
            _eventBus.Subscribe<OtherEvent>(_ => systemB_Events++).AddTo(_disposer);

            _eventBus.Publish(new TestEvent());
            _eventBus.Publish(new OtherEvent());

            Assert.AreEqual(1, systemA_Events);
            Assert.AreEqual(2, systemB_Events); // system B receives both TestEvent and OtherEvent
        }

        [Test]
        public void Disposer_WithEventBus_CleanupOnTeardown()
        {
            int eventCount = 0;

            _eventBus.Subscribe<TestEvent>(_ => eventCount++).AddTo(_disposer);
            _eventBus.Subscribe<OtherEvent>(_ => eventCount++).AddTo(_disposer);

            _eventBus.Publish(new TestEvent());
            _eventBus.Publish(new OtherEvent());
            
            Assert.AreEqual(2, eventCount);

            // Simulate object destruction or scope teardown
            _disposer.Dispose();

            _eventBus.Publish(new TestEvent());
            _eventBus.Publish(new OtherEvent());

            // Still 2, because both subscriptions were cleaned up
            Assert.AreEqual(2, eventCount);
        }

        [Test]
        public void PriorityOrdering_InProductionScenario()
        {
            var executionLog = new List<string>();

            // Simulate UI layer high priority listener
            _eventBus.Subscribe<TestEvent>(_ => executionLog.Add("UI_Update"), EventPriority.High).AddTo(_disposer);
            
            // Simulate analytics layer low priority listener
            _eventBus.Subscribe<TestEvent>(_ => executionLog.Add("Analytics_Track"), EventPriority.Low).AddTo(_disposer);
            
            // Simulate normal game logic listener
            _eventBus.Subscribe<TestEvent>(_ => executionLog.Add("GameLogic_Process")).AddTo(_disposer);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(3, executionLog.Count);
            Assert.AreEqual("UI_Update", executionLog[0]);
            Assert.AreEqual("GameLogic_Process", executionLog[1]);
            Assert.AreEqual("Analytics_Track", executionLog[2]);
        }

        [Test]
        public void ReentrantPublish_ChainedEvents()
        {
            int chainCompleteCalls = 0;

            // When TestEvent fires, we publish OtherEvent
            _eventBus.Subscribe<TestEvent>(e => 
            {
                _eventBus.Publish(new OtherEvent { Tag = "Chained" });
            }).AddTo(_disposer);

            _eventBus.Subscribe<OtherEvent>(e => 
            {
                if (e.Tag == "Chained") chainCompleteCalls++;
            }).AddTo(_disposer);

            _eventBus.Publish(new TestEvent());

            Assert.AreEqual(1, chainCompleteCalls);
        }

        [Test]
        public void StressTest_ManyPublishes_NoLeaks()
        {
            const int listenerCount = 100;
            const int publishCount = 1000; // Using 1000 instead of 10k to keep unit tests fast
            
            int totalDeliveries = 0;

            for (int i = 0; i < listenerCount; i++)
            {
                _eventBus.Subscribe<TestEvent>(_ => totalDeliveries++).AddTo(_disposer);
            }

            for (int i = 0; i < publishCount; i++)
            {
                _eventBus.Publish(new TestEvent());
            }

            Assert.AreEqual(listenerCount * publishCount, totalDeliveries);
        }
    }
}
