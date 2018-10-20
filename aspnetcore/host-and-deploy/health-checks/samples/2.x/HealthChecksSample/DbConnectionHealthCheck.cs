﻿using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SampleApp
{
    // This sample supports specifying a query to run as a boolean test to indicate whether the database is responding.
    //
    // Choose a query that returns quickly or else the query runs the risk of overloading the database.
    //
    // In most cases, running a test query isn't necessary. If you find it necessary, choose a simple query such as 'SELECT 1'.

    public abstract class DbConnectionHealthCheck : IHealthCheck
    {
        protected DbConnectionHealthCheck(string connectionString)
            : this(connectionString, testQuery: null)
        {
        }

        protected DbConnectionHealthCheck(string connectionString, 
            string testQuery)
        {
            ConnectionString = connectionString ?? 
                throw new ArgumentNullException(nameof(connectionString));
            TestQuery = testQuery;
        }

        protected string ConnectionString { get; }

        protected string TestQuery { get; }

        protected abstract DbConnection CreateConnection(string connectionString);

        #region snippet1
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var connection = CreateConnection(ConnectionString))
            {
                try
                {
                    await connection.OpenAsync(cancellationToken);

                    if (TestQuery != null)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = TestQuery;

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                catch (DbException ex)
                {
                    return HealthCheckResult.Failed(exception: ex);
                }
            }

            return HealthCheckResult.Passed();
        }
        #endregion
    }
}