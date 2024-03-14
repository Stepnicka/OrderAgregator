using Microsoft.Data.Sqlite;

namespace OrderAgregator.API.Cache.SqLiteCache
{
    internal static class Initializer
    {
        private static bool initialized = false;
        private static readonly object lockingObject = new();

        public static SqliteConnection Connect(string connectionString, bool rebuildOnInitialize)
        {
            if (initialized == true)
                return new SqliteConnection(connectionString);

            lock (lockingObject)
            {
                if (initialized == false)
                    return Initialize(connectionString, rebuildOnInitialize);
            }

            return new SqliteConnection(connectionString);
        }

        private static SqliteConnection Initialize(string connectionString, bool rebuildOnInitialize)
        {
            var connection = new SqliteConnection(connectionString);

            try
            {
                connection.Open();

                SetTables(connection, rebuildOnInitialize);

                initialized = true;
            }
            finally
            {
                connection.Close();
            }

            return connection;
        }

        private static void SetTables(SqliteConnection connection, bool rebuildOnInitialize)
        {
            using (var command = connection.CreateCommand())
            {
                if (rebuildOnInitialize)
                {
                    command.CommandText = @"DROP TABLE IF EXISTS [Order];";
                    command.ExecuteNonQuery();
                }

                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS [Order]
                    (
                        [Id]            INTEGER     NOT NULL    PRIMARY KEY  AUTOINCREMENT,
	                    [ProductId]		INTEGER		NOT NULL,
	                    [Quantity]		INTEGER		NOT NULL,
                        [State]         INTEGER     NOT NULL
                    );

                    CREATE INDEX IF NOT EXISTS [IDX_Order_state] ON [Order]([State]);
                ";

                command.ExecuteNonQuery();
            }
        }
    }
}
