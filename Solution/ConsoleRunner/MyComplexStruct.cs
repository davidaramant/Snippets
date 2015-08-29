using System.Diagnostics;

namespace ConsoleRunner
{
    [DebuggerDisplay("{ToString()}")]
    public struct MyComplexStruct
    {
        /// <summary>
        /// The real component of the number.
        /// </summary>
        public readonly double Real;

        /// <summary>
        /// The imaginary component of the number.
        /// </summary>
        public readonly double Imaginary;

        public MyComplexStruct(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public override string ToString() => $"({Real}, {Imaginary}i)";

        /// <summary>
        /// Returns the magnitude squared.
        /// </summary>
        public double MagnitudeSquared()
        {
            return Real * Real + Imaginary * Imaginary;
        }

        public static MyComplexStruct operator +(MyComplexStruct c1, MyComplexStruct c2)
        {
            return new MyComplexStruct(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary);
        }

        public static MyComplexStruct operator -(MyComplexStruct c1, MyComplexStruct c2)
        {
            return new MyComplexStruct(c1.Real - c2.Real, c1.Imaginary - c2.Imaginary);
        }

        public static MyComplexStruct operator *(MyComplexStruct c1, MyComplexStruct c2)
        {
            return new MyComplexStruct(
                        real: c1.Real * c2.Real - c1.Imaginary * c2.Imaginary,
                        imaginary: c1.Imaginary * c2.Real + c1.Real * c2.Imaginary
            );
        }
    }
}