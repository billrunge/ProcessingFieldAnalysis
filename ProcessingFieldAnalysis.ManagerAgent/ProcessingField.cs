using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingField
    {
        /// <summary>
        /// Takes a list of FieldRefs and a list of fieldValues and mass creates instances of the Processing Field Object based on these lists.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <param name="fields"></param>
        /// <param name="fieldValues"></param>
        /// <returns>MassCreateResult</returns>
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
                    logger.LogError(exception, "Failed to mass create instances of the Processing Field Object in Workspace: {workspaceArtifactId}", workspaceArtifactId);
                }

            }
            return new MassCreateResult();
        }

        /// <summary>
        /// Gets a list of Mappable Source Field Objects that corresponds with the current instances of the Processing Field Object for a given Workspace.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <returns>List<string></returns>
        public async Task<List<MappableSourceField>> GetProcessingFieldObjectMappableSourceFieldsAsync(IHelper helper, int workspaceArtifactId, IAPILog logger)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    Field field = new Field();
                    var queryRequest = new QueryRequest()
                    {
                        Fields = field.fields,
                        ObjectType = new ObjectTypeRef { Guid = GlobalVariable.PROCESSING_FIELD_OBJECT }
                    };
                    
                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 1, Int32.MaxValue);

                    List<MappableSourceField> output = new List<MappableSourceField>();

                    foreach (RelativityObject resultObject in queryResult.Objects)
                    {
                        FieldValuePair sourceNameFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD];
                        FieldValuePair friendlyNameFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_FRIENDLY_NAME_FIELD];
                        FieldValuePair categoryFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD];
                        FieldValuePair descriptionFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_DESCRIPTION_FIELD];
                        FieldValuePair minimumLengthFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_MINIMUM_LENGTH_FIELD];
                        FieldValuePair dataTypeFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD];
                        FieldValuePair mappedFieldsFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD];

                        MappableSourceField mappableSourceField = new MappableSourceField
                        {
                            SourceName = sourceNameFieldPair.Value.ToString(),
                            FriendlyName = friendlyNameFieldPair.Value.ToString(),
                            Category = categoryFieldPair.Value.ToString(),
                            Description = descriptionFieldPair.Value.ToString(),
                            Length = (int)minimumLengthFieldPair.Value,
                            DataType = dataTypeFieldPair.Value.ToString(),
                            MappedFields = mappedFieldsFieldPair.Value.ToString().Split(',')
                        };


                        output.Add(mappableSourceField);
                    }

                    return output;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Unable to get a list of Source Names from the Proccesing Field Object in Workspace: {workspaceArtifactId}", workspaceArtifactId);
                }
            }
            return new List<MappableSourceField>();
        }
    }
}
