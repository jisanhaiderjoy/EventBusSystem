using Core.Disposables;
using Core.Events;
using VContainer;
using VContainer.Unity;

namespace Core.Services
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
            builder.Register<Disposer>(Lifetime.Scoped);
        }
    }
}