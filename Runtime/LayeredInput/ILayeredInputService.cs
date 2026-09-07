// todo: add a method to query the currently active highest-priority block for debugging
// idea: allow passing a reason string when requesting a block to improve inspector logging

using R3;

public interface ILayeredInputService
{
    /// Returns the reactive state of a specific input layer.
    ReadOnlyReactiveProperty<bool> GetLayerEnabled(string layerName);
    
    /// Request a block on all layers below a specific priority level.
    void SetMinimumActiveLayer(string layerName, object requester);
    
    /// Release a previously requested block.
    void ReleaseLayerBlock(object requester);
}