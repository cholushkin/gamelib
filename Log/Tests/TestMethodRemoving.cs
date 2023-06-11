using GameLib.Log;
using UnityEngine;

public class TestMethodRemoving : MonoBehaviour
{
    private LogChecker log = new LogChecker(LogChecker.Level.Normal);

    void Start()
    {
        log.Print(LogChecker.Level.Verbose, $"calculations {HeavyCalculationMethod(1, 1)}");
        log.Print(LogChecker.Level.Verbose, () => $"calculations {HeavyCalculationMethod(1, 1)}");
    }

    public int HeavyCalculationMethod(int a, int b)
    {
        print("Processor is on fire!");
        return a + b;
    }
}