// Playing with C#...
// Need to import the assembly System.Numerics, so compile as
// csc Fibo.cs /r:System.Numerics.dll
//
// Sort of curious to me that this doesn't get optimised away (I suppose it allocates objects, which
//  is a side-effect).

using System;
using System.Numerics;
using System.Diagnostics;

/// <summary>
/// Simple timer class
/// </summary>
public class TimeLoops
{
    /// <summary>
    /// Delegate class, simply a parameterless void returning function, typically a Lambda
    /// </summary>
    public delegate void TimeFunc();

    /// <summary>
    /// Time a number of loops of a function.  Call as TimeIt(loops, () => RealFunc(realParams));
    /// </summary>
    /// <param name="loops">Number of loops to time over</param>
    /// <param name="func">TimeFunc() function to time</param>
    /// <returns>double which is time in seconds for one loop (the average)</returns>
    public static double TimeIt(uint loops,TimeFunc func)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        for (uint i = 0; i < loops; ++i)
        {
            func();
        }
        stopWatch.Stop();
        return (double)stopWatch.ElapsedMilliseconds / (double)loops / 1000.0;
    }
}

class Fibonnaci
{
    static void Main()
    {
        Console.WriteLine("94 (native) : {0}", TimeLoops.TimeIt(10000000, () => FiboLong(94)));
        Console.WriteLine("94 : {0}", TimeLoops.TimeIt(100000, () => Fibo(94)));
        Console.WriteLine("94 : {0}", TimeLoops.TimeIt(100000, () => Fibo(94)));
        Console.WriteLine("187 : {0}", TimeLoops.TimeIt(100000, () => Fibo(187)));
        Console.WriteLine("187 : {0}", TimeLoops.TimeIt(100000, () => Fibo(187)));
        Console.WriteLine("1000 : {0}", TimeLoops.TimeIt(10000, () => Fibo(1000)));
        Console.WriteLine("10000 : {0}", TimeLoops.TimeIt(500, () => Fibo(10000)));
        Console.WriteLine("100000 : {0}", TimeLoops.TimeIt(5, () => Fibo(100000)));
        Console.WriteLine("1000000 : {0}", TimeLoops.TimeIt(1, () => Fibo(1000000)));
    }

    /// <summary>
    /// Compute Fibonacci Numbers, using 64-bit numbers, overflows at n=95
    /// </summary>
    /// <param name="n">Which number</param>
    /// <returns>The n'th number as a ulong</returns>
    static ulong FiboLong(int n)
    {
        ulong a = 0, b = 1, c;
        for (int i = 0; i < n - 2; ++i)
        {
            a += b;
            c = a; a = b; b = c;
        }
        return b;
    }
   
    /// <summary>
    /// Compute Fibonnaci Numbers
    /// </summary>
    /// <param name="n">Which number to find</param>
    /// <returns>The n'th number as a BigInteger</returns>
    static BigInteger Fibo(int n)
    {
        BigInteger a = 0;
        BigInteger b = 1;
        BigInteger tmp;

        for (int i = 0; i < n - 2; ++i)
        {
            //a += b;
            //tmp = a; a = b; b = tmp;
            tmp = a + b;
            a = b;
            b = tmp;
        }

        return b;
    }
}