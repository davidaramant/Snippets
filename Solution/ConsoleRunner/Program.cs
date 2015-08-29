using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Numerics;
using System.Runtime.Remoting.Messaging;

namespace ConsoleRunner
{
    sealed class Program
    {
        public static void RunSnippet(string[] args)
        {
            var t1 = BitConverter.ToDouble(new byte[] { 0x3B, 0x68, 0x60, 0x74, 0xcc, 0x46, 0xe4, 0xbf }, 0);
            var t2 = BitConverter.ToDouble(new byte[] { 0xb9, 0xe2, 0xaa, 0x1d, 0xd7, 0x09, 0xd8, 0x3f }, 0);

            const double cr1 = -0.25d;
            const double ci1 = 0.01d;

            const double cr2 = -0.24d;
            const double ci2 = 0.011d;

            //var inputs = new[]
            //{
            //    new Complex(-0.25d,0.01d),
            //    new Complex(-0.24,0.11d),
            //};
            var inputs = new[]
            {
                new Complex(t1,t2),
                new Complex(-0.24,0.11d),
            };

            const int minBailout = 1 * 1000 * 1000;
            const int maxBailout = 5 * 1000 * 1000;

            bool[] results;

            using (Timed.Run("Raw Doubles Array"))
            {
                results = BuddhaPointsRawDoubleArray(inputs, minBailout, maxBailout);
            }
            WL("Results: " + String.Join(", ", results.Select(r => r.ToString())));

            using (Timed.Run("Orbit Checking Array"))
            {
                results = BuddhaPointsRawDoubleOrbitCheckingArray(inputs, minBailout, maxBailout);
            }
            WL("Results: " + String.Join(", ", results.Select(r => r.ToString())));

            using (Timed.Run("Raw Doubles"))
            {
                for (int i = 0; i < 2; i++)
                {
                    results[i] = BuddhaPointsRawDouble(inputs[0], minBailout, maxBailout);
                }
            }
            WL("Results: " + String.Join(", ", results.Select(r => r.ToString())));

            using (Timed.Run("Orbit Checking"))
            {
                for (int i = 0; i < 2; i++)
                {
                    results[i] = BuddhaPointsRawDoubleOrbitChecking(inputs[i], minBailout, maxBailout);
                }
            }
            WL("Results: " + String.Join(", ", results.Select(r => r.ToString())));


            //using (Timed.Run("Vector<double>"))
            //{
            //    results = BuddhaPointsVectorDouble(inputs, minBailout, maxBailout);
            //}
            //WL("Results: " + String.Join(", ", results.Select(r => r.ToString())));

            return;
            using (Timed.Run("Raw Doubles"))
            {
                double zr1 = 0;
                double zi1 = 0;

                double zr2 = 0;
                double zi2 = 0;

                for (int i = 0; i < maxBailout; i++)
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

            using (Timed.Run("Vector<double>"))
            {
                var cr = new Vector<double>(new[] { cr1, cr2 });
                var ci = new Vector<double>(new[] { ci1, ci2 });

                var zr = new Vector<double>();
                var zi = new Vector<double>();

                for (int i = 0; i < maxBailout; i++)
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

        private static bool BuddhaPointsRawDouble(Complex input, int minBailout, int maxBailout)
        {
            double cr = input.Real;
            double ci = input.Imaginary;

            double zr = 0;
            double zi = 0;

            double zr2 = 0;
            double zi2 = 0;

            for (int i = 0; i < maxBailout; i++)
            {
                var tempZr = zr2 - zi2 + cr;
                zi = 2 * zr * zi + ci;
                zr = tempZr;

                zr2 = zr * zr;
                zi2 = zi * zi;

                if ((zr2 + zi2) > 4)
                {
                    return i >= minBailout;
                }
            }
            return false;
        }

        private static bool[] BuddhaPointsRawDoubleArray(Complex[] inputs, int minBailout, int maxBailout)
        {
            var isBuddha = new bool[inputs.Length];

            for (int index = 0; index < inputs.Length; index++)
            {
                double cr = inputs[index].Real;
                double ci = inputs[index].Imaginary;

                double zr = 0;
                double zi = 0;

                double zr2 = 0;
                double zi2 = 0;

                for (int i = 0; i < maxBailout; i++)
                {
                    var tempZr = zr2 - zi2 + cr;
                    zi = 2 * zr * zi + ci;
                    zr = tempZr;

                    zr2 = zr * zr;
                    zi2 = zi * zi;

                    if ((zr2 + zi2) > 4)
                    {
                        isBuddha[index] = i >= minBailout;
                        break;
                    }
                }
            }
            return isBuddha;
        }

        private static bool BuddhaPointsRawDoubleOrbitChecking(Complex input, int minBailout, int maxBailout)
        {
            double cr = input.Real;
            double ci = input.Imaginary;

            double zr = 0;
            double zi = 0;

            double zr2 = 0;
            double zi2 = 0;

            // Check for orbits
            // - Check re/im against an old point
            // - Only check every power of 2
            double oldzr = 0;
            double oldzi = 0;

            uint checkNum = 1;

            for (int i = 0; i < maxBailout; i++)
            {
                var tempZr = zr2 - zi2 + cr;
                zi = 2 * zr * zi + ci;
                zr = tempZr;

                // Orbit check
                if (checkNum == i)
                {
                    if (zr == oldzr && zi == oldzi)
                    {
                        return false;
                    }

                    oldzr = zr;
                    oldzi = zi;

                    checkNum = checkNum << 1;
                }


                zr2 = zr * zr;
                zi2 = zi * zi;

                if ((zr2 + zi2) > 4)
                {
                    return i >= minBailout;
                }
            }

            return false;
        }


        private static bool[] BuddhaPointsRawDoubleOrbitCheckingArray(Complex[] inputs, int minBailout, int maxBailout)
        {
            var isBuddha = new bool[inputs.Length];

            for (int index = 0; index < inputs.Length; index++)
            {
                double cr = inputs[index].Real;
                double ci = inputs[index].Imaginary;

                double zr = 0;
                double zi = 0;

                double zr2 = 0;
                double zi2 = 0;

                // Check for orbits
                // - Check re/im against an old point
                // - Only check every power of 2
                double oldzr = 0;
                double oldzi = 0;

                uint checkNum = 1;

                for (int i = 0; i < maxBailout; i++)
                {
                    var tempZr = zr2 - zi2 + cr;
                    zi = 2 * zr * zi + ci;
                    zr = tempZr;

                    // Orbit check
                    if (checkNum == i)
                    {
                        if (zr == oldzr && zi == oldzi)
                        {
                            isBuddha[index] = false;
                            break;
                        }

                        oldzr = zr;
                        oldzi = zi;

                        checkNum = checkNum << 1;
                    }


                    zr2 = zr * zr;
                    zi2 = zi * zi;

                    if ((zr2 + zi2) > 4)
                    {
                        isBuddha[index] = i >= minBailout;
                        break;
                    }
                }
            }
            return isBuddha;
        }

        private static bool[] BuddhaPointsDoubleUnrolled(Complex[] inputs, int minBailout, int maxBailout)
        {
            var isBuddha = new bool[inputs.Length];

            double cr1 = inputs[0].Real;
            double ci1 = inputs[0].Imaginary;

            double zr1 = 0;
            double zi1 = 0;

            double zr1_2 = 0;
            double zi1_2 = 0;

            double cr2 = inputs[1].Real;
            double ci2 = inputs[1].Imaginary;

            double zr2 = 0;
            double zi2 = 0;

            double zr2_2 = 0;
            double zi2_2 = 0;

            for (int i = 0; i < maxBailout; i++)
            {
                var tempZr1 = zr1_2 - zi1_2 + cr1;
                zi1 = 2 * zr1 * zi1 + ci1;
                zr1 = tempZr1;

                zr1_2 = zr1 * zr1;
                zi1_2 = zi1 * zi1;

                var tempZr2 = zr2_2 - zi2_2 + cr2;
                zi2 = 2 * zr2 * zi2 + ci2;
                zr2 = tempZr2;

                zr2_2 = zr2 * zr2;
                zi2_2 = zi2 * zi2;

                if ((zr1_2 + zi1_2) > 4)
                {
                    isBuddha[0] = i >= minBailout;
                }
                if ((zr2_2 + zi2_2) > 4)
                {
                    isBuddha[1] = i >= minBailout;
                }
            }
            return isBuddha;
        }

        private static bool[] BuddhaPointsVectorDouble(Complex[] inputs, int minBailout, int maxBailout)
        {
            var isBuddha = new bool[inputs.Length];

            var cr = new Vector<double>(new[] { inputs[0].Real, inputs[1].Real });
            var ci = new Vector<double>(new[] { inputs[0].Imaginary, inputs[1].Imaginary });

            var zr = new Vector<double>();
            var zi = new Vector<double>();

            var zr2 = Vector<double>.Zero;
            var zi2 = Vector<double>.Zero;

            var iterations = Vector<long>.Zero;
            var increment = Vector<long>.One;

            var vMinbailout = new Vector<long>(minBailout);
            var vMaxbailout = new Vector<long>(maxBailout);
            var vMagnitude = new Vector<double>(4);

            do
            {
                var tempZr = zr2 - zi2 + cr;
                zi = 2 * zr * zi + ci;
                zr = tempZr;

                iterations += increment;

                zr2 = zr * zr;
                zi2 = zi * zi;

                var magnitude = zr2 + zi2;

                var shouldContinue =
                    Vector.LessThanOrEqual(magnitude, vMagnitude) &
                    Vector.LessThanOrEqual(iterations, vMaxbailout);

                increment = increment & shouldContinue;
            } while (increment != Vector<long>.Zero);

            var wasBuddha = Vector.GreaterThanOrEqual(iterations, vMinbailout);

            isBuddha[0] = wasBuddha[0] == 1;
            isBuddha[1] = wasBuddha[1] == 1;

            return isBuddha;
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
                    WL("{0}ms", _watch.ElapsedMilliseconds);
                }
                else
                {
                    WL("{0}: {1}ms", _name, _watch.ElapsedMilliseconds);
                }
            }
        }

        #endregion
    }
}
