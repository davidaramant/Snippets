using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;

namespace ConsoleRunner
{
    sealed class Program
    {
        public static void RunSnippet(string[] args)
        {

        }

        #region Helper methods

        private static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        public static void Main(string[] args)
        {
            try
            {
                RunSnippet(args);
            }
            catch (Exception e)
            {
                WL("---\nEXCEPTION:\n" + e + "\n");
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
            readonly string _name;

            private Timed(string name)
            {
                GC.Collect(2);
                GC.WaitForPendingFinalizers();
                _name = name;
                _watch = System.Diagnostics.Stopwatch.StartNew();
            }

            public static IDisposable Run(string name)
            {
                return new Timed(name);
            }

            public void Dispose()
            {
                _watch.Stop();
                WL($"{_name}: {_watch.ElapsedMilliseconds}ms");
            }
        }

        #endregion
    }
}
