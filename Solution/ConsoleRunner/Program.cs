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
        private sealed class ComplexBatch
        {
            public readonly float[] R;
            public readonly float[] I;

            public ComplexBatch()
            {
                R = new float[Vector<float>.Count];
                I = new float[Vector<float>.Count];
            }

            public ComplexBatch(float[] r, float[] i)
            {
                R = r;
                I = i;
            }
        }

        public static void RunSnippet(string[] args)
        {
            var t1 = (float)BitConverter.ToDouble(new byte[] { 0x3B, 0x68, 0x60, 0x74, 0xcc, 0x46, 0xe4, 0xbf }, 0);
            var t2 = (float)BitConverter.ToDouble(new byte[] { 0xb9, 0xe2, 0xaa, 0x1d, 0xd7, 0x09, 0xd8, 0x3f }, 0);

            const float cr1 = -0.25f;
            const float ci1 = 0.01f;

            const float cr2 = -0.24f;
            const float ci2 = 0.011f;

            const float cr3 = 1f;
            const float ci3 = 2f;

            var inputs = new[]
            {
                new ComplexBatch(
                    r: new[] {t1, cr1, cr2, cr3},
                    i: new[] {t2, ci1, ci2, ci3}),
                new ComplexBatch(
                    r: new[] {t1, t1, t1, t1},
                    i: new[] {t2, t2, t2, t2})
            };

            WL($"Vector<float>Count: {Vector<float>.Count}");
            WL($"Vector<int>Count: {Vector<int>.Count}");
            WL();

            //var inputs = new[]
            //{
            //    new Complex(-0.25d,0.01d),
            //    new Complex(-0.24,0.11d),
            //};
            //var inputs = new[]
            //{
            //    new Complex(t1,t2),
            //    new Complex(-0.24,0.11d),
            //};

            const int minBailout = 1 * 1000 * 1000;
            const int maxBailout = 5 * 1000 * 1000;

            bool[][] results = new bool[inputs.Length][];
            int[][] iresults = new int[inputs.Length][];

            for (int i = 0; i < inputs.Length; i++)
            {
                results[i] = new bool[Vector<float>.Count];
            }


            using (Timed.Run("BuddhaPointsFloatSingleWithOrbitCheck"))
            {
                for (int trial = 0; trial < inputs.Length; trial++)
                {
                    for (int i = 0; i < inputs[trial].R.Length; i++)
                    {
                        results[trial][i] = BuddhaPointsFloatSingleWithOrbitCheck(inputs[trial].R[i], inputs[trial].I[i], minBailout, maxBailout);
                    }
                }
            }
            WL("Results: " + String.Join(", ", results.Select(r => "[" + String.Join(", ", r) + "]")));
            WL();

            using (Timed.Run("BuddhaPointsFloatArrayOrbitCheck"))
            {
                for (int trial = 0; trial < inputs.Length; trial++)
                {
                    results[trial] = BuddhaPointsFloatArrayOrbitCheck(inputs[trial], minBailout, maxBailout);
                }
            }
            WL("Results: " + String.Join(", ", results.Select(r => "[" + String.Join(", ", r) + "]")));
            WL();

            using (Timed.Run("BuddhaPointsFloatArrayOrbitCheckWithoutSquares"))
            {
                for (int trial = 0; trial < inputs.Length; trial++)
                {
                    results[trial] = BuddhaPointsFloatArrayOrbitCheckWithoutSquares(inputs[trial], minBailout, maxBailout);
                }
            }
            WL("Results: " + String.Join(", ", results.Select(r => "[" + String.Join(", ", r) + "]")));
            WL();

            using (Timed.Run("BuddhaPointsVectorFloat"))
            {
                for (int trial = 0; trial < inputs.Length; trial++)
                {
                    iresults[trial] = BuddhaPointsVectorFloat(inputs[trial], minBailout, maxBailout);
                }
            }
            WL("Results: " + String.Join(", ", iresults.Select(r => "[" + String.Join(", ", r.Select(r2 => r2 != 0)) + "]")));
            WL();

            using (Timed.Run("BuddhaPointsVectorFloatOrbitCheck"))
            {
                for (int trial = 0; trial < inputs.Length; trial++)
                {
                    iresults[trial] = BuddhaPointsVectorFloatOrbitCheck(inputs[trial], minBailout, maxBailout);
                }
            }
            WL("Results: " + String.Join(", ", iresults.Select(r => "[" + String.Join(", ", r.Select(r2 => r2 != 0)) + "]")));
            WL();

            using (Timed.Run("BuddhaPointsVectorFloatOrbitCheckWithoutSquares"))
            {
                for (int trial = 0; trial < inputs.Length; trial++)
                {
                    iresults[trial] = BuddhaPointsVectorFloatOrbitCheckWithoutSquares(inputs[trial], minBailout, maxBailout);
                }
            }
            WL("Results: " + String.Join(", ", iresults.Select(r => "[" + String.Join(", ", r.Select(r2 => r2 != 0)) + "]")));
            WL();
        }

        private static bool BuddhaPointsFloatSingleWithOrbitCheck(float cr, float ci, int minBailout, int maxBailout)
        {
            float zr = 0;
            float zi = 0;

            float zr2 = 0;
            float zi2 = 0;

            // Check for orbits
            // - Check re/im against an old point
            // - Only check every power of 2
            var oldzr = 0f;
            var oldzi = 0f;

            uint iterationToCheck = 1;

            for (int i = 0; i < maxBailout; i++)
            {
                var tempZr = zr2 - zi2 + cr;
                zi = 2 * zr * zi + ci;
                zr = tempZr;

                // Orbit check
                if (iterationToCheck == i)
                {
                    if (zr == oldzr && zi == oldzi)
                    {
                        return false;
                    }

                    oldzr = zr;
                    oldzi = zi;

                    iterationToCheck = iterationToCheck << 1;
                }

                zr2 = zr * zr;
                zi2 = zi * zi;

                if ((zr2 + zi2) > 4f)
                {
                    return i >= minBailout;
                }
            }
            return false;
        }

        private static bool[] BuddhaPointsFloatArray(ComplexBatch batch, int minBailout, int maxBailout)
        {
            var isBuddha = new bool[batch.R.Length];

            for (int index = 0; index < isBuddha.Length; index++)
            {
                var cr = batch.R[index];
                var ci = batch.I[index];

                var zr = 0f;
                var zi = 0f;

                var zr2 = 0f;
                var zi2 = 0f;

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
                        //WL($"Stopped iterating for index {index}: {i}");
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


        private static bool[] BuddhaPointsFloatArrayOrbitCheck(ComplexBatch batch, int minBailout, int maxBailout)
        {
            var isBuddha = new bool[batch.R.Length];

            for (int index = 0; index < isBuddha.Length; index++)
            {
                var cr = batch.R[index];
                var ci = batch.I[index];

                var zr = 0f;
                var zi = 0f;

                var zr2 = 0f;
                var zi2 = 0f;

                // Check for orbits
                // - Check re/im against an old point
                // - Only check every power of 2
                var oldzr = 0f;
                var oldzi = 0f;

                uint iterationToCheck = 1;

                for (int i = 0; i < maxBailout; i++)
                {
                    var tempZr = zr2 - zi2 + cr;
                    zi = 2 * zr * zi + ci;
                    zr = tempZr;

                    // Orbit check
                    if (iterationToCheck == i)
                    {
                        if (zr == oldzr && zi == oldzi)
                        {
                            isBuddha[index] = false;
                            break;
                        }

                        oldzr = zr;
                        oldzi = zi;

                        iterationToCheck = iterationToCheck << 1;
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

        private static bool[] BuddhaPointsFloatArrayOrbitCheckWithoutSquares(ComplexBatch batch, int minBailout, int maxBailout)
        {
            var isBuddha = new bool[batch.R.Length];

            for (int index = 0; index < isBuddha.Length; index++)
            {
                var cr = batch.R[index];
                var ci = batch.I[index];

                var zr = 0f;
                var zi = 0f;

                // Check for orbits
                // - Check re/im against an old point
                // - Only check every power of 2
                var oldzr = 0f;
                var oldzi = 0f;

                uint iterationToCheck = 1;

                for (int i = 0; i < maxBailout; i++)
                {
                    var tempZr = zr * zr - zi * zi + cr;
                    zi = 2 * zr * zi + ci;
                    zr = tempZr;

                    // Orbit check
                    if (iterationToCheck == i)
                    {
                        if (zr == oldzr && zi == oldzi)
                        {
                            isBuddha[index] = false;
                            break;
                        }

                        oldzr = zr;
                        oldzi = zi;

                        iterationToCheck = iterationToCheck << 1;
                    }

                    if ((zr * zr + zi * zi) > 4)
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

        private static int[] BuddhaPointsVectorFloat(ComplexBatch batch, int minBailout, int maxBailout)
        {
            //var isBuddha = new bool[Vector<float>.Count];

            var cr = new Vector<float>(batch.R);
            var ci = new Vector<float>(batch.I);

            var zr = Vector<float>.Zero;
            var zi = Vector<float>.Zero;

            var zr2 = Vector<float>.Zero;
            var zi2 = Vector<float>.Zero;

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            var vMinbailout = new Vector<int>(minBailout);
            var vMaxbailout = new Vector<int>(maxBailout);
            var vMagnitude = new Vector<float>(4);

            do
            {
                var tempZr = zr2 - zi2 + cr;
                zi = 2 * zr * zi + ci;
                zr = tempZr;

                zr2 = zr * zr;
                zi2 = zi * zi;

                var magnitude = zr2 + zi2;

                var shouldContinue =
                    Vector.LessThanOrEqual(magnitude, vMagnitude) &
                    Vector.LessThanOrEqual(iterations, vMaxbailout);

                increment = increment & shouldContinue;

                iterations += increment;
            } while (increment != Vector<int>.Zero);

            var wasBuddha =
                Vector.GreaterThanOrEqual(iterations, vMinbailout) &
                Vector.LessThanOrEqual(iterations, vMaxbailout);

            //WL( $"Iterations: {iterations}" );
            //WL( $"WasBuddha: {wasBuddha}" );

            //isBuddha[0] = wasBuddha[0] != 0;
            //isBuddha[1] = wasBuddha[1] != 0;
            //isBuddha[2] = wasBuddha[2] != 0;
            //isBuddha[3] = wasBuddha[3] != 0;

            var isBuddha = new int[Vector<int>.Count];
            wasBuddha.CopyTo(isBuddha);

            return isBuddha;
        }

        private static int[] BuddhaPointsVectorFloatOrbitCheck(ComplexBatch batch, int minBailout, int maxBailout)
        {
            //var isBuddha = new bool[Vector<float>.Count];

            var wasBuddha = Vector<int>.One;

            var cr = new Vector<float>(batch.R);
            var ci = new Vector<float>(batch.I);

            var zr = Vector<float>.Zero;
            var zi = Vector<float>.Zero;

            var zr2 = Vector<float>.Zero;
            var zi2 = Vector<float>.Zero;

            // Check for orbits
            // - Check re/im against an old point
            // - Only check every power of 2
            var oldzr = Vector<float>.Zero;
            var oldzi = Vector<float>.Zero;

            var iterationToCheck = Vector<int>.One;

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            var vMinbailout = new Vector<int>(minBailout);
            var vMaxbailout = new Vector<int>(maxBailout);
            var vMagnitude = new Vector<float>(4);

            var shouldContinue = Vector<int>.Zero;

            do
            {
                var tempZr = zr2 - zi2 + cr;
                zi = 2 * zr * zi + ci;
                zr = tempZr;

                zr2 = zr * zr;
                zi2 = zi * zi;

                if (Vector.EqualsAny(iterationToCheck, iterations))
                {
                    var matchesOldValue =
                        Vector.Equals(zr, oldzr) &
                        Vector.Equals(zi, oldzi);

                    shouldContinue = Vector.Xor(matchesOldValue, Vector<int>.One);

                    increment = increment & shouldContinue;
                    wasBuddha = wasBuddha & shouldContinue;

                    oldzr = zr;
                    oldzi = zi;

                    iterationToCheck = iterationToCheck * shouldContinue;
                    iterationToCheck += iterationToCheck;
                }

                var magnitude = zr2 + zi2;

                shouldContinue =
                    Vector.LessThanOrEqual(magnitude, vMagnitude) &
                    Vector.LessThanOrEqual(iterations, vMaxbailout);

                increment = increment & shouldContinue;

                iterations += increment;
            } while (increment != Vector<int>.Zero);

            wasBuddha =
                wasBuddha &
                Vector.GreaterThanOrEqual(iterations, vMinbailout) &
                Vector.LessThanOrEqual(iterations, vMaxbailout);

            //isBuddha[0] = wasBuddha[0] != 0;
            //isBuddha[1] = wasBuddha[1] != 0;
            //isBuddha[2] = wasBuddha[2] != 0;
            //isBuddha[3] = wasBuddha[3] != 0;

            //return isBuddha;

            var isBuddha = new int[Vector<int>.Count];
            wasBuddha.CopyTo(isBuddha);

            return isBuddha;
        }

        private static int[] BuddhaPointsVectorFloatOrbitCheckWithoutSquares(ComplexBatch batch, int minBailout, int maxBailout)
        {
            //var isBuddha = new bool[Vector<float>.Count];

            var wasBuddha = Vector<int>.One;

            var cr = new Vector<float>(batch.R);
            var ci = new Vector<float>(batch.I);

            var zr = Vector<float>.Zero;
            var zi = Vector<float>.Zero;

            // Check for orbits
            // - Check re/im against an old point
            // - Only check every power of 2
            var oldzr = Vector<float>.Zero;
            var oldzi = Vector<float>.Zero;

            var iterationToCheck = Vector<int>.One;

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            var vMinbailout = new Vector<int>(minBailout);
            var vMaxbailout = new Vector<int>(maxBailout);
            var vMagnitude = new Vector<float>(4);

            var shouldContinue = Vector<int>.Zero;

            do
            {
                var tempZr = zr * zr - zi * zi + cr;
                zi = 2 * zr * zi + ci;
                zr = tempZr;

                if (Vector.EqualsAny(iterationToCheck, iterations))
                {
                    var matchesOldValue =
                        Vector.Equals(zr, oldzr) &
                        Vector.Equals(zi, oldzi);

                    shouldContinue = Vector.Xor(matchesOldValue, Vector<int>.One);

                    increment = increment & shouldContinue;
                    wasBuddha = wasBuddha & shouldContinue;

                    oldzr = zr;
                    oldzi = zi;

                    iterationToCheck = iterationToCheck * shouldContinue;
                    iterationToCheck += iterationToCheck;
                }

                var magnitude = zr * zr + zi * zi;

                shouldContinue =
                    Vector.LessThanOrEqual(magnitude, vMagnitude) &
                    Vector.LessThanOrEqual(iterations, vMaxbailout);

                increment = increment & shouldContinue;

                iterations += increment;
            } while (increment != Vector<int>.Zero);

            wasBuddha =
                wasBuddha &
                Vector.GreaterThanOrEqual(iterations, vMinbailout) &
                Vector.LessThanOrEqual(iterations, vMaxbailout);

            //isBuddha[0] = wasBuddha[0] != 0;
            //isBuddha[1] = wasBuddha[1] != 0;
            //isBuddha[2] = wasBuddha[2] != 0;
            //isBuddha[3] = wasBuddha[3] != 0;

            //return isBuddha;

            var isBuddha = new int[Vector<int>.Count];
            wasBuddha.CopyTo(isBuddha);

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
