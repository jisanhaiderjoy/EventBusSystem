using System;

namespace Core.Disposables
{
    /// <summary>
    /// Adapts any <see cref="Action"/> into an <see cref="IDisposable"/> so it can
    /// be registered with a <see cref="Disposer"/> via <c>AddTo</c>.
    /// Useful for Unity event listeners that expose Add/Remove pairs rather than
    /// returning an <see cref="IDisposable"/> directly.
    /// <example>
    /// <code>
    /// new ActionDisposable(() => _view.button.onClick.RemoveListener(OnClick))
    ///     .AddTo(_disposer);
    /// </code>
    /// </example>
    /// </summary>
    public sealed class ActionDisposable : IDisposable
    {
        private Action _onDispose;

        public ActionDisposable(Action onDispose)
        {
            _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        /// <summary>
        /// Invokes the cleanup action. Safe to call multiple times — subsequent calls are no-ops.
        /// </summary>
        public void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }
}
