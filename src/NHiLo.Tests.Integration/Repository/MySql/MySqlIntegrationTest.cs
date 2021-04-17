using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NHiLo.Tests.Integration.Repository.MySql
{
    [Collection("Database Integration")]
    public class MySqlIntegrationTest
    {
        private readonly ITestOutputHelper _output;

        public MySqlIntegrationTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Should_ConnectToABrandNewDatabaseAndGetKey()
        {
            var testcontainersBuilder = new TestcontainersBuilder<MySqlTestcontainer>()
                .WithDatabase(new MySqlTestcontainerConfiguration
                {
                    Database = "myDataBase",
                    Username = "myUser",
                    Password = "myPassword",
                    
                });

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
                            ""ProviderName"":""MySql.Data.MySqlClient""
                        }}
                    }}
                }}";
                var builder = new ConfigurationBuilder();
                builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
                var factory = new HiLoGeneratorFactory(builder.Build());

                var generator = factory.GetKeyGenerator("myMySqlEntity");
                long key = generator.GetKey();
                _output.WriteLine($"Key generated: '{key}'");
                key.Should().BeGreaterThan(0, "is expected the key to be greater than 0.");

                await using (var connection = new MySqlConnection(testcontainer.ConnectionString))
                {
                    connection.Open();
                    await using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "SELECT * FROM NHILO WHERE ENTITY = 'myMySqlEntity'";
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                            long nexttHi = reader.GetInt64("NEXT_HI");
                            _output.WriteLine($"Next Hi value: '{nexttHi}'");
                            nexttHi.Should().Be(2, "is expected the next Hi value to be equal to 2 (first execution).");
                        }
                    }
                }
            }
        }
    }
}
