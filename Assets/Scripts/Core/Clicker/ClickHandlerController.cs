using Core.Disposables;
using Core.Events;
using DG.Tweening;
using VContainer.Unity;

namespace UnityEngine
{
    public class ClickHandlerController : IStartable
    {
        private readonly ClickerView _view;
        private readonly ClickerConfig _clickerConfig;
        private readonly IEventBus _eventBus;
        private readonly Disposer _disposer;

        public ClickHandlerController(ClickerView view,
            ClickerConfig clickerConfig,
            IEventBus eventBus,
            Disposer disposer)
        {
            _view = view;
            _clickerConfig = clickerConfig;
            _eventBus = eventBus;
            _disposer = disposer;
        }

        public void Start()
        {
            _eventBus.Subscribe<PlayerClickEvent>(OnPlayerClickEvent).AddTo(_disposer);
        }

        private void OnPlayerClickEvent(PlayerClickEvent evt)
        {
            var xpGameObject = GameObject.Instantiate(_view.xpPrefab, _view.panel.transform);
            var xpTransform = xpGameObject.transform;

            Sequence mySequence = DOTween.Sequence();

            var moveDirection = (_clickerConfig.moveDirection * _clickerConfig.moveValue);
            moveDirection += xpTransform.localPosition;
            var moveTween = xpTransform.DOLocalMove(moveDirection, _clickerConfig.animationDuration);
            var scaleTween = xpTransform.DOScale(_clickerConfig.finalScale, _clickerConfig.animationDuration);

            mySequence.Append(moveTween);
            mySequence.Join(scaleTween);
            mySequence.OnComplete(() => Object.Destroy(xpGameObject));
        }
    }
}