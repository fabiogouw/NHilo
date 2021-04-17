using NHiLo.HiLo.Config;
using System.Data;
using Microsoft.Data.Sqlite;

namespace NHiLo.HiLo.Repository
{
    /// <summary>
    /// NHiLo's repository implementation for Microsoft SQL Server.
    /// </summary>
    public class SqliteHiLoRepository : AgnosticHiLoRepository
    {
        private const string SQLSERVERCREATESTRUCTURE = @"
            CREATE TABLE IF NOT EXISTS {0}(
            {2} [nvarchar](100) NOT NULL,
            {1} [bigint] NOT NULL,
                PRIMARY KEY({2} ASC)
            );
            SELECT 1;";
        private const string SQLSERVERINITIALIZEENTITY = @"
            INSERT OR IGNORE INTO {0} VALUES ({3}, 1);
        ";
        private string _sqlStatementToSelectAndUpdateNextHiValue;
        private string _sqlStatementToCreateRepository;
        private string _sqlStatementToInitializeEntity;

        public SqliteHiLoRepository(IHiLoConfiguration config)
            : base(config, Microsoft.Data.Sqlite.SqliteFactory.Instance)
        {
            InitializeSqlStatements(config);
        }

        private void InitializeSqlStatements(IHiLoConfiguration config)
        {
            _sqlStatementToSelectAndUpdateNextHiValue = PrepareSqlStatement("SELECT {1} FROM {0} WHERE {2} = {3};UPDATE {0} SET {1} = {1} + 1 WHERE {2} = {3};", config);
            _sqlStatementToCreateRepository = PrepareSqlStatement(SQLSERVERCREATESTRUCTURE, config);
            _sqlStatementToInitializeEntity = PrepareSqlStatement(SQLSERVERINITIALIZEENTITY, config);
        }

        protected override long GetNextHiFromDatabase(IDbCommand cmd)
        {
            cmd.CommandText = _sqlStatementToSelectAndUpdateNextHiValue;
            cmd.Parameters.Add(CreateEntityParameter(cmd, EntityName));
            return (long)cmd.ExecuteScalar();
        }

        protected override void CreateRepositoryStructure(IDbCommand cmd)
        {
            cmd.CommandText = _sqlStatementToCreateRepository;
            cmd.ExecuteNonQuery();
        }

        protected override void InitializeRepositoryForEntity(IDbCommand cmd)
        {
            cmd.CommandText = _sqlStatementToInitializeEntity;
            cmd.Parameters.Add(CreateEntityParameter(cmd, EntityName));
            cmd.ExecuteNonQuery();
        }
    }
}
