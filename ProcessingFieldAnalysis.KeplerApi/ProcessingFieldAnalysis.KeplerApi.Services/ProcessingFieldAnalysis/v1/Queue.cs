using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.API.Context;
using Relativity.Kepler.Logging;
using Relativity.Services.Exceptions;
using ProcessingFieldAnalysisApi.Interfaces.ProcessingFieldAnalysis.v1;
using ProcessingFieldAnalysisApi.Interfaces.ProcessingFieldAnalysis.v1.Exceptions;
using ProcessingFieldAnalysisApi.Interfaces.ProcessingFieldAnalysis.v1.Models;
using System.Data;

namespace ProcessingFieldAnalysisApi.Services.ProcessingFieldAnalysis.v1
{
    public class Queue : IQueue
    {
        private IHelper _helper;
        private ILog _logger;

        // Note: IHelper and ILog are dependency injected into the constructor every time the service is called.
        public Queue(IHelper helper, ILog logger)
        {
            // Note: Set the logging context to the current class.
            _logger = logger.ForContext<Queue>();
            _helper = helper;
        }

        public async Task<QueueModel> EnableProcessingFieldObjectMaintenance(int workspaceID)
        {
            QueueModel model;

            try
            {
                string workspaceName = await _helper.GetDBContext(-1).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = @"
                            UPDATE [ProcessingFieldManagerQueue]
                            SET    [ProcessingFieldObjectMaintEnabled] = 1
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID",
                    Parameters = new List<SqlParameter>() 
                    { 
                        new SqlParameter() 
                        { 
                            ParameterName = "@WorkspaceArtifactID", 
                            SqlDbType = SqlDbType.Int, Value = workspaceID 
                        } 
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Name = workspaceName
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not enable Processing Field Object Maintenance for Workspace: {WorkspaceID}.", workspaceID);
                throw new QueueException($"Could not enable Processing Field Object Maintenance for Workspace: {workspaceID}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceID}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<List<QueueModel>> QueryWorkspaceByNameAsync(string queryString, int limit)
        {
            var models = new List<QueueModel>();

            // Create a Kepler service proxy to interact with other Kepler services.
            // Use the dependency injected IHelper to create a proxy to an external service.
            // This proxy will execute as the currently logged in user. (ExecutionIdentity.CurrentUser)
            // Note: If calling methods within the same service the proxy is not needed. It is doing so
            //       in this example only as a demonstration of how to call other services.
            var proxy = _helper.GetServicesManager().CreateProxy<IQueue>(ExecutionIdentity.CurrentUser);

            // Validate queryString and throw a ValidationException (HttpStatusCode 400) if the string does not meet the validation requirements.
            if (string.IsNullOrEmpty(queryString) || queryString.Length > 50)
            {
                // ValidationException is in the namespace Relativity.Services.Exceptions and found in the Relativity.Kepler.dll.
                throw new ValidationException($"{nameof(queryString)} cannot be empty or grater than 50 characters.");
            }

            try
            {
                // Use the dependency injected IHelper to get a database connection.
                // In this example a query is made for all workspaces that are like the query string.
                // Note: async/await and ConfigureAwait(false) is used when making calls external to the service.
                //       See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
                //       See also https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.configureawait
                //       See also https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
                //       See also https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
                //       Warning: Improper use of the tasks can cause deadlocks and performance issues within an application.
                var workspaceIDs = await _helper.GetDBContext(-1).ExecuteEnumerableAsync(
                    new ContextQuery
                    {
                        SqlStatement = @"SELECT TOP (@limit) [ArtifactID] FROM [Case] WHERE [ArtifactID] > 0 AND [Name] LIKE '%'+@workspaceName+'%'",
                        Parameters = new[]
                        {
                            new SqlParameter("@limit", limit),
                            new SqlParameter("@workspaceName", queryString)
                        }
                    }, (record, cancel) => Task.FromResult(record.GetInt32(0))).ConfigureAwait(false);

                foreach (int workspaceID in workspaceIDs)
                {
                    // Loop through the results and use the proxy to call another service for more information.
                    // Note: async/await and ConfigureAwait(false) is used when making calls external to the service.
                    //       See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
                    //       See also https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.configureawait
                    //       See also https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/
                    //       See also https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
                    //       Warning: Improper use of the tasks can cause deadlocks and performance issues within an application.
                    QueueModel wsModel = await proxy.EnableProcessingFieldObjectMaintenance(workspaceID).ConfigureAwait(false);
                    if (wsModel != null)
                    {
                        models.Add(wsModel);
                    }
                }
            }
            catch (Exception exception)
            {
                // Note: logging templates should never use interpolation! Doing so will cause memory leaks. 
                _logger.LogWarning(exception, "An exception occured during query for workspace(s) containing {QueryString}.", queryString);

                // Throwing a user defined exception with a 404 status code.
                throw new QueueException($"An exception occured during query for workspace(s) containing {queryString}.");
            }

            return models;
        }

        /// <summary>
        /// All Kepler services must inherit from IDisposable.
        /// Use this dispose method to dispose of any unmanaged memory at this point.
        /// See https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose for examples of how to properly use the dispose pattern.
        /// </summary>
        public void Dispose()
        { }
    }
}
