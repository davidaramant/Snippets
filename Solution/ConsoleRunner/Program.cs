using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Numerics;

namespace ConsoleRunner
{
    sealed class Program
    {
        public static void RunSnippet(string[] args)
        {
            const double cr1 = -0.25d;
            const double ci1 = 0.01d;

            const double cr2 = -0.24d;
            const double ci2 = 0.011d;


            const int trials = 5 * 1000 * 1000;

            using (Timed.Run("Raw Doubles"))
            {
                double zr1 = 0;
                double zi1 = 0;

                double zr2 = 0;
                double zi2 = 0;

                for (int i = 0; i < trials; i++)
                {
                    var zr1_2 = zr1 * zr1;
                    var zi1_2 = zi1 * zi1;

                    var tempZr1 = zr1_2 - zi1_2 + cr1;
                    zi1 = 2 * zr1 * zi1 + ci1;
                    zr1 = tempZr1;

                    var zr2_2 = zr2 * zr2;
                    var zi2_2 = zi2 * zi2;

                    var tempZr2 = zr2_2 - zi2_2 + cr2;
                    zi2 = 2 * zr2 * zi2 + ci2;
                    zr2 = tempZr2;
                }

                WL($"({zr1}, {zi1}i)");
                WL($"({zr2}, {zi2}i)");
            }

            using (Timed.Run("MyComplex"))
            {
                var c1 = new MyComplex(cr1, ci1);
                var z1 = new MyComplex();

                var c2 = new MyComplex(cr2, ci2);
                var z2 = new MyComplex();

                for (int i = 0; i < trials; i++)
                {
                    z1 = z1 * z1 + c1;
                    z2 = z2 * z2 + c2;
                }

                WL(z1);
                WL(z2);
            }

            using (Timed.Run("MyComplexStruct"))
            {
                var c1 = new MyComplexStruct(cr1, ci1);
                var z1 = new MyComplexStruct();

                var c2 = new MyComplexStruct(cr2, ci2);
                var z2 = new MyComplexStruct();

                for (int i = 0; i < trials; i++)
                {
                    z1 = z1 * z1 + c1;
                    z2 = z2 * z2 + c2;
                }

                WL(z1);
                WL(z2);
            }

            using (Timed.Run("System.Numerics.Complex"))
            {
                var c1 = new Complex(cr1, ci1);
                var z1 = new Complex();

                var c2 = new Complex(cr2, ci2);
                var z2 = new Complex();

                for (int i = 0; i < trials; i++)
                {
                    z1 = z1 * z1 + c1;
                    z2 = z2 * z2 + c2;
                }

                WL(z1);
                WL(z2);
            }

            using (Timed.Run("ComplexVector"))
            {
                var c = new ComplexVector(
                    new Vector<double>(new[] { cr1, cr2 }),
                    new Vector<double>(new[] { ci1, ci2 }));

                var z = new ComplexVector();

                for (int i = 0; i < trials; i++)
                {
                    z = z.Square() + c;
                }

                WL(z);
            }

            using (Timed.Run("ComplexVector - Different math"))
            {
                var cr = new Vector<double>(new[] { cr1, cr2 });
                var ci = new Vector<double>(new[] { ci1, ci2 });

                var zr = new Vector<double>();
                var zi = new Vector<double>();

                for (int i = 0; i < trials; i++)
                {
                    var zr2 = zr * zr;
                    var zi2 = zi * zi;

                    var tempZr = zr2 - zi2 + cr;
                    zi = 2 * zr * zi + ci;
                    zr = tempZr;
                }

                WL($"({zr}, {zi}i)");
            }            
        }

        #region Helper methods

        private static string Desktop => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        public static void Main(string[] args)
        {
            try
            {
                RunSnippet(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("---\nEXCEPTION:\n" + e + "\n");
            }
            finally
            {
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void WL()
        {
            Console.WriteLine();
        }

        private static void WL(object text, params object[] args)
        {
            if (text == null)
            {
                Console.WriteLine("null");
            }
            else
            {
                Console.WriteLine(text.ToString(), args);
            }
        }

        private static void RL()
        {
            Console.ReadLine();
        }

        private static void Break()
        {
            System.Diagnostics.Debugger.Break();
        }

        sealed class Timed : IDisposable
        {
            readonly System.Diagnostics.Stopwatch _watch;
            readonly string _name = String.Empty;

            private Timed(string name)
            {
                GC.Collect(2);
                GC.WaitForPendingFinalizers();
                _name = name;
                _watch = System.Diagnostics.Stopwatch.StartNew();
            }

            public static IDisposable Run(string name = "")
            {
                return new Timed(name);
            }

            public void Dispose()
            {
                _watch.Stop();
                if (String.IsNullOrEmpty(_name))
                {
                    WL("{0}ms\n", _watch.ElapsedMilliseconds);
                }
                else
                {
                    WL("{0}: {1}ms\n", _name, _watch.ElapsedMilliseconds);
                }
            }
        }

        #endregion
    }
}
