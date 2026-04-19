using Core.Disposables;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Services
{
    public class MainSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private TestPopupView _testPopupView;
        
        [Space]
        [SerializeField] private ClickerView _clickerView;
        [SerializeField] private ClickerConfig _clickerConfig;
        
        protected override LifetimeScope FindParent()
        {
            return VContainerSettings.Instance?.GetOrCreateRootLifetimeScopeInstance();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<Disposer>(Lifetime.Scoped);

            builder.RegisterEntryPoint<TestPopupController>(Lifetime.Scoped)
                .WithParameter(_testPopupView);

            builder.RegisterInstance(_clickerConfig);
            builder.RegisterEntryPoint<ClickHandlerController>(Lifetime.Scoped)
                .WithParameter(_clickerView);
        }
    }
}
