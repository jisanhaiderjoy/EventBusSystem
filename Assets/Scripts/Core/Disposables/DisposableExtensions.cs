using System;

namespace Core.Disposables
{
    /// <summary>
    /// Fluent extension methods for registering <see cref="IDisposable"/> instances
    /// with a <see cref="Disposer"/>.
    /// </summary>
    public static class DisposableExtensions
    {
        /// <summary>
        /// Registers <paramref name="disposable"/> with <paramref name="disposer"/> and
        /// returns the original instance so call-chains can continue.
        /// <example>
        /// <code>
        /// _eventBus.Subscribe&lt;PlayerClickEvent&gt;(OnPlayerClickEvent).AddTo(_disposer);
        /// </code>
        /// </example>
        /// </summary>
        public static T AddTo<T>(this T disposable, Disposer disposer) where T : IDisposable
        {
            disposer.Add(disposable);
            return disposable;
        }
    }
}
