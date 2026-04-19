using System;
using Core.Events;
using NUnit.Framework;

namespace Tests.Editor
{
    public class EventSubscriptionTests
    {
        [Test]
        public void Constructor_NullAction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new EventSubscription(null));
        }

        [Test]
        public void Dispose_CallsUnsubscribeAction()
        {
            int actionCalls = 0;
            var sub = new EventSubscription(() => actionCalls++);

            sub.Dispose();

            Assert.AreEqual(1, actionCalls);
        }

        [Test]
        public void Dispose_Twice_ActionInvokedOnce()
        {
            int actionCalls = 0;
            var sub = new EventSubscription(() => actionCalls++);

            sub.Dispose();
            sub.Dispose(); // idempotent

            Assert.AreEqual(1, actionCalls);
        }

        [Test]
        public void IsDisposed_FalseBeforeDispose()
        {
            var sub = new EventSubscription(() => { });
            Assert.IsFalse(sub.IsDisposed);
        }

        [Test]
        public void IsDisposed_TrueAfterDispose()
        {
            var sub = new EventSubscription(() => { });
            sub.Dispose();
            Assert.IsTrue(sub.IsDisposed);
        }
    }
}
