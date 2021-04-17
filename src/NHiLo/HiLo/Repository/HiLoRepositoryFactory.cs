using NHiLo.HiLo.Repository;
using NHiLo.HiLo.Config;
using System;
using System.Collections.Concurrent;

namespace NHiLo.HiLo.Repository
{
    /// <summary>
    /// Factory that creates repositories based on the provider specified in the connection string configuration.
    /// </summary>
    public class HiLoRepositoryFactory : IHiLoRepositoryFactory
    {
        private delegate IHiLoRepository CreateIHiLoRepositoryFunction(IHiLoConfiguration config);

        /// <summary>
        /// Relates each kind of provider to a function that actually creates the correct repository. If a new provider is add, this constant should change.
        /// </summary>
        private static readonly ConcurrentDictionary<string, CreateIHiLoRepositoryFunction> _factoryFunctions = 
            new ConcurrentDictionary<string, CreateIHiLoRepositoryFunction>()
            {
                ["Microsoft.Data.SqlClient"] = (config) => GetSqlServerRepository(config),
                ["MySql.Data.MySqlClient"] = (config) => new MySqlHiLoRepository(config),
                ["System.Data.OracleClient"] = (config) => new OracleHiLoRepository(config),
                ["Microsoft.Data.Sqlite"] = (config) => new SqliteHiLoRepository(config),
                ["NHiLo.InMemory"] = (config) => new InMemoryHiloRepository()
            };

        public IHiLoRepository GetRepository(string entityName, IHiLoConfiguration config)
        {
            string provider = config.ProviderName;
            if (string.IsNullOrWhiteSpace(provider))
                throw new NHiLoException(ErrorCodes.NoProviderName);
            if (!_factoryFunctions.ContainsKey(provider))
                throw new NHiLoException(ErrorCodes.ProviderNotImplemented).WithInfo("Provider Name", provider);
            var repository = new ExceptionWrapperRepository(() => _factoryFunctions[provider](config));
            repository.PrepareRepository(entityName);
            return repository;
        }

        private static IHiLoRepository GetSqlServerRepository(IHiLoConfiguration config)
        {
            if (config.StorageType == Common.Config.HiLoStorageType.Sequence)
            {
                return new SqlServerSequenceHiLoRepository(config);
            }
            return new SqlServerHiLoRepository(config);
        }

        /// <summary>
        /// Register a new repository to be used to store hi values.
        /// </summary>
        /// <param name="providerName">The name of the custom respository provider.</param>
        /// <param name="funcCreateRepository">A function that creates new instances of the repository.</param>
        public static void RegisterRepository(string providerName, Func<IHiLoRepository> funcCreateRepository)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException($"Provider name cannot be registered with an empty value.");
            lock (_factoryFunctions)
            {
                if (!_factoryFunctions.ContainsKey(providerName))
                    _factoryFunctions.TryAdd(providerName, (config) => funcCreateRepository());
            }
        }
    }
}
