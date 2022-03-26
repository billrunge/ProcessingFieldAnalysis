using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingField
    {
        public async Task<MassCreateResult> CreateProcessingFieldObjects(IHelper helper, int workspaceArtifactId, IAPILog logger, IReadOnlyList<FieldRef> fields, IReadOnlyList<IReadOnlyList<object>> fieldValues)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    if (fieldValues.Count > 0)
                    {
                        var massCreateRequest = new MassCreateRequest
                        {
                            ObjectType = new ObjectTypeRef { Guid = GlobalVariable.PROCESSING_FIELD_OBJECT },
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
        public async Task<List<string>> GetProcessingFieldNamesAsync(IHelper helper, int workspaceArtifactId, IAPILog logger)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    var queryRequest = new QueryRequest()
                    {
                        Fields = new List<FieldRef> {
                        new FieldRef { Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD }
                    },
                        ObjectType = new ObjectTypeRef { Guid = GlobalVariable.PROCESSING_FIELD_OBJECT }
                    };

                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 1, 1000);

                    List<string> output = new List<string>();

                    foreach (RelativityObject resultObject in queryResult.Objects)
                    {
                        FieldValuePair fieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD];

                        output.Add(fieldPair.Value.ToString());
                    }

                    return output;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Unable to get a list of Processing Field Names from the Workspace Processing Field Object");
                }
            }
            return null;
        }
    }
}
