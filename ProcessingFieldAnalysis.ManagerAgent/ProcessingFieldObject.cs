using Relativity.API;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingFieldObject
    {
        /// <summary>
        /// This method populates the Processing Field Object by checking what Source Names exist 
        /// in the Processing Field Object for a given Workspace and creates any that are missing
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task PopulateProcessingFieldObjectAsync(IHelper helper, int workspaceArtifactId, MappableSourceField[] mappableSourceFields, List<MappableSourceField> existingProcessingFields, IAPILog logger)
        {
            //helpers
            //InvariantField invariantField = new InvariantField();
            Choice choice = new Choice();
            ProcessingField processingField = new ProcessingField();
            Field field = new Field();
            Hash hash = new Hash();

            List<FieldRef> fields = field.fields;
            List<string> existingProcessingFieldSourceNames = existingProcessingFields.Select(x => x.SourceName).ToList();

            try
            {
                List<IReadOnlyList<object>> fieldValues = new List<IReadOnlyList<object>>();
                foreach (MappableSourceField mappableSourceField in mappableSourceFields)
                {
                    if (!existingProcessingFieldSourceNames.Contains(mappableSourceField.SourceName))
                    {
                        IReadOnlyList<object> fieldValue = new List<object>()
                            {
                                hash.GetHash(mappableSourceField.SourceName, logger),
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
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while populating the Processing Field Object");
            }
        }

        public async Task UpdateProcessingFieldObjectAsync(IHelper helper, IAPILog logger)
        {
            //helpers
            InvariantField invariantField = new InvariantField();
            Workspace workspace = new Workspace();
            Choice choice = new Choice();
            ProcessingField processingField = new ProcessingField();
            Field field = new Field();
            Hash hash = new Hash();

            List<FieldRef> fields = field.fields;

            try
            {
                List<int> installedWorkspaceArtifactIds = workspace.GetWorkspaceArtifactIdsWhereApplicationIsInstalled(helper.GetDBContext(-1), logger);

                foreach (int workspaceArtifactId in installedWorkspaceArtifactIds)
                {

                    MappableSourceField[] mappableSourceFields = await invariantField.GetInvariantFieldsAsync(helper, workspaceArtifactId, logger);
                    List<MappableSourceField> existingProcessingFields = await processingField.GetProcessingFieldObjectMappableSourceFieldsAsync(helper, workspaceArtifactId, logger);
                    List<string> existingProcessingFieldSourceNames = existingProcessingFields.Select(x => x.SourceName).ToList();

                    List<IReadOnlyList<object>> fieldValues = new List<IReadOnlyList<object>>();
                    foreach (MappableSourceField mappableSourceField in mappableSourceFields)
                    {
                        if (!existingProcessingFieldSourceNames.Contains(mappableSourceField.SourceName))
                        {
                            IReadOnlyList<object> fieldValue = new List<object>()
                            {
                                hash.GetHash(mappableSourceField.SourceName, logger),
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
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while updating the Processing Field Object");
            }
        }
    }
}
