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
            WL( WorstSubstring(
                    "This is a long string upsum loreal yeah whatever this is probably good enough",
                    start: 0,
                    length: 11 ) );
        }

        static string WorstSubstring( string input, int start, int length )
        {
            var inputArray = input.ToCharArray();

            var inputMatrix = new char[1, inputArray.Length];

            for( int i = 0; i < inputArray.Length; i++ )
            {
                inputMatrix[0, i] = inputArray[i];
            }

            var shorteningMatrix = new char[inputArray.Length, length];

            for( int col = 0; col < length; col++ )
            {
                shorteningMatrix[start + col, col] = (char)1;
            }

            var substringMatrix = Multiply( inputMatrix, shorteningMatrix );

            var substringArray = new char[length];
            for( int i = 0; i < length; i++ )
            {
                substringArray[i] = substringMatrix[0, i];
            }

            return new String( substringArray );
        }

        static char[,] Multiply( char[,] a, char[,] b )
        {
            if( a.GetLength( 1 ) != b.GetLength( 0 ) )
            {
                throw new ArgumentException( "Invalid matrix sizes for multiplcation." );
            }

            var c = new char[a.GetLength( 0 ), b.GetLength( 1 )];

            for( int i = 0; i < a.GetLength( 0 ); i++ )
            {
                for( int j = 0; j < b.GetLength( 1 ); j++ )
                {
                    for( int k = 0; k < a.GetLength( 1 ); k++ )
                    {
                        c[i, j] += (char)( a[i, k] * b[k, j] );
                    }
                }
            }

            return c;
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

        class Timed : IDisposable
        {
            readonly System.Diagnostics.Stopwatch _watch;
            readonly string _name = String.Empty;
            readonly System.Threading.ThreadPriority _previous;


            private Timed( string name )
            {
                _name = name;
                _previous = System.Threading.Thread.CurrentThread.Priority;
                System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;
                _watch = System.Diagnostics.Stopwatch.StartNew();
            }

            public static IDisposable Run()
            {
                return Run( String.Empty );
            }

            public static IDisposable Run( string name )
            {
                return new Timed( name );
            }

            public void Dispose()
            {
                _watch.Stop();
                System.Threading.Thread.CurrentThread.Priority = _previous;
                if( String.IsNullOrEmpty( _name ) )
                {
                    WL( "{0}ms", _watch.ElapsedMilliseconds );
                    WL( _watch.ElapsedMilliseconds );
                }
                else
                {
                    WL( "{0}: {1}ms", _name, _watch.ElapsedMilliseconds );
                    WL( "{0}: {1}", _name, _watch.ElapsedMilliseconds );
                }
            }
        }

        #endregion
    }
}
