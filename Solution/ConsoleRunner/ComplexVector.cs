using System;
using System.Numerics;

namespace ConsoleRunner
{
    internal struct ComplexVector
    {
        public ComplexVector(Vector<double> real, Vector<double> imaginary)
        {
            Real = real; Imaginary = imaginary;
        }

        public Vector<double> Real;
        public Vector<double> Imaginary;

        public ComplexVector Square()
        {
            return new ComplexVector(Real * Real - Imaginary * Imaginary, Real * Imaginary + Real * Imaginary);
        }

        public Vector<double> Sqabs()
        {
            return Real * Real + Imaginary * Imaginary;
        }

        public override string ToString()
        {
            return $"({Real}, {Imaginary}i)";
        }

        public static ComplexVector operator +(ComplexVector a, ComplexVector b)
        {
            return new ComplexVector(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }
    }
}