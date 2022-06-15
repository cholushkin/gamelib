using GameLib;
using UnityEngine.Assertions;

public class Chooser<T>
{
    private T[] _items;
    private int _cyclesCount;
    private int _maxValueAmount;
    private CyclerBase _cycler;

    private int _cyclesRemaining;
    private int _valuesRemaining;


    public Chooser(T[] items, CyclerType cyclerType, long seed, int cyclesCount = -1, int maxValueAmount = -1)
    {
        _items = items;
        _cyclesCount = cyclesCount;
        _maxValueAmount = maxValueAmount;
        _cyclesRemaining = _cyclesCount;
        _valuesRemaining = _maxValueAmount;
        _cycler = CyclerFactory.CreateCycler(cyclerType, seed, items.Length);
    }

    public T GetCurrent()
    {
        if (_items.Length == 0)
            return default(T);
        if (_valuesRemaining < 1 && _maxValueAmount != -1)
            return default(T);
        if (_cyclesRemaining < 1 && _cyclesCount != -1) // ending of cycles for non-infinite cycler
            return default(T);
        return _items[_cycler.Now()];
    }

    // todo: IsCycleEnded(); // is it currently on the last step

    public void Step()
    {
        if (_items.Length == 0)
            return;
        if (_cyclesRemaining < 1 && _cyclesCount != -1) // ending of cycles for non-infinite cycler
            return;
        if (_cycler.IsCycleEnded() && _cyclesCount > 0)
            --_cyclesRemaining;
        if (_valuesRemaining < 1 && _maxValueAmount != -1)
            return;
        if (_maxValueAmount != -1)
            --_valuesRemaining;
        _cycler.Step();
    }

    public void Reset()
    {
        _cycler.Reset();
        _cyclesRemaining = _cyclesCount;
        _valuesRemaining = _maxValueAmount;
    }
}
