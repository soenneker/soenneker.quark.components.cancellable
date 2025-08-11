using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Soenneker.Extensions.Task;
using Soenneker.Quark.Components.Cancellable.Abstract;
using Soenneker.Utils.AtomicResources;

namespace Soenneker.Quark.Components.Cancellable;

///<inheritdoc cref="ICancellableComponent"/>
public abstract class CancellableComponent : ComponentBase, ICancellableComponent
{
    private readonly AtomicResource<CancellationTokenSource> _atomic;

    protected CancellableComponent() : this(CancellationToken.None)
    {
    }

    /// <summary>
    /// Optionally link to an external token so parent cancellation flows into this component.
    /// </summary>
    protected CancellableComponent(CancellationToken linkedToken)
    {
        _atomic = new AtomicResource<CancellationTokenSource>(
            factory: () => linkedToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(linkedToken) : new CancellationTokenSource(),
            teardown: async cts =>
            {
                try
                {
                    await cts.CancelAsync().NoSync();
                }
                catch
                {
                    /* ignore */
                }

                cts.Dispose();
            });
    }

    public CancellationToken CancellationToken => _atomic.GetOrCreate()?.Token ?? CancellationToken.None;

    public Task Cancel()
    {
        CancellationTokenSource? cts = _atomic.TryGet();
        return cts is null ? Task.CompletedTask : cts.CancelAsync();
    }

    public ValueTask ResetCancellation() => _atomic.Reset();

    public virtual ValueTask DisposeAsync() => _atomic.DisposeAsync();
}