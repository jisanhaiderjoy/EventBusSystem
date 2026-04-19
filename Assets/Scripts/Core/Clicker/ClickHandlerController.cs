using System;
using Core.Events;
using DG.Tweening;
using VContainer.Unity;

namespace UnityEngine
{
    public class ClickHandlerController : IStartable, IDisposable
    {
        private readonly ClickerView _view;
        private readonly ClickerConfig _clickerConfig;
        
        private readonly IEventBus _eventBus;

        private EventSubscription _clickSubscription;

        public ClickHandlerController(ClickerView view, ClickerConfig clickerConfig, IEventBus eventBus)
        {
            _view = view;
            _clickerConfig = clickerConfig;
            _eventBus = eventBus;
        }

        public void Start()
        {
            _clickSubscription = _eventBus.Subscribe<PlayerClickEvent>(OnPlayerClickEvent);
        }

        public void Dispose()
        {
            _clickSubscription.Dispose();
        }

        private void OnPlayerClickEvent(PlayerClickEvent evt)
        {
            var xpGameobject = GameObject.Instantiate(_view.xpPrefab, _view.panel.transform);
            var xpTransform = xpGameobject.transform;
            
            Sequence mySequence = DOTween.Sequence();

            var moveDirection = (_clickerConfig.moveDirection * _clickerConfig.moveValue);
            var moveTween = xpTransform.DOLocalMove(xpTransform.localPosition + moveDirection, _clickerConfig.animationDuration);
            var scaleTween = xpTransform.DOScale(_clickerConfig.finalScale, _clickerConfig.animationDuration);
            
            mySequence.Append(moveTween);
            mySequence.Join(scaleTween);
            mySequence.OnComplete(() =>
            {
                Object.Destroy(xpGameobject);
            });
        }
    }
}