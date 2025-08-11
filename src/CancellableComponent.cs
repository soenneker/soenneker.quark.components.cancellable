using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Soenneker.Extensions.Task;

namespace Soenneker.Quark.Components.Cancellable;

/// <summary>
/// Base class that provides a per-component CancellationTokenSource
/// and implements IAsyncDisposable so work is cancelled when the
/// component is torn down.
/// </summary>
public abstract class CancellableComponent : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource? _cts;

    protected CancellationToken CancellationToken => (_cts ??= new CancellationTokenSource()).Token;

    /// <summary>
    /// Cancel any in-flight work (no-op if nothing started).
    /// </summary>
    protected Task Cancel()
    {
        CancellationTokenSource? cts = _cts;
        return cts is null ? Task.CompletedTask : cts.CancelAsync();
    }

    /// <summary>
    /// Cancel current work and swap in a fresh CTS for new work.
    /// </summary>
    protected async ValueTask ResetCancellation()
    {
        CancellationTokenSource? old = Interlocked.Exchange(ref _cts, new CancellationTokenSource());

        if (old is null)
            return;

        try
        {
            await old.CancelAsync().NoSync();
        }
        catch
        {
            /* ignore */
        }

        old.Dispose();
    }

    /// <summary>
    /// Uses CancelAsync() and only allocates when there’s a CTS to tear down.
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        CancellationTokenSource? cts = Interlocked.Exchange(ref _cts, null);

        if (cts is null)
            return;

        try
        {
            await cts.CancelAsync().ConfigureAwait(false);
        }
        catch
        {
            /* ignore */
        }

        cts.Dispose();
    }
}