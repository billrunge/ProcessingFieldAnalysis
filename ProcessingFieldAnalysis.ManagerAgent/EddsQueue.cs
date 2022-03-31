using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class EddsQueue
    {
        IHelper Helper { get; set; }
        IAPILog Logger { get; set; }

        public EddsQueue(IHelper helper, IAPILog logger)
        {
            Helper = helper;
            Logger = logger;
        }

        public void CreateProcessingFieldManagerQueueTable()
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        IF Object_id(N'[EDDS].[eddsdbo].[ProcessingFieldManagerQueue]', N'U') IS NULL
                          BEGIN
                              SET ANSI_NULLS ON
                              SET QUOTED_IDENTIFIER ON

                              CREATE TABLE [eddsdbo].[ProcessingFieldManagerQueue]
                                (
                                   [WorkspaceArtifactID]                    [int] NOT NULL,
                                   [ProcessingFieldObjectMaintLastRun]      [datetime] NULL,
                                   [ProcessingFieldObjectMaintEnabled]      [bit] NOT NULL,
                                   [ProcessingFieldObjectMaintInProgress]   [bit] NOT NULL,
                                   [ProcessingFieldObjectMaintStartTime]    [datetime] NULL,
                                   [OtherMetadataAnalysisLastRun]           [datetime] NULL,
                                   [OtherMetadataAnalysisEnabled]           [bit] NOT NULL,
                                   [OtherMetadataAnalysisInProgress]        [bit] NOT NULL,
                                   [OtherMetadataAnalysisStartTime]         [datetime] NULL
                                )
                              ON [PRIMARY]

                              ALTER TABLE [eddsdbo].[ProcessingFieldManagerQueue]
                                ADD CONSTRAINT
                                [DF_ProcessingFieldManagerQueue_ProcessingFieldObjectMaintEnabled]
                                DEFAULT
                                ((0
                                )) FOR [ProcessingFieldObjectMaintEnabled]

                              ALTER TABLE [eddsdbo].[ProcessingFieldManagerQueue]
                                ADD CONSTRAINT
                                [DF_ProcessingFieldManagerQueue_OtherMetadataAnalysisEnabled]
                                DEFAULT ((0)) FOR [OtherMetadataAnalysisEnabled]
                          END";


                eddsDbContext.ExecuteNonQuerySQLStatement(sql);

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while creating the [ProcessingFieldManagerQueue] table in the EDDS database");
            }
        }

        public void PopulateProcessingFieldManagerQueueTable()
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        INSERT INTO [ProcessingFieldManagerQueue]
                        SELECT [CaseID],
                               NULL,
                               0,
                               0,
                               NULL,
                               NULL,
                               0,
                               0,
                               NULL
                        FROM   [CaseApplication] C
                               JOIN [ApplicationInstall] A
                                 ON C.[CurrentApplicationInstallID] = A.[ApplicationInstallID]
                               JOIN [LibraryApplication] LA
                                 ON C.[ApplicationID] = LA.[ArtifactID]
                        WHERE  LA.[Guid] = @ApplicationGuid
                               AND [CaseID] <> -1
                               AND [CaseID] NOT IN (SELECT [WorkspaceArtifactID]
                                                    FROM   [ProcessingFieldManagerQueue])";

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@ApplicationGuid", SqlDbType.UniqueIdentifier) {Value = GlobalVariable.PROCESSING_FIELD_APPLICATION_GUID}
                };

                eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while populating the [ProcessingFieldManagerQueue] table in the EDDS database");
            }
        }

        public List<int> GetListOfWorkspaceArtifactIdsToManageProcessingFields()
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        SELECT [WorkspaceArtifactID]
                        FROM   [ProcessingFieldManagerQueue]
                        WHERE  [ProcessingFieldOBjectMaintEnabled] = 1
                        AND    [ProcessingFieldObjectMaintInProgress] = 0
                               AND ( [ProcessingFieldObjectMaintLastRun] IS NULL
                                      OR Datediff(hour, [ProcessingFieldObjectMaintLastRun],
                                         Getutcdate()) >
                                         @HourlyInterval )";

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@HourlyInterval", SqlDbType.Int) {Value = GlobalVariable.PROCESSING_FIELD_MAINTENANCE_HOURLY_INTERVAL}
                };

                DataTable results = eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
                List<int> workspaceArtifactIdList = new List<int>();

                foreach(DataRow row in results.Rows)
                {
                    workspaceArtifactIdList.Add((int)row["WorkspaceArtifactID"]);
                }
                return workspaceArtifactIdList;
            }
            catch(Exception e)
            {
                Logger.LogError(e, "Error occurred while getting a list of Workspace Artifact IDs to perform Processing Field Magagent");
            }
            return new List<int>();
        }

        public List<int> GetListOfWorkspaceArtifactIdsToAnalyzeOtherMetadata()
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        SELECT [WorkspaceArtifactID]
                        FROM   [ProcessingFieldManagerQueue]
                        WHERE  [OtherMetadataAnalysisEnabled] = 1
                        AND  ( [OtherMetadataAnalysisLastRun] IS NULL
                                      OR Datediff(hour, [OtherMetadataAnalysisLastRun], Getutcdate()) >
                                         @HourlyInterval ) ";

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@HourlyInterval", SqlDbType.Int) {Value = GlobalVariable.OTHER_METADATA_ANALYSIS_HOURLY_INTERVAL}
                };

                DataTable results = eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
                List<int> workspaceArtifactIdList = new List<int>();

                foreach (DataRow row in results.Rows)
                {
                    workspaceArtifactIdList.Add((int)row["WorkspaceArtifactID"]);
                }
                return workspaceArtifactIdList;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while getting a list of Workspace Artifact IDs to perform Other Metadata analysis");
            }
            return new List<int>();
        }

        public bool StartProcessingFieldObjectMaintenance(int workspaceArtifactId)
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        UPDATE [EDDS].[eddsdbo].[ProcessingFieldManagerQueue]
                        SET    [ProcessingFieldObjectMaintInProgress] = 1,
                               [ProcessingFieldObjectMaintStartTime] = Getutcdate()
                        WHERE  [ProcessingFieldObjectMaintInProgress] <> 1
                               AND [WorkspaceArtifactID] = @WorkspaceArtifactID

                        SELECT @@ROWCOUNT AS [HaveLock]";

                bool output = false;

                 if ((int)eddsDbContext.ExecuteSqlStatementAsScalar(sql, new SqlParameter("@WorkspaceArtifactID", SqlDbType.Int) { Value = workspaceArtifactId }) > 0)
                {
                    output = true;
                }
                return output;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while setting the [ProcessingFieldObjectMaintInProgress] column to true in the [EDDS].[eddsdbo].[ProcessingFieldManagerQueue] for Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
            return false;
        }

        public void EndProcessingFieldObjectMaintenance(int workspaceArtifactId)
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        UPDATE [EDDS].[eddsdbo].[ProcessingFieldManagerQueue]
                        SET    [ProcessingFieldObjectMaintInProgress] = 0,
                               [ProcessingFieldObjectMaintLastRun]    = Getutcdate(),
                               [ProcessingFieldObjectMaintStartTime]  = NULL
                        WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID";

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@WorkspaceArtifactID", SqlDbType.Int) { Value = workspaceArtifactId }
                };

                eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while ending Processing Field Object Maintenance for Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
        }

        public bool StartOtherMetadataAnalysis(int workspaceArtifactId)
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        UPDATE [EDDS].[eddsdbo].[ProcessingFieldManagerQueue]
                        SET    [OtherMetadataAnalysisInProgress] = 1,
                               [OtherMetadataAnalysisStartTime]  = Getutcdate()
                        WHERE  [OtherMetadataAnalysisInProgress] <> 1
                               AND [WorkspaceArtifactID] = @WorkspaceArtifactID

                        SELECT @@ROWCOUNT AS [HaveLock]";

                bool output = false;

                if ((int)eddsDbContext.ExecuteSqlStatementAsScalar(sql, new SqlParameter("@WorkspaceArtifactID", SqlDbType.Int) { Value = workspaceArtifactId }) > 0)
                {
                    output = true;
                }
                return output;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while setting the [OtherMetadataAnalysisInProgress] column to true in the [EDDS].[eddsdbo].[ProcessingFieldManagerQueue] for Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
            return false;
        }

        public void EndOtherMetadataAnalysis(int workspaceArtifactId)
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = @"
                        UPDATE [EDDS].[eddsdbo].[ProcessingFieldManagerQueue]
                        SET    [OtherMetadataAnalysisInProgress] = 0,
                               [OtherMetadataAnalysisLastRun]    = Getutcdate(),
                               [OtherMetadataAnalysisStartTime] = NULL
                        WHERE  [WorkspaceArtifactID] = @WorkspaceArtifactID";

                var sqlParams = new List<SqlParameter>
                {
                    new SqlParameter("@WorkspaceArtifactID", SqlDbType.Int) { Value = workspaceArtifactId }
                };

                eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while ending Other Metadata Analysis for Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
        }
    }
}
