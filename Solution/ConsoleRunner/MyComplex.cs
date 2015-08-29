using System.Diagnostics;

namespace ConsoleRunner
{
    [DebuggerDisplay("{ToString()}")]
    public sealed class MyComplex
    {
        /// <summary>
        /// The real component of the number.
        /// </summary>
        public readonly double Real;

        /// <summary>
        /// The imaginary component of the number.
        /// </summary>
        public readonly double Imaginary;

        public MyComplex()
        {
        }

        public MyComplex(double real, double imaginary)
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

        public static MyComplex operator +(MyComplex c1, MyComplex c2)
        {
            return new MyComplex(c1.Real + c2.Real, c1.Imaginary + c2.Imaginary);
        }

        public static MyComplex operator -(MyComplex c1, MyComplex c2)
        {
            return new MyComplex(c1.Real - c2.Real, c1.Imaginary - c2.Imaginary);
        }

        public static MyComplex operator *(MyComplex c1, MyComplex c2)
        {
            return new MyComplex(
                        real: c1.Real * c2.Real - c1.Imaginary * c2.Imaginary,
                        imaginary: c1.Imaginary * c2.Real + c1.Real * c2.Imaginary
            );
        }
    }
}