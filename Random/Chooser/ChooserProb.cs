using GameLib;
using GameLib.Random;
using UnityEngine.Assertions;

public class ChooserProb<T>
{
    private readonly T[] _items; // Array of items to choose from
    private readonly int _cyclesCount; // Total number of cycles (-1 for infinite)
    private readonly int _maxValueAmount; // Maximum number of values to select (-1 for no limit)
    private readonly CyclerBaseProb _cycler; // Cycler for probability-based selection

    private int _cyclesRemaining; // Tracks remaining cycles
    private int _valuesRemaining; // Tracks remaining values to select

    /// <summary>
    /// Creates a new probability-based chooser.
    /// </summary>
    /// <param name="items">The items to choose from.</param>
    /// <param name="probs">The probabilities associated with each item.</param>
    /// <param name="cyclerType">The type of cycler to use for selection.</param>
    /// <param name="cyclesCount">The total number of cycles (-1 for infinite).</param>
    /// <param name="maxValueAmount">The maximum number of values to select (-1 for no limit).</param>
    public ChooserProb(T[] items, float[] probs, CyclerProbType cyclerType, Random rng, int cyclesCount = -1, int maxValueAmount = -1)
    {
        Assert.IsNotNull(items, "Items array must not be null.");
        Assert.IsTrue(items.Length > 0, "Items array must contain at least one element.");
        Assert.IsNotNull(probs, "Probabilities array must not be null.");
        Assert.AreEqual(items.Length, probs.Length, "Items and probabilities arrays must have the same length.");

        _items = items;
        _cyclesCount = cyclesCount;
        _maxValueAmount = maxValueAmount;
        _cyclesRemaining = _cyclesCount;
        _valuesRemaining = _maxValueAmount;

        _cycler = CyclerProbFactory.CreateCyclerProb(cyclerType, probs, rng);
    }

    /// <summary>
    /// Returns the currently selected item without advancing the cycler.
    /// </summary>
    /// <returns>The currently selected item, or the default value if conditions are not met.</returns>
    public T GetCurrent()
    {
        if (_items.Length == 0)
            return default;

        if (_valuesRemaining < 1 && _maxValueAmount != -1)
            return default;

        if (_cyclesRemaining < 1 && _cyclesCount != -1)
            return default;

        return _items[_cycler.Now()];
    }

    /// <summary>
    /// Advances the cycler to the next item, respecting cycle and value limits.
    /// </summary>
    public void Step()
    {
        if (_cyclesRemaining < 1 && _cyclesCount != -1)
            return;

        if (_cycler.IsCycleEnded() && _cyclesCount > 0)
            --_cyclesRemaining;

        if (_valuesRemaining < 1 && _maxValueAmount != -1)
            return;

        if (_maxValueAmount != -1)
            --_valuesRemaining;

        _cycler.Step();
    }

    /// <summary>
    /// Resets the cycler to its initial state, including cycles and value limits.
    /// </summary>
    public void Reset()
    {
        _cycler.Reset();
        _cyclesRemaining = _cyclesCount;
        _valuesRemaining = _maxValueAmount;
    }
}
