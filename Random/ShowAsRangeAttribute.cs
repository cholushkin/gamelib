using UnityEngine;

namespace GameLib.Random
{
    /// Can be applied to float2/int2 fields to show as a range in the inspector.
    /// Lives in runtime assembly so scripts can reference it.
    public class ShowAsRangeAttribute : PropertyAttribute
    {
    }
}