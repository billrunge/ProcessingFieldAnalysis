using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingFieldObject
    {
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
        public async Task PopulateProcessingFieldObjectAsync(IHelper helper, IAPILog logger)
        {
            //helpers
            InvariantField invariantField = new InvariantField();
            ProcessingFieldObject processingFieldObject = new ProcessingFieldObject();
            Workspace appWorkspace = new Workspace();
            Choice choice = new Choice();
            ProcessingField processingField = new ProcessingField();
            Field field = new Field();

            List<FieldRef> fields = field.fields;

            List<int> installedWorkspaceArtifactIds = appWorkspace.RetrieveWorkspacesWhereApplicationIsInstalled(helper.GetDBContext(-1));

            foreach (int workspaceArtifactId in installedWorkspaceArtifactIds)
            {

                MappableSourceField[] mappableSourceFields = await invariantField.GetInvariantFieldsAsync(helper, workspaceArtifactId, logger);
                List<string> existingProcessingFields = await processingFieldObject.GetProcessingFieldNameListFromWorkspace(helper, workspaceArtifactId, logger);

                List<IReadOnlyList<object>> fieldValues = new List<IReadOnlyList<object>>();
                foreach (MappableSourceField mappableSourceField in mappableSourceFields)
                {
                    if (!existingProcessingFields.Contains(mappableSourceField.SourceName))
                    {              
                        IReadOnlyList<object> fieldValue = new List<object>()
                            {
                                ComputeHashString(mappableSourceField.SourceName),
                                mappableSourceField.FriendlyName,
                                await choice.GetSingleChoiceChoiceRefByNameAsync(helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD, mappableSourceField.Category, logger),
                                mappableSourceField.Description,
                                mappableSourceField.Length,
                                await choice.GetSingleChoiceChoiceRefByNameAsync(helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD, mappableSourceField.DataType, logger),
                                mappableSourceField.SourceName,
                                await choice.GetMultipleChoiceRefsByNameAsync(helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, mappableSourceField.MappedFields, logger)
                            };
                        fieldValues.Add(fieldValue);
                    }
                }
                await processingField.CreateProcessingFieldObjects(helper, workspaceArtifactId, logger, fields, fieldValues);
            }
        }

        static string ComputeHashString(string rawData)
        {  
            using (SHA512 sha512Hash = SHA512.Create())
            {
                byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
