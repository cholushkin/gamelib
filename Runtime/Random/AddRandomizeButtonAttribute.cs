using UnityEngine;

namespace GameLib.Random
{
    /// Draws a small "Randomize" button next to uint RNG state fields.
    /// Clicking it assigns a new non-trivial random state.
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class AddRandomizeButtonAttribute : PropertyAttribute
    {
        public AddRandomizeButtonAttribute() { }
    }
}