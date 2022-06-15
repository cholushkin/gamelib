using System;
using UnityEngine;

namespace GameLib.Random
{
    [Serializable]
    public class Range
    {
        public static readonly Range Zero = new Range(0f, 0f);
        public static readonly Range One = new Range(1f, 1f);
        public static readonly Range PositiveInfinity = new Range(float.PositiveInfinity, float.PositiveInfinity);
        public static readonly Range NegativeInfinity = new Range(float.NegativeInfinity, float.NegativeInfinity);
        public static readonly Range InfiniteRange = new Range(float.NegativeInfinity, float.PositiveInfinity);

        public float From;
        public float To;


        public Range(float from, float to)
        {
            From = from;
            To = to;
        }

        public virtual bool IsIn(float val)
        {
            if (val < From)
                return false;
            if (val > To)
                return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("<{0} : {1}>", From, To);
        }

        public bool IsZero()
        {
            return Mathf.Approximately(From, 0f) && Mathf.Approximately(To, 0f);
        }
    }

    [Serializable]
    public class RangeInt
    {
        public static readonly RangeInt Zero = new RangeInt(0, 0);
        public static readonly RangeInt One = new RangeInt(1, 1);
        public static readonly RangeInt PositiveInfinity = new RangeInt(int.MaxValue, int.MaxValue);
        public static readonly RangeInt NegativeInfinity = new RangeInt(int.MinValue, int.MinValue);
        public static readonly RangeInt InfiniteRange = new RangeInt(int.MinValue, int.MaxValue);

        public int From;
        public int To;

        public RangeInt(int from, int to)
        {
            From = from;
            To = to;
        }

        public virtual bool IsIn(int val)
        {
            if (val < From)
                return false;
            if (val > To)
                return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("<{0} : {1}>", From, To);
        }

        public bool IsZero()
        {
            return From == 0 && To == 0;
        }
    }

    [Serializable]
    public class StrictRange : Range
    {
        public enum Restriction // all range types: (), [], [), (]
        {
            Included, // [
            Excluded // (
        }

        public Restriction FromRestriction;
        public Restriction ToRestriction;

        public StrictRange(float from, float to, Restriction fromRestriction = Restriction.Included, Restriction toRestriction = Restriction.Excluded) : base(from, to)
        {
            FromRestriction = fromRestriction;
            ToRestriction = toRestriction;
        }

        public override bool IsIn(float val)
        {
            if (FromRestriction == Restriction.Excluded ? val <= From : val < From)
                return false;
            if (ToRestriction == Restriction.Excluded ? val >= To : val > To)
                return false;
            return true;
        }

        public override string ToString()
        {
            return
                $"{(FromRestriction == Restriction.Included ? "[" : "(")}{From} : {To}{(ToRestriction == Restriction.Included ? "]" : ")")}";
        }
    }

    [Serializable]
    public class StrictRangeInt : RangeInt
    {
        public enum Restriction // all range types: (), [], [), (]
        {
            Included, // [
            Excluded // (
        }

        public Restriction FromRestriction;
        public Restriction ToRestriction;

        public StrictRangeInt(int from, int to, Restriction fromRestriction = Restriction.Included, Restriction toRestriction = Restriction.Excluded) : base(from, to)
        {
            FromRestriction = fromRestriction;
            ToRestriction = toRestriction;
        }

        public override bool IsIn(int val)
        {
            if (FromRestriction == Restriction.Excluded ? val <= From : val < From)
                return false;
            if (ToRestriction == Restriction.Excluded ? val >= To : val > To)
                return false;
            return true;
        }

        public override string ToString()
        {
            return
                $"{(FromRestriction == Restriction.Included ? "[" : "(")}{From} : {To}{(ToRestriction == Restriction.Included ? "]" : ")")}";
        }
    }
}