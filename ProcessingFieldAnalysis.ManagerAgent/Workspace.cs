using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class Workspace
    {
        public IHelper Helper { get; set; }
        public IAPILog Logger { get; set; }

        public Workspace (IHelper helper, IAPILog logger)
        {
            Helper = helper;
            Logger = logger;
        }
        /// <summary>
        /// Retrieves a list of Workspace Artifact IDs where the Processing Field Application is installed.
        /// Currently uses DB context, would like to replace with API.
        /// </summary>
        /// <param name="eddsDbContext"></param>
        /// <returns></returns>
        public List<int> GetWorkspaceArtifactIdsWhereApplicationIsInstalled()
        {
            try
            {
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                string sql = $@"
                    SELECT [CaseID]
                    FROM   [EDDS].[eddsdbo].[CaseApplication] C
                           JOIN [EDDS].[eddsdbo].[ApplicationInstall] A
                             ON C.[CurrentApplicationInstallID] = A.[ApplicationInstallID]
                           JOIN [EDDS].[eddsdbo].[LibraryApplication] LA
                             ON C.[ApplicationID] = LA.[ArtifactID]
                    WHERE  LA.[Guid] = @applicationGuid
                           AND [CaseID] <> -1";

                var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {Value = GlobalVariable.PROCESSING_FIELD_APPLICATION_GUID}
            };

                DataTable installedWorkspacesDataTable = eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
                List<int> installedWorkspaceArtifactIds = new List<int>();

                foreach (DataRow workspaceArtifactIdRow in installedWorkspacesDataTable.Rows)
                {
                    installedWorkspaceArtifactIds.Add((int)workspaceArtifactIdRow["CaseID"]);
                }

                return installedWorkspaceArtifactIds;
            } catch (Exception e)
            {
                Logger.LogError(e, "Error occurred getting list of Workspace Artifact IDs where the Processing Field Application is installed.");
            }
            return new List<int>();
        }
        /// <summary>
        /// Gets Artifact ID for a give Guid
        /// Uses DB context, would like to replace with API.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public int GetArtifactIdByGuid(int workspaceArtifactId, Guid guid)
        {
            string sql = $@"
                    SELECT TOP 1 [ArtifactID]
                    FROM   [EDDSDBO].[ArtifactGuid]
                    WHERE  [ArtifactGuid] = @Guid";

            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) {Value = guid}
            };
            try
            {
                IDBContext dbContext = Helper.GetDBContext(workspaceArtifactId);
                return (int)dbContext.ExecuteSqlStatementAsScalar(sql, sqlParams);
            } 
            catch(Exception e)
            {
                Logger.LogError(e, "Failed to get Artifact ID for Guid: {guid} in Workspace: {workspaceArtifactId}", guid, workspaceArtifactId);
            }
            return 0;
        }
        /// <summary>
        /// Retrieves an Object's Artifact Text Identifier by Guid
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public string GetTextIdentifierByGuid(int workspaceArtifactId, Guid guid)
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
            try
            {
                IDBContext dbContext = Helper.GetDBContext(workspaceArtifactId);
                return (string)dbContext.ExecuteSqlStatementAsScalar(sql, sqlParams);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get Artifact ID for Guid: {guid} in Workspace: {workspaceArtifactId}", guid, workspaceArtifactId);
            }
            return string.Empty;
        }
    }
}
