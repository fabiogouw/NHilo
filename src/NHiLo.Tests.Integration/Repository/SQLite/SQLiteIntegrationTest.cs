using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SQLitePCL;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace NHiLo.Tests.Integration.Repository.MySql
{
    [Collection("Database Integration")]
    public class SqliteIntegrationTest
    {
        private readonly ITestOutputHelper _output;

        public SqliteIntegrationTest(ITestOutputHelper output)
        {
            if (File.Exists("./mydb.db"))
            {
                File.Delete("./mydb.db");
            }
            Batteries.Init();
            _output = output;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task Should_ConnectToABrandNewDatabaseAndGetKey()
        {
            string connectionString = "Data Source=./mydb.db;";
            var appSettings = $@"{{
                    ""NHiLo"":{{
                        ""DefaultMaxLo"" : ""100""
                    }},
                    ""ConnectionStrings"":{{
                        ""NHiLo"":{{
                            ""ConnectionString"":""{ connectionString }"",
                            ""ProviderName"":""Microsoft.Data.Sqlite""
                        }}
                    }}
                }}";
            var builder = new ConfigurationBuilder();
            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));
            var factory = new HiLoGeneratorFactory(builder.Build());

            var generator = factory.GetKeyGenerator("mySqliteEntity");
            long key = generator.GetKey();
            _output.WriteLine($"Key generated: '{key}'");
            key.Should().BeGreaterThan(0, "is expected the key to be greater than 0.");

            await using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                await using (var cmd = new SqliteCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT * FROM NHILO WHERE ENTITY = 'mySqliteEntity'";
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
