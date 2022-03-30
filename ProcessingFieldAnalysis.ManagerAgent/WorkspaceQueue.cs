using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                                   [DocumentArtifactID] [int] NOT NULL,
                                   [Status]             [int] NOT NULL,
                                   [LastUpdated]        [datetime] NULL
                                )
                              ON [PRIMARY]
                          END";

                workspaceDbContext.ExecuteNonQuerySQLStatement(sql);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while populating the [ProcessingFieldManagerQueue] table in the EDDS database");
            }
        }



    }
}
