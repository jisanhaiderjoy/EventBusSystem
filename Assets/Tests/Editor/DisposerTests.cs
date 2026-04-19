using System;
using System.Collections.Generic;
using Core.Disposables;
using Core.Events;
using NUnit.Framework;

namespace Tests.Editor
{
    public class DisposerTests
    {
        [Test]
        public void Disposer_Add_Then_Dispose_CallsAll()
        {
            var disposer = new Disposer();
            int action1Count = 0;
            int action2Count = 0;

            disposer.Add(new ActionDisposable(() => action1Count++));
            disposer.Add(new ActionDisposable(() => action2Count++));

            disposer.Dispose();

            Assert.AreEqual(1, action1Count);
            Assert.AreEqual(1, action2Count);
        }

        [Test]
        public void Disposer_Dispose_IsLIFO()
        {
            var disposer = new Disposer();
            var callOrder = new List<string>();

            disposer.Add(new ActionDisposable(() => callOrder.Add("First")));
            disposer.Add(new ActionDisposable(() => callOrder.Add("Second")));

            disposer.Dispose();

            Assert.AreEqual(2, callOrder.Count);
            Assert.AreEqual("Second", callOrder[0]);
            Assert.AreEqual("First", callOrder[1]);
        }

        [Test]
        public void Disposer_Dispose_Twice_IsNoOp()
        {
            var disposer = new Disposer();
            int callCount = 0;

            disposer.Add(new ActionDisposable(() => callCount++));

            disposer.Dispose();
            disposer.Dispose();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Disposer_Add_AfterDispose_ThrowsObjectDisposedException()
        {
            var disposer = new Disposer();
            disposer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => disposer.Add(new ActionDisposable(() => { })));
        }

        [Test]
        public void ActionDisposable_Dispose_InvokesAction()
        {
            int callCount = 0;
            var actionDisposable = new ActionDisposable(() => callCount++);

            actionDisposable.Dispose();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void ActionDisposable_Dispose_Twice_InvokesActionOnce()
        {
            int callCount = 0;
            var actionDisposable = new ActionDisposable(() => callCount++);

            actionDisposable.Dispose();
            actionDisposable.Dispose();

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void AddTo_RegistersWithDisposer_AndReturnsOriginal()
        {
            var disposer = new Disposer();
            int actionCount = 0;
            var actionDisposable = new ActionDisposable(() => actionCount++);

            var returnedDisposable = actionDisposable.AddTo(disposer);

            Assert.AreSame(actionDisposable, returnedDisposable);
            
            disposer.Dispose();
            Assert.AreEqual(1, actionCount);
        }

        [Test]
        public void EventSubscription_AddTo_DisposedWhenDisposerDisposed()
        {
            var disposer = new Disposer();
            int actionCount = 0;
            
            var subscription = new EventSubscription(() => actionCount++).AddTo(disposer);

            disposer.Dispose();

            Assert.AreEqual(1, actionCount);
            Assert.IsTrue(subscription.IsDisposed);
        }
    }
}
