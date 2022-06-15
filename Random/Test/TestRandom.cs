using System.Collections.Generic;
using GameLib.Random;
using UnityEngine;


// todo: sandbox case
public class TestRandom : MonoBehaviour
{
    private Dictionary<int, int> distribution;

    void Start()
    {
        TestValueInt();
    }

    // params
    const int iterations = 300;
    const int maxValue = 8;


    void TestValueInt()
    {
        distribution = new Dictionary<int, int>();

        

        for (int i = 0; i < iterations; ++i)
        {
            IPseudoRandomNumberGenerator rnd = RandomHelper.CreateRandomNumberGenerator(i, RandomHelper.PseudoRandomNumberGenerator.LinearCongruential);
            CountDistribution(rnd.ValueInt(maxValue));
        }

        PrintDistribution();
    }

    void PrintDistribution()
    {
        foreach (var kv in distribution)
            Debug.LogFormat("Value {0} appeared {1} times ({2}%)", kv.Key, kv.Value, kv.Value/ (double)iterations * 100f);
    }

    void CountDistribution(int value)
    {
        if(!distribution.ContainsKey(value))
            distribution.Add(value, 0);
        distribution[value]++;
    }
}
