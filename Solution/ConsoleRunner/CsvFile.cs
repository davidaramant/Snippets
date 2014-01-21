using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleRunner
{
    public sealed class CsvFile
    {
        public sealed class Entry : IEnumerable<Field>
        {
            public readonly string Name;
            private readonly IEnumerable<Field> _fields;

            public Entry( string name, IEnumerable<Field> fields )
            {
                Name = name;
                _fields = fields;
            }

            public IEnumerator<Field> GetEnumerator()
            {
                return _fields.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public sealed class Field
        {
            public readonly FieldType Metadata;
            public readonly string Value;

            public Field( FieldType metadata, string value )
            {
                Metadata = metadata;
                Value = value;
            }
        }

        public sealed class FieldType
        {
            public readonly string RawName;
            public readonly Type Type;

            public string FieldName
            {
                get { return "_" + ArgumentName; }
            }

            public string ArgumentName
            {
                get { return Char.ToLowerInvariant( RawName[0] ) + RawName.Substring( 1 ); }
            }

            public string PropertyName
            {
                get { return Char.ToUpperInvariant( RawName[0] ) + RawName.Substring( 1 ); }
            }

            public FieldType( string rawName, Type type )
            {
                RawName = rawName;
                Type = type;
            }
        }

        private readonly List<string[]> _rows;

        public readonly IEnumerable<FieldType> FieldTypes;

        public CsvFile( string[] names, Type[] types, List<string[]> rows )
        {
            _rows = rows;

            FieldTypes = names.Zip(
                types,
                ( name, type ) => new FieldType( name, type ) ).ToArray();
        }

        public IEnumerable<Entry> GetEntries()
        {
            return
                _rows.Select(
                    row => new Entry(
                        row.First(),
                        FieldTypes.Zip(
                            row.Skip( 1 ),
                            ( fieldType, colValue ) => new Field( fieldType, colValue ) ) ) );
        }

        #region Parsing CSV file

        private enum Stage
        {
            Names,
            Types,
            Rows,
        }

        public static CsvFile ParseFile( IEnumerable<string> lines )
        {
            var currentStage = Stage.Names;

            int numFields = 0;
            string[] names = null;
            Type[] types = null;
            List<string[]> rows = new List<string[]>();

            foreach( var line in lines )
            {
                switch( currentStage )
                {
                    case Stage.Names:
                        names = ParseRow( line ).Skip( 1 ).ToArray();
                        numFields = names.Length;
                        currentStage = Stage.Types;
                        break;

                    case Stage.Types:
                        types = ParseRow( line ).Skip( 1 ).Select( Type.GetType ).ToArray();
                        if( types.Length != numFields )
                        {
                            throw new ArgumentException( "Bad number of types." );
                        }
                        currentStage = Stage.Rows;
                        break;

                    case Stage.Rows:
                        var newRow = ParseRow( line ).ToArray();
                        if( newRow.Length != numFields + 1 ) // These rows include the name as well
                        {
                            throw new ArgumentException( "Bad number of columns in data row." );
                        }
                        rows.Add( newRow );
                        break;

                    default:
                        throw new Exception( "Messed up parsing" );
                }
            }

            return new CsvFile( names, types, rows );
        }

        #endregion

        #region Parsing CSV row

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

        #endregion
    }
}
