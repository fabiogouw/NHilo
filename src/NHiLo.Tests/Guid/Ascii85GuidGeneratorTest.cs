using NHiLo.Guid;
using Xunit;
using FluentAssertions;

namespace NHiLo.Tests.Guid
{
    public class Ascii85GuidGeneratorTest
    {
        public class GetKey : BaseTestGuidGenerator.GetKey
        {
            [Fact]
            [Trait("Category", "Unit")]
            public void ShouldGetANonNullAscii85Guid()
            {
                // Arrange
                var generator = new Ascii85GuidGenerator();
                // Act & Assert
                ShouldGetANonNullGuid(generator);
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void ShouldGetDifferentAscii85Keys()
            {
                // Arrange
                var generator = new Ascii85GuidGenerator();
                // Act & Assert
                ShouldGetDifferentKeys(generator);
            }

            [Fact]
            [Trait("Category", "Unit")]
            public void Should_Get20LengthGuid_When_GeneratingNewKey()
            {
                // Arrange
                var generator = new Ascii85GuidGenerator();
                // Act
                var key = generator.GetKey();
                // Assert
                key.Length.Should().Be(20);
            }

        }
    }
}
