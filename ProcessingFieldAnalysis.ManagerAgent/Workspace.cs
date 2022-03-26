using Relativity.API;
using Relativity.ObjectModel.V1.Choice;
using Relativity.ObjectModel.V1.Choice.Models;
using Relativity.Shared.V1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class Workspace
    {
        public DataTable RetrieveApplicationWorkspaces(IDBContext eddsDbContext)
        {
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
                new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {Value = GlobalVariables.PROCESSING_FIELD_APPLICATION_GUID}
            };

            return eddsDbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
        }

        public int GetArtifactIdByGuid(IHelper helper, int workspaceArtifactId, Guid guid)
        {
            string sql = $@"
                    SELECT TOP 1 [ArtifactID]
                    FROM   [EDDSDBO].[ArtifactGuid]
                    WHERE  [ArtifactGuid] = @Guid";

            SqlParameter[] sqlParams = new SqlParameter[]
            {
                new SqlParameter("@Guid", SqlDbType.UniqueIdentifier) {Value = guid}
            };

            IDBContext dbContext = helper.GetDBContext(workspaceArtifactId);

            return (int)dbContext.ExecuteSqlStatementAsScalar(sql, sqlParams);
        }

    }
}
