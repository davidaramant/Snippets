using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ConsoleRunner;

namespace Tests
{
    [TestFixture]
    public sealed class RowParsingTests
    {
        [Test]
        public void ShouldHandleEmptyRow()
        {
            Assert.That( CsvFile.ParseRow( String.Empty ), Is.Empty, "Did not handle empty row." );
        }

        [Test]
        public void ShouldHandleJustSeparators()
        {
            Assert.That(
                CsvFile.ParseRow( ",," ).ToArray(),
                Is.EqualTo( new[] { String.Empty, String.Empty } ),
                "Did not parse empty columns." );
        }

        [Test]
        public void ShouldHandleSingleUnescaptedColumn()
        {
            Assert.That(
                CsvFile.ParseRow( "Column" ).ToArray(),
                Is.EqualTo( new[] { "Column" } ),
                "Did not parse single column." );
        }

        [Test]
        public void ShouldHandleMultipleUnescapedColumns()
        {
            Assert.That(
                CsvFile.ParseRow( "Column1,Column2" ).ToArray(),
                Is.EqualTo( new[] { "Column1", "Column2" } ),
                "Did not parse multiple columns." );
        }

        [Test]
        public void ShouldHandleSingleEscaptedColumn()
        {
            Assert.That(
                CsvFile.ParseRow( "\"Column\"" ).ToArray(),
                Is.EqualTo( new[] { "Column" } ),
                "Did not parse single column." );
        }

        [Test]
        public void ShouldHandleMultipleEscapedColumns()
        {
            Assert.That(
                CsvFile.ParseRow( "\"Column1\",\"Column2\"" ).ToArray(),
                Is.EqualTo( new[] { "Column1", "Column2" } ),
                "Did not parse multiple columns." );
        }

        [Test]
        public void ShouldHandleSingleUnescaptedColumnWithSpaces()
        {
            Assert.That(
                CsvFile.ParseRow( "Column 1" ).ToArray(),
                Is.EqualTo( new[] { "Column 1" } ),
                "Did not parse single column." );
        }

        [Test]
        public void ShouldHandleMultipleUnescapedColumnsWithSpaces()
        {
            Assert.That(
                CsvFile.ParseRow( "Column 1,Column 2" ).ToArray(),
                Is.EqualTo( new[] { "Column 1", "Column 2" } ),
                "Did not parse multiple columns." );
        }

        [Test]
        public void ShouldHandleSingleEscaptedColumnWithSpaces()
        {
            Assert.That(
                CsvFile.ParseRow( "\"Column 1\"" ).ToArray(),
                Is.EqualTo( new[] { "Column 1" } ),
                "Did not parse single column." );
        }

        [Test]
        public void ShouldHandleMultipleEscapedColumnsWithSpaces()
        {
            Assert.That(
                CsvFile.ParseRow( "\"Column 1\",\"Column 2\"" ).ToArray(),
                Is.EqualTo( new[] { "Column 1", "Column 2" } ),
                "Did not parse multiple columns." );
        }

        [Test]
        public void ShouldHandleEscapedColumnsWithCommas()
        {
            Assert.That(
                CsvFile.ParseRow( "\"Column, Number 1\",\"Column, Number 2\"" ).ToArray(),
                Is.EqualTo( new[] { "Column, Number 1", "Column, Number 2" } ),
                "Did not parse multiple columns." );
        }


        [Test]
        public void ShouldHandleEscapedColumnsWithQuotes()
        {
            Assert.That(
                CsvFile.ParseRow( "\"Column \"\"1\"\"\",\"Column \"\"2\"\"\"" ).ToArray(),
                Is.EqualTo( new[] { "Column \"1\"", "Column \"2\"" } ),
                "Did not parse multiple columns." );
        }
    }
}
