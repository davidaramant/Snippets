using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace ConsoleRunner
{
    sealed class Program
    {
        public static void RunSnippet( string[] args )
        {

        }

        #region Helper methods

        private static string Desktop
        {
            get { return Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory ); }
        }

        public static void Main( string[] args )
        {
            try
            {
                RunSnippet( args );
            }
            catch( Exception e )
            {
                Console.WriteLine( "---\nEXCEPTION:\n" + e + "\n" );
            }
            finally
            {
                Console.Write( "Press any key to continue..." );
                Console.ReadKey();
            }
        }

        private static void WL()
        {
            Console.WriteLine();
        }

        private static void WL( object text, params object[] args )
        {
            if( text == null )
            {
                Console.WriteLine( "null" );
            }
            else
            {
                Console.WriteLine( text.ToString(), args );
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
            readonly System.Threading.ThreadPriority _previous;

            private Timed(string name)
            {
                GC.Collect(2);
                GC.WaitForPendingFinalizers();
                _name = name;
                _previous = System.Threading.Thread.CurrentThread.Priority;
                System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
                _watch = System.Diagnostics.Stopwatch.StartNew();
            }

            public static IDisposable Run(string name = "")
            {
                return new Timed(name);
            }

            public void Dispose()
            {
                _watch.Stop();
                System.Threading.Thread.CurrentThread.Priority = _previous;
                if (String.IsNullOrEmpty(_name))
                {
                    WL("{0}ms", _watch.ElapsedMilliseconds);
                }
                else
                {
                    WL("{0}: {1}ms", _name, _watch.ElapsedMilliseconds);
                }
                GC.Collect(2);
                GC.WaitForPendingFinalizers();
            }
        }

        #endregion
    }
}
