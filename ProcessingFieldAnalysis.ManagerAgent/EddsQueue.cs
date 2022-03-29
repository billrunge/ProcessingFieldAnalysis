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
                                   [WorkspaceArtifactID]               [int] NOT NULL,
                                   [ProcessingFieldObjectMaintLastRun] [datetime] NULL,
                                   [ProcessingFieldObjectMaintEnabled] [bit] NOT NULL,
                                   [OtherMetadataAnalysisLastRun]      [datetime] NULL,
                                   [OtherMetadataAnalysisEnabled]      [bit] NOT NULL
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
                               NULL,
                               0
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


    }
}
