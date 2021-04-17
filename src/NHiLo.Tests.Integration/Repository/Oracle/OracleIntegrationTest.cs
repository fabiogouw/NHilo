using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NHiLo.Tests.Integration.Repository.Oracle
{
    [Collection("Database Integration")]
    public class OracleIntegrationTest
    {
        private readonly ITestOutputHelper _output;

        public OracleIntegrationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Should_ConnectToABrandNewDatabaseAndGetKey()
        {
            var testcontainersBuilder = new TestcontainersBuilder<OracleTestcontainer>()
                .WithDatabase(new OracleTestcontainerConfiguration());

            await using (var testcontainer = testcontainersBuilder.Build())
            {
                await testcontainer.StartAsync();

                var appSettings = $@"{{
                    ""NHiLo"":{{
                        ""DefaultMaxLo"" : ""100""
                    }},
                    ""ConnectionStrings"":{{
                        ""NHiLo"":{{
                            ""ConnectionString"":""{ testcontainer.ConnectionString }"",
                            ""ProviderName"":""System.Data.OracleClient""
                        }}
                    }}
                }}";
                var builder = new ConfigurationBuilder();
                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
                var factory = new HiLoGeneratorFactory(builder.Build());

                var generator = factory.GetKeyGenerator("myOracleEntity");
                long key = generator.GetKey();
                _output.WriteLine($"Key generated: '{key}'");
                key.Should().BeGreaterThan(0, "is expected the key to be greater than 0.");

                await using (var connection = new OracleConnection(testcontainer.ConnectionString))
                {
                    connection.Open();
                    await using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM NHILO WHERE ENTITY = 'myOracleEntity'";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            long nexttHi = reader.GetInt64(reader.GetOrdinal("NEXT_HI"));
                            _output.WriteLine($"Next Hi value: '{nexttHi}'");
                            nexttHi.Should().Be(2, "is expected the next Hi value to be equal to 2 (first execution).");
                        }
                    }
                }
            }
        }
    }
}
