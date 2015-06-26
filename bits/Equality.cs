// Equality.cs
//
// How does IEquatable work?

using System;
using System.Collections.Generic;

struct TestStructDefault
{
    public TestStructDefault(int a)
    {
        A = a;
    }

    public int A;
}

struct TestStructCustomOverride
{
    public TestStructCustomOverride(int a)
    {
        A = a;
    }

    public int A;

    override public bool Equals(Object other)
    {
        Console.WriteLine("TestStructCustomOverride::Equals(Object other) called.");
        if ( other is TestStructCustomOverride )
        {
            var rhs = (TestStructCustomOverride)other;
            return rhs.A == this.A;
        }
        return false;
        //TestStructCustomOverride rhs = other as TestStructCustomOverride;
        //return other != null && rhs.A == this.A;
    }

    override public int GetHashCode()
    {
        return A;
    }
}

struct TestStructCustomInterface : IEquatable<TestStructCustomInterface>
{
    public TestStructCustomInterface(int a)
    {
        A = a;
    }

    public int A;

    public bool Equals(TestStructCustomInterface other)
    {
        Console.WriteLine("TestStructCustomInterface::Equals(TestStructCustomInterface other) called.");
        return this.A == other.A;
    }
}

static class Program
{
    static bool MyEquals<T>(T x, T y)
    {
        return EqualityComparer<T>.Default.Equals(x, y);
    }

    static void TestOne()
    {
        Console.WriteLine("\nTestStructDefault test.");
        TestStructDefault x = new TestStructDefault(1);
        TestStructDefault y = new TestStructDefault(2);
        TestStructDefault z = new TestStructDefault(1);
        Console.WriteLine("x == y : {0}", MyEquals(x, y));
        Console.WriteLine("x == z : {0}", MyEquals(x, z));
        Object objx = x, objy = y, objz=z;
        Console.WriteLine("x == y : {0}", MyEquals(x, objy));
        Console.WriteLine("x == z : {0}", MyEquals(x, objz));
        Console.WriteLine("x == y : {0}", MyEquals(objx, y));
        Console.WriteLine("x == z : {0}", MyEquals(objx, z));
        Console.WriteLine("x == y : {0}", MyEquals(objx, objy));
        Console.WriteLine("x == z : {0}", MyEquals(objx, objz));
    }

    static void TestTwo()
    {
        Console.WriteLine("\nTestStructCustomOverride test.");
        TestStructCustomOverride x = new TestStructCustomOverride(1);
        TestStructCustomOverride y = new TestStructCustomOverride(2);
        TestStructCustomOverride z = new TestStructCustomOverride(1);
        Console.WriteLine("x == y : {0}", MyEquals(x, y));
        Console.WriteLine("x == z : {0}", MyEquals(x, z));
        // This should all the overload, as here the overload is implemented using
        // polymorphism.
        Object objx = x, objy = y, objz = z;
        Console.WriteLine("x == y : {0}", MyEquals(x, objy));
        Console.WriteLine("x == z : {0}", MyEquals(x, objz));
        Console.WriteLine("x == y : {0}", MyEquals(objx, y));
        Console.WriteLine("x == z : {0}", MyEquals(objx, z));
        Console.WriteLine("x == y : {0}", MyEquals(objx, objy));
        Console.WriteLine("x == z : {0}", MyEquals(objx, objz));
    }

    static void TestThree()
    {
        Console.WriteLine("\nTestStructCustomInterface test.");
        var x = new TestStructCustomInterface(1);
        var y = new TestStructCustomInterface(2);
        var z = new TestStructCustomInterface(1);
        Console.WriteLine("x == y : {0}", MyEquals(x, y));
        Console.WriteLine("x == z : {0}", MyEquals(x, z));
        // We don't expect these to call our overload, as here the overload is generic,
        // and so after a cast to Object we lose knowledge of the original type.
        Object objx = x, objy = y, objz = z;
        Console.WriteLine("x == y : {0}", MyEquals(x, objy));
        Console.WriteLine("x == z : {0}", MyEquals(x, objz));
        Console.WriteLine("x == y : {0}", MyEquals(objx, y));
        Console.WriteLine("x == z : {0}", MyEquals(objx, z));
        Console.WriteLine("x == y : {0}", MyEquals(objx, objy));
        Console.WriteLine("x == z : {0}", MyEquals(objx, objz));
    }

    static void Main()
    {
        TestOne();
        TestTwo();
        TestThree();
    }
}