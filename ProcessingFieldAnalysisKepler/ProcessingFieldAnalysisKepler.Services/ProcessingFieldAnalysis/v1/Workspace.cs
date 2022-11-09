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
using Newtonsoft.Json;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Processing.V1.Services;
using Relativity.Processing.V1.Services.Interfaces.DTOs;
using System.Threading;

namespace ProcessingFieldAnalysisKepler.Services.ProcessingFieldAnalysis.v1
{
    public class Workspace : IWorkspace
    {
        private IHelper _helper;
        private ILog _logger;

        // Note: IHelper and ILog are dependency injected into the constructor every time the service is called.
        public Workspace(IHelper helper, ILog logger)
        {
            // Note: Set the logging context to the current class.
            _logger = logger.ForContext<Queue>();
            _helper = helper;
        }

        public async Task<bool> DoesWorkspaceQueueTableExist(int workspaceArtifactId)
        {
            try
            {
                IDBContext workspaceDbContext = _helper.GetDBContext(workspaceArtifactId);

                string sql = @"
                        IF Object_id(N'[ProcessingFieldOtherMetadataQueue]', N'U') IS NULL
                          BEGIN
                              SELECT 0
                          END
                        ELSE
                          BEGIN
                              SELECT 1
                          END";

                ContextQuery query = new ContextQuery() 
                { 
                    SqlStatement = sql                
                };

                int result = await workspaceDbContext.ExecuteScalarAsync<int>(query);

                return (result > 0);

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when checking if he workspace queue table exists in Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
            return false;
        }

        public async Task CreateWorkspaceQueueTable(int workspaceArtifactId)
        {
            try
            {
                IDBContext workspaceDbContext = _helper.GetDBContext(workspaceArtifactId);

                string sql = @"
                        IF Object_id(N'[ProcessingFieldOtherMetadataQueue]', N'U') IS NULL
                          BEGIN
                              SET ANSI_NULLS ON
                              SET QUOTED_IDENTIFIER ON

                              CREATE TABLE [ProcessingFieldOtherMetadataQueue]
                                (
                                   [DocumentArtifactID] [int] NOT NULL UNIQUE,
                                   [Status]             [int] NOT NULL,
                                   [Started]        [datetime] NULL
                                )
                              ON [PRIMARY]

                              CREATE UNIQUE CLUSTERED INDEX PK_DocumentArtifactID
                                ON ProcessingFieldOtherMetadataQueue(DocumentArtifactID)
                                WITH IGNORE_DUP_KEY
                          END";
                ContextQuery query = new ContextQuery() 
                { 
                SqlStatement = sql
                };

                await workspaceDbContext.ExecuteNonQueryAsync(query);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while creating the [ProcessingFieldOtherMetadataQueue] table in the EDDS{workspaceArtifactID} database", workspaceArtifactId);
            }
        }

        public async Task InsertIntoWorkspaceQueueTableAsync(int workspaceArtifactId, List<int> documentArtifactIds)
        {
            try
            {
                    DataTable inputTable = new DataTable();

                    inputTable.Columns.Add("DocumentArtifactID", typeof(int));
                    inputTable.Columns.Add("Status", typeof(int));
                    inputTable.Columns.Add("Started", typeof(DateTime));

                    foreach (int documentArtifactId in documentArtifactIds)
                    {
                        DataRow row = inputTable.NewRow();
                        row["DocumentArtifactID"] = documentArtifactId;
                        row["Status"] = 0;
                        row["Started"] = DBNull.Value;
                        inputTable.Rows.Add(row);
                    }

                    ISqlBulkCopyParameters bulkParams = new SqlBulkCopyParameters()
                    {
                        BatchSize = 1000,
                        DestinationTableName = "ProcessingFieldOtherMetadataQueue"
                    };

                    IDBContext workspaceDbContext = _helper.GetDBContext(workspaceArtifactId);

                    DataTableReader dataTableReader = new DataTableReader(inputTable);

                    CancellationToken token = new CancellationToken();

                    await workspaceDbContext.ExecuteBulkCopyAsync(dataTableReader, bulkParams, token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred while populating the [ProcessingFieldOtherMetadataQueue] table in the EDDS{workspaceArtifactID} database", workspaceArtifactId);
            }
        }

        public async Task<int> GetArtifactIdByGuid(int workspaceArtifactId, Guid guid)
        {
            string sql = $@"
                    SELECT TOP 1 [ArtifactID]
                    FROM   [EDDSDBO].[ArtifactGuid]
                    WHERE  [ArtifactGuid] = @Guid";

            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) {Value = guid}
            };

            ContextQuery query = new ContextQuery()
            {
                Parameters = sqlParams,
                SqlStatement = sql
            };

            try
            {
                IDBContext dbContext = _helper.GetDBContext(workspaceArtifactId);
                return await dbContext.ExecuteScalarAsync<int>(query);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get Artifact ID for Guid: {guid} in Workspace: {workspaceArtifactId}", guid, workspaceArtifactId);
            }
            return 0;
        }

        public async Task<string> GetTextIdentifierByGuid(int workspaceArtifactId, Guid guid)
        {
            string sql = $@"
                    SELECT [TextIdentifier]
                    FROM   [Artifact] A
                           JOIN [ArtifactGuid] AG
                             ON A.[ArtifactID] = AG.[ArtifactID]
                    WHERE  AG.[ArtifactGuid] = @Guid";

            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) {Value = guid}
            };

            ContextQuery query = new ContextQuery()
            {
                Parameters = sqlParams,
                SqlStatement = sql
            };

            try
            {
                IDBContext dbContext = _helper.GetDBContext(workspaceArtifactId);
                return await dbContext.ExecuteScalarAsync<string>(query);
            }
            catch (Exception e)
            {
               _logger.LogError(e, "Failed to get Artifact ID for Guid: {guid} in Workspace: {workspaceArtifactId}", guid, workspaceArtifactId);
            }
            return string.Empty;
        }


        //public async Task<PublishModel> PublishFiles(List<long> documentArtifactIds, int workspaceId)
        //{
        //    PublishModel model;

        //    Guid processingFileIdFieldGuid = new Guid("93E1CFEB-F21E-4386-ADC3-846066525FE8");
        //    List<long> processingFileIds = new List<long>();

        //    try
        //    {
        //        var queryRequest = new Relativity.ObjectManager.V1.Models.QueryRequest()
        //        {
        //            ObjectType = new Relativity.ObjectManager.V1.Models.ObjectTypeRef { ArtifactTypeID = 10 },
        //            Condition = $"('Artifact ID' IN [{ string.Join(",", documentArtifactIds) }])",
        //            Fields = new List<Relativity.ObjectManager.V1.Models.FieldRef>()
        //            {
        //            new Relativity.ObjectManager.V1.Models.FieldRef { Guid = processingFileIdFieldGuid }
        //            }
        //        };

        //        using (Relativity.ObjectManager.V1.Interfaces.IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<Relativity.ObjectManager.V1.Interfaces.IObjectManager>(ExecutionIdentity.System))
        //        {
        //            Relativity.ObjectManager.V1.Models.QueryResult queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, 1, 1000);

        //            foreach (Relativity.ObjectManager.V1.Models.RelativityObject result in queryResult.Objects)
        //            {
        //                Relativity.ObjectManager.V1.Models.FieldValuePair documentFieldPair = result[processingFileIdFieldGuid];

        //                bool parseSuccessful = long.TryParse(documentFieldPair.Value.ToString(), out long fileId);

        //                if (parseSuccessful)
        //                {
        //                    processingFileIds.Add(fileId);
        //                    _logger.LogError("Parsing fileId: {fileId}", fileId);
        //                }
        //                else
        //                {
        //                    _logger.LogError("There was an issue parsing the file ids.");
        //                }
        //            }
        //        }



        //        using (IProcessingDocumentManager proxy = _helper.GetServicesManager().CreateProxy<IProcessingDocumentManager>(ExecutionIdentity.CurrentUser))
        //        {
        //            ProcessingDocumentsRequest request = new ProcessingDocumentsRequest()
        //            {
        //                Expression = "{\"Type\":\"ConditionalExpression\",\"Property\":\"IsDeleted\",\"Constraint\":\"Is\",\"Value\":false}",
        //                ProcessingFileIDs = processingFileIds
        //            };

        //            await proxy.PublishDocumentsAsync(workspaceId, request);

        //        }
        //        model = new PublishModel
        //        {
        //            Message = $"Processing Field Object Maintenance enabled for Workspace: {workspaceId}"
        //        };
        //    }
        //    catch (Exception exception)
        //    {
        //        _logger.LogError(exception, "Could not enable Processing Field Object Maintenance for Workspace: {WorkspaceId}.", workspaceId);
        //        throw new QueueException($"Could not enable Processing Field Object Maintenance for Workspace: {workspaceId}.")
        //        {
        //            FaultSafeObject = new QueueException.FaultSafeInfo()
        //            {
        //                Information = $"Workspace {workspaceId}",
        //                Time = DateTime.Now
        //            }
        //        };
        //    }

        //    return model;
        //}


        /// <summary>
        /// All Kepler services must inherit from IDisposable.
        /// Use this dispose method to dispose of any unmanaged memory at this point.
        /// See https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose for examples of how to properly use the dispose pattern.
        /// </summary>
        public void Dispose()
        { }
    }
}
