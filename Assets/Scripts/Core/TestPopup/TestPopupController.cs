using Core.Disposables;
using Core.Events;
using UnityEngine;
using VContainer.Unity;

public class TestPopupController : IStartable
{
    private readonly TestPopupView _view;
    private readonly IEventBus _eventBus;
    private readonly Disposer _disposer;

    public TestPopupController(TestPopupView view, IEventBus eventBus, Disposer disposer)
    {
        _view = view;
        _eventBus = eventBus;
        _disposer = disposer;
    }

    public void Start()
    {
        _view.button.onClick.AddListener(OnTestButtonClick);
        new ActionDisposable(() => _view.button.onClick.RemoveListener(OnTestButtonClick))
            .AddTo(_disposer);
    }

    private void OnTestButtonClick()
    {
        Debug.Log($"{nameof(TestPopupController)}: Event {nameof(PlayerClickEvent)} Fired");
        _eventBus.Publish(new PlayerClickEvent());
    }
}
