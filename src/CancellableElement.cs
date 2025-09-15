using Microsoft.AspNetCore.Components;

namespace Soenneker.Quark.Components.Cancellable;

public abstract class CancellableElement : CancellableComponent
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}