using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingFieldObject
    {
        public async Task<Relativity.ObjectManager.V1.Models.MassCreateResult> CreateProcessingFieldObjects(IHelper helper, int workspaceArtifactId, IAPILog logger, IReadOnlyList<Relativity.ObjectManager.V1.Models.FieldRef> fields, IReadOnlyList<IReadOnlyList<object>> fieldValues)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    if (fieldValues.Count > 0)
                    {
                        var massCreateRequest = new Relativity.ObjectManager.V1.Models.MassCreateRequest
                        {
                            ObjectType = new Relativity.ObjectManager.V1.Models.ObjectTypeRef { Guid = GlobalVariables.PROCESSING_FIELD_OBJECT },
                            Fields = fields,
                            ValueLists = fieldValues
                        };

                        return await objectManager.CreateAsync(workspaceArtifactId, massCreateRequest);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "The Relativity Object could not be created.");
                }

            }
            return null;
        }

        public async Task<List<string>> GetProcessingFieldNameListFromWorkspace(IHelper helper, int workspaceArtifactId, IAPILog logger)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    var queryRequest = new QueryRequest()
                    {
                        Fields = new List<FieldRef> {
                        new FieldRef { Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD }
                    },
                        ObjectType = new Relativity.ObjectManager.V1.Models.ObjectTypeRef { Guid = GlobalVariables.PROCESSING_FIELD_OBJECT }
                    };

                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 1, 1000);

                    List<string> output = new List<string>();

                    foreach (RelativityObject resultObject in queryResult.Objects)
                    {
                        FieldValuePair fieldPair = resultObject[GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD];
                        
                        output.Add(fieldPair.Value.ToString());
                    }

                    return output;
                }
                catch(Exception exception)
                {
                    logger.LogError(exception, "Unable to get a list of Processing Field Names from the Workspace Processing Field Object");
                }
            }
            return null;
        }

        public Dictionary<string,int> GetFieldChoiceNames(IHelper helper, int workspaceArtifactId, Guid fieldGuid, IAPILog logger)
        {
            string sql = $@"
                SELECT C.[ArtifactID],
                       C.[Name]
                FROM   [Code] C
                       JOIN [Field] F
                         ON F.[CodeTypeID] = C.[CodeTypeID]
                       JOIN [ArtifactGuid] A
                         ON A.[ArtifactID] = F.[ArtifactID]
                WHERE  A.[ArtifactGuid] = @FieldGuid";

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@FieldGuid", SqlDbType.UniqueIdentifier) {Value = fieldGuid}
            };

            IDBContext dbContext = helper.GetDBContext(workspaceArtifactId);
            DataTable resultDataTable = dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
            Dictionary<string, int> choices = new Dictionary<string, int>();
            foreach (DataRow row in resultDataTable.Rows)
            {
                choices.Add((string)row["Name"],(int)row["ArtifactID"]);
            }
            return choices;
        }
    }
}
