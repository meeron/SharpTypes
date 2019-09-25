using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Xunit;

namespace SharpTypes.Tests
{
    public class TypeWriterTests
    {
        private const string ExpectedMockType = "export interface MockType { }";

        private readonly TypeWriter _typeWriter;

        public TypeWriterTests()
        {
            _typeWriter = new TypeWriter();
        }

        [Theory]
        [InlineData(typeof(MockType), ExpectedMockType)]
        [InlineData(typeof(IEnumerable<MockType>), ExpectedMockType)]
        [InlineData(typeof(List<MockType>), ExpectedMockType)]
        [InlineData(typeof(MockType[]), ExpectedMockType)]
        [InlineData(typeof(Dictionary<MockType, MockType>), "")]
        [InlineData(typeof(string), "")]
        [InlineData(typeof(string[]), "")]
        [InlineData(typeof(IEnumerable<string>), "")]
        [InlineData(typeof(List<string>), "")]
        [InlineData(typeof(int), "")]
        [InlineData(typeof(int[]), "")]
        [InlineData(typeof(IEnumerable<int>), "")]
        [InlineData(typeof(List<int>), "")]
        [InlineData(typeof(bool), "")]
        [InlineData(typeof(Guid), "")]
        [InlineData(typeof(DateTime), "")]
        [InlineData(typeof(TimeSpan), "")]
        [InlineData(typeof(MockEnum), "")]
        public void Write_GivenType_ShouldRenderInterfaceType(Type type, string expected)
        {
            using (var stringWriter = new StringWriter())
            {
                // Act
                _typeWriter.Write(type, stringWriter);

                // Assert
                Assert.Equal(expected, stringWriter.ToString());
            }
        }

        [Fact]
        public void Write_GivenTypeWithSimpleProperties_ShouldRenderInterfaceType()
        {
            // Arrange
            const string expected =
                "export interface MockSimpleTypes { id: number; name: string; guid: string; bool: boolean; " +
                "int: number; uInt: number; long: number; uLong: number; short: number; uShort: number; " +
                "byte: number; sByte: number; float: number; double: number; decimal: number; dateTime: string; "+
                "arrayInt: number[]; listInt: number[]; collectionInt: number[]; }";

            using (var stringWriter = new StringWriter())
            {
                // Act
                _typeWriter.Write(typeof(MockSimpleTypes), stringWriter);

                var result = stringWriter.ToString();

                // Assert
                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public void Write_GivenTypeWithComplexProperties_ShouldRenderInterfaceType()
        {
            // Arrange
            const string expected = "export interface MockTypeComplex { mockType: MockType; arrayMockType: MockType[]; "+
                                    "listMockType: MockType[]; collectionMockType: MockType[]; dictionary: any; object: any; }\n" +
                                    "export interface MockType { }";

            using (var stringWriter = new StringWriter())
            {
                // Act
                _typeWriter.Write(typeof(MockTypeComplex), stringWriter);
                var result = stringWriter.ToString();

                // Assert
                Assert.Equal(expected, result);
            }
        }

        public class MockSimpleTypes
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public Guid Guid { get; set; }

            public bool Bool { get; set; }

            public int Int { get; set; }

            public uint UInt { get; set; }

            public long Long { get; set; }

            public ulong ULong { get; set; }

            public short Short { get; set; }

            public ushort UShort { get; set; }

            public byte Byte { get; set; }

            public sbyte SByte { get; set; }

            public float Float { get; set; }

            public double Double { get; set; }

            public decimal Decimal { get; set; }

            public DateTime DateTime { get; set; }
            
            public int[] ArrayInt { get; set; }

            public List<int> ListInt { get; set; }

            public IEnumerable<int> CollectionInt { get; set; }
        }

        public class MockType
        {
        }

        public class MockTypeComplex
        {
            public MockType MockType { get; set; }

            public MockType[] ArrayMockType { get; set; }

            public List<MockType> ListMockType { get; set; }

            public IEnumerable<MockType> CollectionMockType { get; set; }

            public Dictionary<string, string> Dictionary { get; set; }

            public object Object { get; set; }
        }

        public enum MockEnum
        {
            Value
        }
    }
}
