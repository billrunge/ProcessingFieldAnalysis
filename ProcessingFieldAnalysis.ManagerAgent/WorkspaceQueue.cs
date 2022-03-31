using Newtonsoft.Json;
using Relativity.API;
using Relativity.API.Context;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class WorkspaceQueue
    {
        IHelper Helper { get; set; }
        IAPILog Logger { get; set; }

        public WorkspaceQueue(IHelper helper, IAPILog logger)
        {
            Helper = helper;
            Logger = logger;
        }

        public void CreateWorkspaceQueueTable(int workspaceArtifactId)
        {
            try
            {
                IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

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

                workspaceDbContext.ExecuteNonQuerySQLStatement(sql);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while creating the [ProcessingFieldOtherMetadataQueue] table in the EDDS{workspaceArtifactID} database", workspaceArtifactId);
            }
        }

        public async Task PopulateWorkspaceQueueTableAsync(int workspaceArtifactId, int batchSize, int requestStart = 1)
        {
            try
            {
                using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
                {
                    Workspace workspace = new Workspace(Helper, Logger);
                    string otherMetadataFieldName = workspace.GetTextIdentifierByGuid(workspaceArtifactId, GlobalVariable.DOCUMENT_OBJECT_OTHER_METADATA_FIELD);

                    var queryRequest = new QueryRequest()
                    {
                        ObjectType = new ObjectTypeRef { Guid = GlobalVariable.DOCUMENT_OBJECT },
                        Condition = $"'{otherMetadataFieldName}' ISSET"
                    };

                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, requestStart, batchSize);

                    if (queryResult.TotalCount == batchSize)
                    {
                        await PopulateWorkspaceQueueTableAsync(workspaceArtifactId, batchSize, requestStart + batchSize);
                    }

                    DataTable inputTable = new DataTable();

                    inputTable.Columns.Add("DocumentArtifactID", typeof(int));
                    inputTable.Columns.Add("Status", typeof(int));
                    inputTable.Columns.Add("Started", typeof(DateTime));

                    foreach (RelativityObject result in queryResult.Objects)
                    {
                        DataRow row = inputTable.NewRow();
                        row["DocumentArtifactID"] = result.ArtifactID;
                        row["Status"] = 0;
                        row["Started"] = DBNull.Value;
                        inputTable.Rows.Add(row);
                    }

                    ISqlBulkCopyParameters bulkParams = new SqlBulkCopyParameters() 
                    { 
                        BatchSize = batchSize,
                        DestinationTableName = "ProcessingFieldOtherMetadataQueue"                        
                    };

                    IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

                    DataTableReader dataTableReader = new DataTableReader(inputTable);

                    CancellationToken token = new CancellationToken();                    

                    await workspaceDbContext.ExecuteBulkCopyAsync(dataTableReader, bulkParams, token);
                }


            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while populating the [ProcessingFieldOtherMetadataQueue] table in the EDDS{workspaceArtifactID} database", workspaceArtifactId);
            }
        }

        public bool DoesWorkspaceQueueTableExist(int workspaceArtifactId)
        {
            try
            {
                IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

                string sql = @"
                        IF Object_id(N'[ProcessingFieldOtherMetadataQueue]', N'U') IS NULL
                          BEGIN
                              SELECT 0
                          END
                        ELSE
                          BEGIN
                              SELECT 1
                          END";

                return (bool)workspaceDbContext.ExecuteSqlStatementAsScalar(sql);
            }
            catch(Exception e)
            {
                Logger.LogError(e, "Error when checking if he workspace queue table exists in Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
            return false;
        }

        public List<int> CheckOutBatchOfDocumentArtifactIds(int workspaceArtifactId)
        {
            try
            {
                IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

                string sql = @"
                        UPDATE TOP (@BatchSize) [ProcessingFieldOtherMetadataQueue]
                        SET    [DocumentArtifactID] = [DocumentArtifactID],
                               [Status]  = 1,
                               [Started] = Getutcdate() 
                        OUTPUT INSERTED.[DocumentArtifactID]
                        WHERE  [Status] = 0";
                
                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@BatchSize", SqlDbType.Int) {Value = GlobalVariable.OTHER_METADATA_FIELD_PARSING_BATCH_SIZE}
                };

                DataTable results = workspaceDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);

                List<int> output = new List<int>();

                foreach (DataRow row in results.Rows)
                {
                    output.Add((int)row["DocumentArtifactID"]);
                }
                return output;

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting batch of Documents to run Other Metadata analysis on Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
            return new List<int>();
        }

        //UNTESTED
        public void CheckInBatchOfDocumentArtifactIds(int workspaceArtifactId, List<int> documentArtifactIds)
        {
            try
            {
                IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

                string artifactIds = JsonConvert.SerializeObject(documentArtifactIds).Trim('[').Trim(']');

                string sql = $@"
                        UPDATE [ProcessingFieldOtherMetadataQueue]
                        SET    [Status] = 2,
                               [Started] = NULL
                        WHERE  [DocumentArtifactID] IN ( {artifactIds} )";

                DataTable results = workspaceDbContext.ExecuteSqlStatementAsDataTable(sql);

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error checking in a batch of Documents after running Other Metadata analysis on Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
        }

        //NOT FINISHED
        public bool IsWorkComplete(int workspaceArtifactId)
        {
            try
            {
                bool output = false;
                IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

                string artifactIds = JsonConvert.SerializeObject(documentArtifactIds).Trim('[').Trim(']');

                string sql = $@"
                        UPDATE [ProcessingFieldOtherMetadataQueue]
                        SET    [Status] = 2,
                               [Started] = NULL
                        WHERE  [DocumentArtifactID] IN ( {artifactIds} )";

                DataTable results = workspaceDbContext.ExecuteSqlStatementAsDataTable(sql);

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error checking in a batch of Documents after running Other Metadata analysis on Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
            return false;
        }



    }
}
