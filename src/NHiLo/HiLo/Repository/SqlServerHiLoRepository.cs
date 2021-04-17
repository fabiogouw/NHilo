using Microsoft.Data.SqlClient;
using NHiLo.HiLo.Config;
using System.Data;

namespace NHiLo.HiLo.Repository
{
    /// <summary>
    /// NHiLo's repository implementation for Microsoft SQL Server.
    /// </summary>
    public class SqlServerHiLoRepository : AgnosticHiLoRepository
    {
        private const int ERROR_NUMBER_CREATE_TABLE_PERMISSION = 262;
        private const int ERROR_NUMBER_SELECT_PERMISSION = 229;

        private const string SQLSERVERCREATESTRUCTURE = @"
            IF NOT EXISTS(SELECT 1 FROM SYS.OBJECTS WHERE NAME = '{0}' AND TYPE = 'U')
            BEGIN
                CREATE TABLE [dbo].[{0}](
                [{2}] [nvarchar](100) NOT NULL,
                [{1}] [bigint] NOT NULL,
                    CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED 
                    (
                        [{2}] ASC
                    )
                );
                SELECT 1; 
            END
            ELSE
                SELECT 0;";
        private const string SQLSERVERINITIALIZEENTITY = @"
            SET NOCOUNT ON;
            IF NOT EXISTS(SELECT 1 FROM {0} WHERE {2} = {3})
            BEGIN
                INSERT INTO {0} VALUES ({3}, 1);
            END
        ";
        private string _sqlStatementToSelectAndUpdateNextHiValue;
        private string _sqlStatementToCreateRepository;
        private string _sqlStatementToInitializeEntity;
        private string _sqlStatementToCheckSelectPermission;

        public SqlServerHiLoRepository(IHiLoConfiguration config)
            : base(config, Microsoft.Data.SqlClient.SqlClientFactory.Instance)
        {
            InitializeSqlStatements(config);
        }

        private void InitializeSqlStatements(IHiLoConfiguration config)
        {
            _sqlStatementToSelectAndUpdateNextHiValue = PrepareSqlStatement("SELECT {1} FROM {0} WHERE {2} = {3};UPDATE {0} SET {1} = {1} + 1 WHERE {2} = {3};", config);
            _sqlStatementToCreateRepository = PrepareSqlStatement(SQLSERVERCREATESTRUCTURE, config);
            _sqlStatementToInitializeEntity = PrepareSqlStatement(SQLSERVERINITIALIZEENTITY, config);
            _sqlStatementToCheckSelectPermission = PrepareSqlStatement("SELECT * FROM [dbo].[{0}]", config);
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
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                CheckSelectPermissionError(cmd, ex);
            }
        }

        private void CheckSelectPermissionError(IDbCommand cmd, SqlException ex)
        {
            if (ex.Number == ERROR_NUMBER_CREATE_TABLE_PERMISSION)
            {
                cmd.CommandText = _sqlStatementToCheckSelectPermission;
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex2)
                {
                    throw ex2.Number == ERROR_NUMBER_SELECT_PERMISSION ? ex2 : ex;
                }
            }
        }

        protected override void InitializeRepositoryForEntity(IDbCommand cmd)
        {
            cmd.CommandText = _sqlStatementToInitializeEntity;
            cmd.Parameters.Add(CreateEntityParameter(cmd, EntityName));
            cmd.ExecuteNonQuery();
        }
    }
}
