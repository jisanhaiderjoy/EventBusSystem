using System;
using Core.Events;
using UnityEngine;
using VContainer.Unity;

public class TestPopupController : IStartable, IDisposable
{
    private readonly TestPopupView _view;
    private readonly IEventBus _eventBus;

    public TestPopupController(TestPopupView view, IEventBus eventBus)
    {
        _view = view;
        _eventBus = eventBus;
    }
    
    public void Start()
    {
        _view.button.onClick.AddListener(OnTestButtonClick);
    }

    public void Dispose()
    {
        if (_view != null)
        {
            _view.button.onClick.RemoveListener(OnTestButtonClick);
        }
    }

    private void OnTestButtonClick()
    {
        Debug.Log($"{nameof(TestPopupController)}: Event {nameof(PlayerClickEvent)} Fired");
        _eventBus.Publish(new PlayerClickEvent());
    }
}
