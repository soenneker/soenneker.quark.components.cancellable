using Microsoft.AspNetCore.Components;
using Soenneker.Quark.Components.Abstract;

namespace Soenneker.Quark.Components.Cancellable.Abstract;

public interface ICancellableElement : IElement
{
    RenderFragment? ChildContent { get; set; }
}