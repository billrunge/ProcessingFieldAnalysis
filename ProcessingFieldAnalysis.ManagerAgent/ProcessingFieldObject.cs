using Relativity.API;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingFieldObject
    {
        public async Task PopulateProcessingFieldObjectAsync(IHelper helper, IAPILog logger)
        {
            //helpers
            InvariantField invariantField = new InvariantField();
            Workspace workspace = new Workspace();
            Choice choice = new Choice();
            ProcessingField processingField = new ProcessingField();
            Field field = new Field();

            List<FieldRef> fields = field.fields;

            List<int> installedWorkspaceArtifactIds = workspace.GetWorkspaceArtifactIdsWhereApplicationIsInstalled(helper.GetDBContext(-1));

            foreach (int workspaceArtifactId in installedWorkspaceArtifactIds)
            {

                MappableSourceField[] mappableSourceFields = await invariantField.GetInvariantFieldsAsync(helper, workspaceArtifactId, logger);
                List<string> existingProcessingFields = await processingField.GetProcessingFieldNamesAsync(helper, workspaceArtifactId, logger);

                List<IReadOnlyList<object>> fieldValues = new List<IReadOnlyList<object>>();
                foreach (MappableSourceField mappableSourceField in mappableSourceFields)
                {
                    if (!existingProcessingFields.Contains(mappableSourceField.SourceName))
                    {              
                        IReadOnlyList<object> fieldValue = new List<object>()
                            {
                                ComputeHashString(mappableSourceField.SourceName),
                                mappableSourceField.FriendlyName,
                                await choice.GetSingleChoiceChoiceRefByNameAsync(helper, workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD, mappableSourceField.Category, logger),
                                mappableSourceField.Description,
                                mappableSourceField.Length,
                                await choice.GetSingleChoiceChoiceRefByNameAsync(helper, workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD, mappableSourceField.DataType, logger),
                                mappableSourceField.SourceName,
                                await choice.GetMultipleChoiceRefsByNameAsync(helper, workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, mappableSourceField.MappedFields, logger)
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
