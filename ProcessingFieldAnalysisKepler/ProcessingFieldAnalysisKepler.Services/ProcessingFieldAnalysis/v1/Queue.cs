using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.API.Context;
using Relativity.Kepler.Logging;
using Relativity.Services.Exceptions;
using ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1;
using ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1.Exceptions;
using ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1.Models;
using System.Data;

namespace ProcessingFieldAnalysisKepler.Services.ProcessingFieldAnalysis.v1
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

        public async Task<QueueModel> EnableProcessingFieldObjectMaintenance(int workspaceId)
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
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = $"Processing Field Object Maintenance enabled for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not enable Processing Field Object Maintenance for Workspace: {WorkspaceId}.", workspaceId);
                throw new QueueException($"Could not enable Processing Field Object Maintenance for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<QueueModel> DisableProcessingFieldObjectMaintenance(int workspaceId)
        {
            QueueModel model;

            try
            {
                string workspaceName = await _helper.GetDBContext(-1).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = @"
                            UPDATE [ProcessingFieldManagerQueue]
                            SET    [ProcessingFieldObjectMaintEnabled] = 0
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = $"Processing Field Object Maintenance disabled for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not disable Processing Field Object Maintenance for Workspace: {WorkspaceID}.", workspaceId);
                throw new QueueException($"Could not disable Processing Field Object Maintenance for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<QueueModel> ForceProcessingFieldObjectMaintenance(int workspaceId)
        {
            QueueModel model;

            try
            {
                string workspaceName = await _helper.GetDBContext(-1).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = @"
                            UPDATE [ProcessingFieldManagerQueue]
                            SET    [ProcessingFieldObjectMaintLastRun] = NULL
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = $"Processing Field Object Maintenance has been forced for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not force Processing Field Object Maintenance for Workspace: {WorkspaceID}.", workspaceId);
                throw new QueueException($"Could not force Processing Field Object Maintenance for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<QueueModel> IsProcessingFieldObjectMaintenanceEnabled(int workspaceId)
        {
            QueueModel model;

            try
            {
                bool isEnabled = await _helper.GetDBContext(-1).ExecuteScalarAsync<bool>(new ContextQuery()
                {
                    SqlStatement = @"
                            SELECT [ProcessingFieldObjectMaintEnabled]
                            FROM   [ProcessingFieldManagerQueue]
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactId",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = (isEnabled) ? $"Processing Field Object Maintenance is enabled for Workspace: {workspaceId}" : $"Processing Field Object Maintenance is disabled for Workspace: {workspaceId}",
                    IsEnabled = isEnabled
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not check if Processing Field Object Maintenance is enabled for Workspace: {WorkspaceID}.", workspaceId);
                throw new QueueException($"Could not check if Processing Field Object Maintenance is enabled for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<QueueModel> EnableOtherMetadataAnalysis(int workspaceId)
        {
            QueueModel model;

            try
            {
                string workspaceName = await _helper.GetDBContext(-1).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = @"
                            UPDATE [ProcessingFieldManagerQueue]
                            SET    [OtherMetadataAnalysisEnabled] = 1
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = $"Other Metadata Analysis enabled for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not enable Other Metadata Analysis for Workspace: {WorkspaceId}.", workspaceId);
                throw new QueueException($"Could not enable Other Metadata Analysis for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<QueueModel> DisableOtherMetadataAnalysis(int workspaceId)
        {
            QueueModel model;

            try
            {
                string workspaceName = await _helper.GetDBContext(-1).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = @"
                            UPDATE [ProcessingFieldManagerQueue]
                            SET    [OtherMetadataAnalysisEnabled] = 0
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = $"Other Metadata Analysis disabled for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not disable Other Metadata Analysis for Workspace: {WorkspaceId}.", workspaceId);
                throw new QueueException($"Could not disable Other Metadata Analysis for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }

        public async Task<QueueModel> ForceOtherMetadataAnalysis(int workspaceId)
        {
            QueueModel model;

            try
            {
                string workspaceName = await _helper.GetDBContext(-1).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = @"
                            UPDATE [ProcessingFieldManagerQueue]
                            SET    [OtherMetadataAnalysisLastRun] = NULL
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = $"Other Metadata Analysis forced for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not force Other Metadata Analysis for Workspace: {WorkspaceId}.", workspaceId);
                throw new QueueException($"Could not force Other Metadata Analysis for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }
            return model;
        }

        public async Task<QueueModel> ForceCustomOtherMetadataAnalysis(int workspaceId)
        {
            QueueModel model;

            try
            {
                string workspaceName = await _helper.GetDBContext(-1).ExecuteScalarAsync<string>(new ContextQuery()
                {
                    SqlStatement = @"
                            UPDATE [ProcessingFieldManagerQueue]
                            SET    [OtherMetadataAnalysisLastRun]    = NULL,
                                   [OtherMetadataAnalysisInProgress] = 1
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = $"Custom Other Metadata Analysis forced for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not force custom Other Metadata Analysis for Workspace: {WorkspaceId}.", workspaceId);
                throw new QueueException($"Could not force custom Other Metadata Analysis for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }
            return model;
        }

        public async Task<QueueModel> IsOtherMetadataAnalysisEnabled(int workspaceId)
        {
            QueueModel model;

            try
            {
                bool isEnabled = await _helper.GetDBContext(-1).ExecuteScalarAsync<bool>(new ContextQuery()
                {
                    SqlStatement = @"
                            SELECT [OtherMetadataAnalysisEnabled]
                            FROM   [ProcessingFieldManagerQueue]
                            WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactId",
                    Parameters = new List<SqlParameter>()
                    {
                        new SqlParameter()
                        {
                            ParameterName = "@WorkspaceArtifactID",
                            SqlDbType = SqlDbType.Int, Value = workspaceId
                        }
                    }
                }).ConfigureAwait(false);

                model = new QueueModel
                {
                    Message = (isEnabled) ? $"Other Metadata Analysis is enabled for Workspace: {workspaceId}" : $"Other Metadata Analysis is disabled for Workspace: {workspaceId}",
                    IsEnabled = isEnabled
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not check if Other Metadata Analysis is enabled for Workspace: {WorkspaceID}.", workspaceId);
                throw new QueueException($"Could not check if Other Metadata Analysis is enabled for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
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
