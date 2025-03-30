using static Dapper.SqlMapper;

namespace Core.DataSources {

    public interface IDataSource {
        string connectionConfig { get; set; }

        void BeginTransaction();

        void CommitTransaction();

        void CreateConnection(string connectionConfig);

        void Dispose();

        IList<T> ExecuteDapper<T>(string query, object parameters = null, int? timeOut = null);

        Task<IList<T>> ExecuteDapperAsync<T>(string query, object parameters = null, int? timeOut = null);

        T ExecuteDapperScalar<T>(string query, object parameters = null, int? timeOut = null);

        Task<T> ExecuteDapperScalarAsync<T>(string query, object parameters = null, int? timeOut = null);

        Task<T?> ExecuteDapperScalarNullableAsync<T>(string query, object? parameters = null, int? timeOut = null);

        GridReader ExecuteDapperMultipleQuery(string query, object? parameters = null, int? timeOut = null);

        Task<GridReader> ExecuteDapperMultipleQueryAsync(string query, object parameters = null, int? timeOut = null);

        void ExecuteQuery(string query);

        Task ExecuteQueryAsync(string query);

        void RollbackTransaction();

        IList<T> Select<T>(string query, object parameters = null, int? timeOut = null);

        Task<IList<T>> SelectAsync<T>(string query, object? parameters = null, int? timeOut = null);

        T? SelectScalar<T>(string query, object parameters = null, int? timeOut = null);

        Task<T?> SelectScalarAsync<T>(string query, object parameters = null, int? timeOut = null);

        GridReader SelectMultipleQuery(string query, object? parameters = null, int? timeOut = null);

        Task<GridReader> SelectMultipleQueryAsync(string query, object? parameters = null, int? timeOut = null);

        T TransactionalQuery<T>(string query, object? parameters = null, int? timeOut = null);

        T? TransactionalQueryScalar<T>(string query, object? parameters = null, int? timeOut = null);

        Task<T> TransactionalQueryAsync<T>(string query, object? parameters = null, int? timeOut = null);

        Task<T?> TransactionalQueryScalarAsync<T>(string query, object? parameters = null, int? timeOut = null);

        GridReader TransactionalMultipleQuery(string query, object? parameters = null, int? timeOut = null);

        Task<GridReader> TransactionalMultipleQueryAsync(string query, object? parameters = null, int? timeOut = null);

        void Execute(string query, object? parameters = null);

        void ExecuteConnection(string query, object parameters = null, int? timeOut = null);

        Task ExecuteAsync(string query, object? parameters = null);

        Task ExecuteConnectionAsync(string query, object parameters = null, int? timeOut = null);

        void TransactionalExecute(string query, object? parameters = null);

        Task TransactionalExecuteAsync(string query, object? parameters = null);

        void BulkInsert<T>(string targetTable, IEnumerable<T> data);

        Task BulkInsertAsync<T>(string targetTable, IEnumerable<T> data);

        void BulkInsertTransaction<T>(string targetTable, IEnumerable<T> data);

        Task BulkInsertTransactionAsync<T>(string targetTable, IEnumerable<T> data);
    }
}