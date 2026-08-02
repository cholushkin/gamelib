using GameLib;
using GameLib.Random;

public class Chooser<T>
{
    private readonly T[] _items; // Array of items to choose from
    private readonly int _cyclesCount; // Total number of cycles (-1 for infinite cycles)
    private readonly int _maxValueAmount; // Maximum number of values to select (-1 for no limit)
    private readonly CyclerBase _cycler; // Cycler to handle item selection

    private int _cyclesRemaining; // Tracks remaining cycles
    private int _valuesRemaining; // Tracks remaining values to select

    /// <summary>
    /// Creates a new Chooser instance to select items from a list based on cycler behavior.
    /// </summary>
    /// <param name="items">The items to choose from.</param>
    /// <param name="cyclerType">The type of cycler to use for selection.</param>
    /// <param name="seed">The seed for random-based cyclers.</param>
    /// <param name="cyclesCount">The total number of cycles (-1 for infinite).</param>
    /// <param name="maxValueAmount">The maximum number of values to select (-1 for no limit).</param>
    public Chooser(T[] items, CyclerType cyclerType, Random rng, int cyclesCount = -1, int maxValueAmount = -1)
    {
        _items = items;
        _cyclesCount = cyclesCount;
        _maxValueAmount = maxValueAmount;
        _cyclesRemaining = _cyclesCount;
        _valuesRemaining = _maxValueAmount;
        _cycler = CyclerFactory.CreateCycler(cyclerType, rng, items.Length);
    }

    /// <summary>
    /// Returns the currently selected item without advancing the cycler.
    /// </summary>
    /// <returns>The currently selected item, or the default value if no items are available.</returns>
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
    /// Advances the cycler to the next item.
    /// </summary>
    public void Step()
    {
        if (_items.Length == 0)
            return;

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

    /// <summary>
    /// Checks if the cycler is currently on the last step of the current cycle.
    /// </summary>
    /// <returns>True if the cycler is on the last step; otherwise, false.</returns>
    public bool IsCycleEnded()
    {
        return _cycler.IsCycleEnded();
    }
}