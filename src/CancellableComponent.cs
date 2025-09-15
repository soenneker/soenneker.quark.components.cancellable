using System.Threading;
using System.Threading.Tasks;
using Soenneker.Quark.Components.Cancellable.Abstract;
using Soenneker.Utils.AtomicResources;

namespace Soenneker.Quark.Components.Cancellable;

///<inheritdoc cref="ICancellableComponent"/>
public abstract class CancellableComponent : Component, ICancellableComponent
{
    public CancellationToken CancellationToken =>
        Disposed || AsyncDisposed
            ? CancellationToken.None
            : _cancellationTokenSource.TryGet()
                ?.Token ?? CancellationToken.None;

    private readonly AtomicResource<CancellationTokenSource> _cancellationTokenSource;

    protected CancellableComponent() : this(CancellationToken.None)
    {
    }

    /// <summary>
    /// Optionally link to an external token so parent cancellation flows into this component.
    /// </summary>
    protected CancellableComponent(CancellationToken linkedToken)
    {
        _cancellationTokenSource = new AtomicResource<CancellationTokenSource>(
            factory: () => linkedToken.CanBeCanceled ? CancellationTokenSource.CreateLinkedTokenSource(linkedToken) : new CancellationTokenSource(),
            teardown: async cts =>
            {
                try
                {
                    await cts.CancelAsync();
                }
                catch
                {
                    /* ignore */
                }

                cts.Dispose();
            });
    }

    public Task Cancel()
    {
        CancellationTokenSource? cts = _cancellationTokenSource.TryGet();
        return cts is null ? Task.CompletedTask : cts.CancelAsync();
    }

    public ValueTask ResetCancellation() => _cancellationTokenSource.Reset();

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
            await _cancellationTokenSource.DisposeAsync();

        await base.DisposeAsync(disposing);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _cancellationTokenSource.Dispose();

        base.Dispose(disposing);
    }
}