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

                string resourceTableName = $"[Resource].[PFA_{Guid.NewGuid().ToString("N")}]";

                ISqlBulkCopyParameters bulkParams = new SqlBulkCopyParameters()
                {
                    BatchSize = 1000,
                    DestinationTableName = resourceTableName
                };

                IDBContext workspaceDbContext = _helper.GetDBContext(workspaceArtifactId);

                DataTableReader dataTableReader = new DataTableReader(inputTable);

                string sql = $@"
                        IF Object_id(N'{ resourceTableName }', N'U') IS NULL
                          BEGIN
                              SET ANSI_NULLS ON
                              SET QUOTED_IDENTIFIER ON

                              CREATE TABLE { resourceTableName }
                                (
                                   [DocumentArtifactID] [int] NOT NULL UNIQUE,
                                   [Status]             [int] NOT NULL,
                                   [Started]        [datetime] NULL
                                )
                              ON [PRIMARY]

                              CREATE UNIQUE CLUSTERED INDEX PK_DocumentArtifactID
                                ON { resourceTableName }(DocumentArtifactID)
                                WITH IGNORE_DUP_KEY
                          END";

                ContextQuery query = new ContextQuery()
                {
                    SqlStatement = sql
                };

                await workspaceDbContext.ExecuteNonQueryAsync(query);

                CancellationToken token = new CancellationToken();

                await workspaceDbContext.ExecuteBulkCopyAsync(dataTableReader, bulkParams, token);

                sql = $@"
                        MERGE [ProcessingFieldOtherMetadataQueue] AS Target
                        USING { resourceTableName } AS Source
                        ON Source.[DocumentArtifactID] = Target.[DocumentArtifactID]
                        WHEN NOT MATCHED BY Target THEN
                          INSERT ([DocumentArtifactID],
                                  [Status],
                                  [Started])
                          VALUES (Source.[DocumentArtifactID],
                                  Source.[Status],
                                  Source.[Started])
                        WHEN MATCHED THEN
                          UPDATE SET Target.[Status] = Source.[Status],
                                     Target.[Started] = Source.[Started];";


                query = new ContextQuery()
                {
                    SqlStatement = sql
                };

                await workspaceDbContext.ExecuteNonQueryAsync(query);

                sql = $@"
                        IF Object_id(N'{ resourceTableName }', N'U') IS NOT NULL
                          BEGIN
                              DROP TABLE { resourceTableName }
                          END";

                query = new ContextQuery()
                {
                    SqlStatement = sql
                };

                await workspaceDbContext.ExecuteNonQueryAsync(query);



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

        /// <summary>
        /// All Kepler services must inherit from IDisposable.
        /// Use this dispose method to dispose of any unmanaged memory at this point.
        /// See https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose for examples of how to properly use the dispose pattern.
        /// </summary>
        public void Dispose()
        { }
    }
}
