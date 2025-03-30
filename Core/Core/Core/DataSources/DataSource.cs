using Core.DataSources.Exceptions;
using Core.DataSources.Extensions;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using static Core.DataSources.Exceptions.SQLException;
using static Dapper.SqlMapper;

namespace Core.DataSources {

    public class SQLDataBase : IDataSource {
        private const int maxTimeOut = 300;
        private SqlCommand Command;
        private SqlConnection sqlConnection;
        private bool UseTransaction;
        private SqlTransaction transaction;

        public string connectionConfig { get; set; }

        public void CreateConnection(string connectionConfig) {
            try {
                sqlConnection = new SqlConnection(connectionConfig);
                if (sqlConnection?.State == ConnectionState.Closed) sqlConnection.Open();
                Command = sqlConnection.CreateCommand();
            }
            catch (Exception) {
                Dispose();
                throw new SQLException(SQLErrorEnum.Connection);
            }
        }

        public void Dispose() {
            Command?.Dispose();
            transaction?.Dispose();
            if (sqlConnection != null) {
                if (sqlConnection.State != ConnectionState.Closed) sqlConnection.Close();
                SqlConnection.ClearPool(sqlConnection);
                sqlConnection.Dispose();
            }
        }

        public void Dispose(SqlConnection connection, SqlCommand? command = null, SqlTransaction? transaction = null) {
            command?.Dispose();
            transaction?.Dispose();
            if (connection != null) {
                if (connection.State != ConnectionState.Closed) connection.Close();
                connection.Dispose();
            }
        }

        public void BeginTransaction() {
            UseTransaction = true;
            transaction = sqlConnection.BeginTransaction();
            Command.Transaction = transaction;
        }

        public void CommitTransaction() {
            if (UseTransaction) transaction.Commit();
            Dispose();
        }

        public void RollbackTransaction() {
            if (UseTransaction) {
                void Run(Action act) {
                    try {
                        act();
                    }
                    catch (Exception) { }
                }
                Run(() => Command.Dispose());
                Run(() => transaction.Rollback());
                Run(() => transaction.Dispose());
                Run(() => sqlConnection.Close());
                Run(() => sqlConnection.Dispose());
            }
            Dispose();
        }

        public T? ExecuteDapperScalar<T>(string query, object? parameters = null, int? timeOut = null) {
            var result = SqlMapper.Query<T>(sqlConnection, query, parameters, transaction, commandTimeout: timeOut);
            return result != null ? result.FirstOrDefault() : default;
        }

        public async Task<T?> ExecuteDapperScalarAsync<T>(string query, object? parameters = null, int? timeOut = null) {
            var result = await SqlMapper.QueryAsync<T>(sqlConnection, query, parameters, transaction, timeOut);
            return result != null ? result.FirstOrDefault() : default;
        }

        public async Task<T?> ExecuteDapperScalarNullableAsync<T>(string query, object? parameters = null, int? timeOut = null) {
            var result = await SqlMapper.QueryAsync<T>(sqlConnection, query, parameters, transaction, timeOut);
            return result.FirstOrDefault();
        }

        public IList<T> ExecuteDapper<T>(string query, object? parameters = null, int? timeOut = null) {
            return (IList<T>)SqlMapper.Query<T>(sqlConnection, query, parameters, transaction, true, timeOut);
        }

        public async Task<IList<T>> ExecuteDapperAsync<T>(string query, object parameters = null, int? timeOut = null) {
            return (IList<T>)await SqlMapper.QueryAsync<T>(sqlConnection, query, parameters, transaction, timeOut);
        }

        public GridReader? ExecuteDapperMultipleQuery(string query, object? parameters = null, int? timeOut = null) {
            return sqlConnection.QueryMultiple(query, parameters, transaction, timeOut);
        }

        public async Task<GridReader?> ExecuteDapperMultipleQueryAsync(string query, object parameters = null, int? timeOut = null) {
            return await sqlConnection.QueryMultipleAsync(query, parameters, transaction, timeOut);
        }

        public async Task ExecuteConnectionAsync(string query, object parameters = null, int? timeOut = null) {
            await sqlConnection.ExecuteAsync(query, parameters, transaction, timeOut);
        }

        public void ExecuteConnection(string query, object parameters = null, int? timeOut = null) {
            sqlConnection.Execute(query, parameters, transaction, timeOut);
        }

        public void ExecuteQuery(string query) {
            try {
                Command.CommandText = query;
                Command.ExecuteNonQuery();
            }
            catch (Exception) {
                throw new SQLException(SQLErrorEnum.ExecutionSQL);
            }
        }

        public async Task ExecuteQueryAsync(string query) {
            try {
                Command.CommandText = query;
                await Command.ExecuteNonQueryAsync();
            }
            catch (Exception) {
                throw new SQLException(SQLErrorEnum.ExecutionSQLAsync);
            }
        }

        public IList<T> Select<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                connection.Open();
                var command = connection.CreateCommand();
                var result = (IList<T>)SqlMapper.Query<T>(connection, query, parameters, transaction, true, timeOut);
                Dispose(connection, command);
                return result;
            }
        }

        public async Task<IList<T>> SelectAsync<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                var result = (IList<T>)await SqlMapper.QueryAsync<T>(connection, query, parameters, null, timeOut);
                Dispose(connection, command);
                command = null;
                return result;
            }
        }

        public T? SelectScalar<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                connection.Open();
                var command = connection.CreateCommand();
                var result = SqlMapper.Query<T>(connection, query, parameters, null, commandTimeout: timeOut);
                Dispose(connection, command);
                return result != null ? result.FirstOrDefault() : default;
            }
        }

        public async Task<T?> SelectScalarAsync<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                var result = await SqlMapper.QueryAsync<T>(connection, query, parameters, null, commandTimeout: timeOut);
                Dispose(connection, command);
                return result != null ? result.FirstOrDefault() : default;
            }
        }

        public GridReader SelectMultipleQuery(string query, object? parameters = null, int? timeOut = null) {
            GridReader result;
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                connection.Open();
                var command = connection.CreateCommand();
                result = connection.QueryMultiple(query, parameters, null, timeOut);
                Dispose(connection, command);
            }
            return result;
        }

        public async Task<GridReader> SelectMultipleQueryAsync(string query, object? parameters = null, int? timeOut = null) {
            GridReader result;
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                result = await connection.QueryMultipleAsync(query, parameters, null, timeOut);
                Dispose(connection, command);
            }
            return result;
        }

        public T TransactionalQuery<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    connection.Open();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    var result = (T)SqlMapper.Query<T>(connection, query, parameters, transaction, commandTimeout: timeOut);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                    return result;
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
        }

        public T? TransactionalQueryScalar<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    connection.Open();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    var result = SqlMapper.Query<T>(connection, query, parameters, transaction, commandTimeout: timeOut);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                    return result != null ? result.FirstOrDefault() : default;
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
        }

        public async Task<T> TransactionalQueryAsync<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    await connection.OpenAsync();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    var result = (T)await SqlMapper.QueryAsync<T>(connection, query, parameters, transaction, timeOut);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                    return result;
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
        }

        public async Task<T?> TransactionalQueryScalarAsync<T>(string query, object? parameters = null, int? timeOut = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    await connection.OpenAsync();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    var result = await SqlMapper.QueryAsync<T>(connection, query, parameters, transaction, timeOut);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                    return result != null ? result.FirstOrDefault() : default;
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
        }

        public GridReader TransactionalMultipleQuery(string query, object? parameters = null, int? timeOut = null) {
            GridReader result;
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    connection.Open();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    result = connection.QueryMultiple(query, parameters, transaction, timeOut);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
            return result;
        }

        public async Task<GridReader> TransactionalMultipleQueryAsync(string query, object? parameters = null, int? timeOut = null) {
            GridReader result;
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    await connection.OpenAsync();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    result = await connection.QueryMultipleAsync(query, parameters, transaction, timeOut);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
            return result;
        }

        public void Execute(string query, object? parameters = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                try {
                    connection.Open();
                    command = connection.CreateCommand();
                    connection.Execute(query, parameters);
                    Dispose(connection, command);
                }
                catch (Exception) {
                    Dispose(connection, command);
                    throw;
                }
            }
        }

        public async Task ExecuteAsync(string query, object? parameters = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                try {
                    await connection.OpenAsync();
                    command = connection.CreateCommand();
                    await connection.ExecuteAsync(query, parameters);
                    Dispose(connection, command);
                }
                catch (Exception) {
                    Dispose(connection, command);
                    throw;
                }
            }
        }

        public void TransactionalExecute(string query, object? parameters = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    connection.Open();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    connection.Execute(query, parameters, transaction);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
        }

        public async Task TransactionalExecuteAsync(string query, object? parameters = null) {
            using (SqlConnection connection = new SqlConnection(connectionConfig)) {
                SqlCommand? command = null;
                SqlTransaction? transaction = null;
                try {
                    await connection.OpenAsync();
                    command = connection.CreateCommand();
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    await connection.ExecuteAsync(query, parameters, transaction);
                    transaction.Commit();
                    Dispose(connection, command, transaction);
                }
                catch (Exception) {
                    transaction?.Rollback();
                    Dispose(connection, command, transaction);
                    throw;
                }
            }
        }

        public void BulkInsert<T>(string targetTable, IEnumerable<T> data) {
            using (var connection = new SqlConnection(connectionConfig + ";Connection Timeout=" + maxTimeOut.ToString())) {
                connection.Open();
                SqlTransaction? transaction = connection.BeginTransaction();
                try {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)) {
                        bulkCopy.BulkCopyTimeout = maxTimeOut;
                        bulkCopy.DestinationTableName = targetTable;
                        bulkCopy.WriteToServer(data.ToDataTable());
                        transaction.Commit();
                    }
                }
                catch (Exception e) {
                    transaction?.Rollback();
                    throw;
                }
            }
        }

        public async Task BulkInsertAsync<T>(string targetTable, IEnumerable<T> data) {
            using (var connection = new SqlConnection(connectionConfig + ";Connection Timeout=" + maxTimeOut.ToString())) {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction()) {
                    try {
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)) {
                            bulkCopy.BulkCopyTimeout = maxTimeOut;
                            bulkCopy.DestinationTableName = targetTable;
                            await bulkCopy.WriteToServerAsync(data.ToDataTable());
                            await transaction.CommitAsync();
                        }
                    }
                    catch (SqlException e) {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public void BulkInsertTransaction<T>(string targetTable, IEnumerable<T> data) {
            var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, transaction);
            bulkCopy.BulkCopyTimeout = maxTimeOut;
            bulkCopy.DestinationTableName = targetTable;
            bulkCopy.WriteToServer(data.ToDataTable());
        }

        public async Task BulkInsertTransactionAsync<T>(string targetTable, IEnumerable<T> data) {
            var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, transaction);
            bulkCopy.BulkCopyTimeout = maxTimeOut;
            bulkCopy.DestinationTableName = targetTable;
            await bulkCopy.WriteToServerAsync(data.ToDataTable());
        }
    }
}