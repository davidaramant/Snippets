using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleRunner
{
    public sealed class CsvFile
    {
        private sealed class Entry : IEnumerable<FieldType>
        {
            public readonly string Name;
            private readonly IEnumerable<FieldType> _fields;

            public Entry( string name, IEnumerable<FieldType> fields )
            {
                Name = name;
                _fields = fields;
            }

            public IEnumerator<FieldType> GetEnumerator()
            {
                return _fields.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public sealed class FieldType
        {
            public readonly string Name;
            public readonly Type Type;

            public FieldType( string name, Type type )
            {
                Name = name;
                Type = type;
            }
        }

        private enum Stage
        {
            Names,
            Types,
            Rows,
        }

        public static CsvFile ParseFile( string fileName )
        {
            var currentStage = Stage.Names;

            int cols = 0;
            IEnumerable<string> names = null;
            IEnumerable<Type> types = null;
            List<IEnumerable<string>> rows = new List<IEnumerable<string>>();

            foreach( var line in File.ReadLines( fileName ) )
            {
                switch( currentStage )
                {
                    case Stage.Names:
                        names = ParseRow( line );
                        cols = names.Count();
                        currentStage = Stage.Types;
                        break;

                    case Stage.Types:
                        types = ParseRow( line ).Select( Type.GetType ).ToArray();
                        // TODO: Check width
                        currentStage = Stage.Rows;
                        break;

                    case Stage.Rows:
                        var newRow = ParseRow( line );
                        // TODO: Check width
                        rows.Add( newRow );
                        break;

                    default:
                        throw new Exception( "Messed up parsing" );
                }
            }

            return new CsvFile( names, types, rows );
        }

        public CsvFile( IEnumerable<string> names, IEnumerable<Type> types, List<IEnumerable<string>> rows )
        {

        }

        private enum RowState
        {
            ColumnStart,
            UnescapedColumn,
            EscapedColumn,
            QuoteInEscapedColumn,
        }

        public static IEnumerable<string> ParseRow( string line )
        {
            var state = RowState.ColumnStart;

            var currentColumn = new StringBuilder();

            foreach( var c in line )
            {
                switch( state )
                {
                    case RowState.ColumnStart:
                        if( c == '"' )
                        {
                            state = RowState.EscapedColumn;
                        }
                        else if( c == ',' )
                        {
                            yield return currentColumn.ToString();
                            currentColumn.Clear();
                        }
                        else
                        {
                            state = RowState.UnescapedColumn;
                            currentColumn.Append( c );
                        }
                        break;

                    case RowState.UnescapedColumn:
                        if( c == '"' )
                        {
                            throw new ArgumentException( "Cannot have quote in unescaped column" );
                        }
                        else if( c == ',' )
                        {
                            yield return currentColumn.ToString();
                            currentColumn.Clear();
                            state = RowState.ColumnStart;
                        }
                        else
                        {
                            currentColumn.Append( c );
                        }
                        break;

                    case RowState.EscapedColumn:
                        if( c == '"' )
                        {
                            state = RowState.QuoteInEscapedColumn;
                        }
                        else
                        {
                            currentColumn.Append( c );
                        }
                        break;

                    case RowState.QuoteInEscapedColumn:
                        if( c == '"' )
                        {
                            currentColumn.Append( c );
                            state = RowState.EscapedColumn;
                        }
                        else if( c == ',' )
                        {
                            yield return currentColumn.ToString();
                            currentColumn.Clear();
                            state = RowState.ColumnStart;
                        }
                        else
                        {
                            throw new ArgumentException( "Invalid escaped column" );
                        }
                        break;
                }
            }

            if( currentColumn.Length > 0 )
            {
                yield return currentColumn.ToString();
            }
        }
    }
}
