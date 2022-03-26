using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public async Task PopulateProcessingFieldAsync(IHelper helper, IAPILog logger)
        {
            List<FieldRef> fields = new List<FieldRef>()
                {
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_HASH_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_FRIENDLY_NAME_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_DESCRIPTION_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_MINIMUM_LENGTH_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD }
                };

            //helpers
            InvariantField invariantField = new InvariantField();
            ProcessingFieldObject processingFieldObject = new ProcessingFieldObject();
            Workspace appWorkspace = new Workspace();
            Choice choice = new Choice();
            ProcessingField processingField = new ProcessingField();

            DataTable installedWorkspaceArtifactIds = appWorkspace.RetrieveApplicationWorkspaces(helper.GetDBContext(-1));

            foreach (DataRow workspaceArtifactIdRow in installedWorkspaceArtifactIds.Rows)
            {
                int workspaceArtifactId = (int)workspaceArtifactIdRow["CaseID"];

                MappableSourceField[] mappableSourceFields = await invariantField.GetInvariantFieldsAsync(helper, workspaceArtifactId, logger);
                List<string> existingProcessingFields = await processingFieldObject.GetProcessingFieldNameListFromWorkspace(helper, workspaceArtifactId, logger);

                List<IReadOnlyList<object>> fieldValues = new List<IReadOnlyList<object>>();
                foreach (MappableSourceField mappableSourceField in mappableSourceFields)
                {
                    if (!existingProcessingFields.Contains(mappableSourceField.SourceName))
                    {
                        Dictionary<string, int> existingCategoryChoices = processingFieldObject.GetFieldChoiceNames(helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD, logger);
                        Dictionary<string, int> existingDataTypeChoices = processingFieldObject.GetFieldChoiceNames(helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD, logger);
                        Dictionary<string, int> existingMappedFieldsChoices = processingFieldObject.GetFieldChoiceNames(helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, logger);

                        int categoryChoiceArtifactId;
                        int dataTypeChoiceArtifactId;
                        List<ChoiceRef> mappedFieldsChoiceRefs = new List<ChoiceRef>();

                        if (existingDataTypeChoices.ContainsKey(mappableSourceField.DataType))
                        {
                            existingDataTypeChoices.TryGetValue(mappableSourceField.DataType, out dataTypeChoiceArtifactId);
                        }
                        else
                        {
                            dataTypeChoiceArtifactId = await choice.CreateChoiceAsync(helper, workspaceArtifactId, mappableSourceField.DataType, GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD);
                        }

                        if (existingCategoryChoices.ContainsKey(mappableSourceField.Category))
                        {
                            existingCategoryChoices.TryGetValue(mappableSourceField.Category, out categoryChoiceArtifactId);
                        }
                        else
                        {
                            categoryChoiceArtifactId = await choice.CreateChoiceAsync(helper, workspaceArtifactId, mappableSourceField.Category, GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD);
                        }

                        if (mappableSourceField.MappedFields != null)
                        {
                            foreach (string mappedField in mappableSourceField.MappedFields)
                            {
                                int mappedFieldArtifactId;
                                if (existingMappedFieldsChoices.ContainsKey(mappedField))
                                {
                                    existingMappedFieldsChoices.TryGetValue(mappedField, out mappedFieldArtifactId);
                                }
                                else
                                {
                                    mappedFieldArtifactId = await choice.CreateChoiceAsync(helper, workspaceArtifactId, mappedField, GlobalVariables.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD);
                                }

                                mappedFieldsChoiceRefs.Add(new ChoiceRef { ArtifactID = mappedFieldArtifactId });
                            }
                        }

                        IReadOnlyList<object> fieldValue = new List<object>()
                            {
                                ComputeHashString(mappableSourceField.SourceName),
                                mappableSourceField.FriendlyName,
                                new ChoiceRef { ArtifactID = categoryChoiceArtifactId },
                                mappableSourceField.Description,
                                mappableSourceField.Length,
                                new ChoiceRef { ArtifactID = dataTypeChoiceArtifactId },
                                mappableSourceField.SourceName,
                                mappedFieldsChoiceRefs
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
