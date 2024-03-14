using System.Data;
using Dapper;

namespace OrderAgregator.API.Cache.SqLiteCache
{
    public interface ISqLiteDatabase : IDisposable
    {
        /// <summary>
        ///     Load data from database
        /// </summary>
        Task<List<T>> LoadData<T, U>(string sql, CommandType commandType, U parameters);

        /// <summary>
        ///     Save data to database
        /// </summary>
        Task<int> SaveData<U>(string sql, CommandType commandType, U parameters);

        /// <summary>
        ///     Create new transaction on connection
        /// </summary>
        void StartTransaction(IsolationLevel isolationLevel);

        /// <summary>
        ///     Commit active transaction and close it
        /// </summary>
        void Commit();

        /// <summary>
        ///     Rollback active transaction and close it
        /// </summary>
        void RollBack();
    }

    public class SqLiteDatabase : ISqLiteDatabase
    {
        /// <summary> Flag if unit and underlaying connection is disposed </summary>
        private bool disposed = false;

        /// <summary> Underlaying connection </summary>
        private readonly IDbConnection _connection;

        /// <summary> Underlaying transaction </summary>
        private IDbTransaction? _transaction;

        public SqLiteDatabase(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqLite") ?? throw new ArgumentException(message: "SqLite connection string not set");

            var sqLiteConfig = configuration.GetSection("SqLite");
            var rebuild = sqLiteConfig.GetValue<bool?>("RebuildOnInitialize");

            _connection = Initializer.Connect(connectionString, rebuild ?? false);
        }

        /// <inheritdoc/>
        public async Task<List<T>> LoadData<T, U>(string sql, CommandType commandType, U parameters)
        {
            var rows = await _connection.QueryAsync<T>(sql, parameters, commandType: commandType, transaction: _transaction);

            return rows.ToList();
        }

        /// <inheritdoc/>
        public async Task<int> SaveData<U>(string sql, CommandType commandType, U parameters)
        {
            return await _connection.ExecuteAsync(sql, parameters, commandType: commandType, transaction: _transaction);
        }

        /// <inheritdoc/>
        public void StartTransaction(IsolationLevel isolationLevel)
        {
            if (_transaction != null)
                return;

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            _transaction = _connection.BeginTransaction(isolationLevel);
        }

        /// <inheritdoc/>
        public void Commit()
        {
            if (_transaction == null)
                return;

            try
            {
                _transaction.Commit();

                if (_connection.State != ConnectionState.Closed)
                    _connection.Close();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        /// <inheritdoc/>
        public void RollBack()
        {
            if (_transaction == null)
                return;

            try
            {
                _transaction.Rollback();

                if (_connection.State != ConnectionState.Closed)
                    _connection.Close();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose() => Dispose(disposed != true);
        protected virtual void Dispose(bool disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();

            disposed = true;
        }
    }
}
