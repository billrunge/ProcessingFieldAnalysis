using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingField
    {
        public IHelper Helper { get; set; }
        public IAPILog Logger { get; set; }

        public ProcessingField (IHelper helper, IAPILog logger)
        {
            Helper = helper;
            Logger = logger;
        }
        /// <summary>
        /// This method populates the Processing Field Object by checking what Source Names exist 
        /// in the Processing Field Object for a given Workspace and creates any that are missing
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task PopulateProcessingFieldObjectAsync(int workspaceArtifactId, MappableSourceField[] mappableSourceFields, List<MappableField> existingProcessingFieldObjects)
        {
            ProcessingFieldObjectChoice choice = new ProcessingFieldObjectChoice(Helper, Logger);
            Field field = new Field();
            Hash hash = new Hash(Logger);

            List<FieldRef> fields = field.fields;
            List<string> existingProcessingFieldObjectSourceNames = existingProcessingFieldObjects.Select(x => x.SourceName).ToList();

            try
            {
                List<IReadOnlyList<object>> fieldValues = new List<IReadOnlyList<object>>();
                foreach (MappableSourceField mappableSourceField in mappableSourceFields)
                {
                    if (!existingProcessingFieldObjectSourceNames.Contains(mappableSourceField.SourceName))
                    {
                        IReadOnlyList<object> fieldValue = new List<object>()
                            {
                                hash.GetHash(mappableSourceField.SourceName),
                                mappableSourceField.FriendlyName,
                                await choice.GetSingleChoiceChoiceRefByNameAsync(workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD, mappableSourceField.Category),
                                mappableSourceField.Description,
                                mappableSourceField.Length,
                                await choice.GetSingleChoiceChoiceRefByNameAsync(workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD, mappableSourceField.DataType),
                                mappableSourceField.SourceName,
                                await choice.GetMultipleChoiceRefsByNameAsync(workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, mappableSourceField.MappedFields)
                            };
                        fieldValues.Add(fieldValue);
                    }
                }
                await CreateProcessingFieldObjects(workspaceArtifactId, fields, fieldValues);

            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while populating the Processing Field Object");
            }
        }
        /// <summary>
        /// This method checks all of the MappableFields returned by the Invariant Field Mapping API to MappableFields returned by Object Manager
        /// from the Processing Field Object in the Workspace and compares each applicable property to check if any of the properties are different.
        /// If so, it will update the applicable Processing Field OBject in the workspace
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="mappableSourceFields"></param>
        /// <param name="existingProcessingFields"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task UpdateProcessingFieldObjectsAsync(int workspaceArtifactId, MappableSourceField[] mappableSourceFields, List<MappableField> existingProcessingFields)
        {
            ProcessingFieldObjectChoice choice = new ProcessingFieldObjectChoice(Helper, Logger);
            Field field = new Field();
            List<FieldRef> fields = field.fields;

            try
            {
                foreach (MappableField mappableField in existingProcessingFields)
                {
                    RelativityObjectRef relativityObjectRef = new RelativityObjectRef() { ArtifactID = mappableField.ArtifactId };
                    MappableSourceField mappableSourceField = mappableSourceFields.SingleOrDefault(x => x.SourceName == mappableField.SourceName);
                    List<FieldRefValuePair> fieldRefValuePairs = new List<FieldRefValuePair>();

                    if (mappableField.Category != mappableSourceField.Category)
                    {
                        Logger.LogDebug("Categories {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.Category, mappableSourceField.Category, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD
                            },
                            Value = await choice.GetSingleChoiceChoiceRefByNameAsync(workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD, mappableSourceField.Category)
                        });
                    }

                    if (mappableField.DataType != mappableSourceField.DataType)
                    {
                        Logger.LogDebug("Data Types {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.DataType, mappableSourceField.DataType, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD
                            },
                            Value = await choice.GetSingleChoiceChoiceRefByNameAsync(workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD, mappableSourceField.DataType)
                        });
                    }

                    if (NormalizeFieldValues(mappableField.Description) != NormalizeFieldValues(mappableSourceField.Description))
                    {
                        Logger.LogDebug("Descriptions are not equal for Processing Field: {artifactId}", mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_DESCRIPTION_FIELD
                            },
                            Value = mappableSourceField.Description
                        });
                    }

                    if (NormalizeFieldValues(mappableField.FriendlyName) != NormalizeFieldValues(mappableSourceField.FriendlyName))
                    {
                        Logger.LogDebug("Friendly names {0} and {1} are not equal for Processing Field: {artifactId}", NormalizeFieldValues(mappableField.FriendlyName), NormalizeFieldValues(mappableSourceField.FriendlyName), mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_FRIENDLY_NAME_FIELD
                            },
                            Value = mappableSourceField.FriendlyName
                        });
                    }

                    string[] sourceMappedFields = mappableSourceField.MappedFields ?? (new string[] { });

                    if (sourceMappedFields.Length > 0)
                    {
                        if (!mappableField.MappedFields.SequenceEqual(sourceMappedFields))
                        {
                            Logger.LogDebug("Mapped Fields {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.MappedFields, sourceMappedFields, mappableField.ArtifactId);
                            fieldRefValuePairs.Add(new FieldRefValuePair()
                            {
                                Field = new FieldRef
                                {
                                    Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD
                                },
                                Value = await choice.GetMultipleChoiceRefsByNameAsync(workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, sourceMappedFields)
                            });
                        }

                    }
                    else if (mappableField.MappedFields.Length > 0)
                    {
                        Logger.LogDebug("Mapped Fields {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.MappedFields, sourceMappedFields, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD
                            },
                            Value = await choice.GetMultipleChoiceRefsByNameAsync(workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, sourceMappedFields)
                        });
                    }

                    if (mappableField.MinimumLength != mappableSourceField.Length)
                    {
                        Logger.LogDebug("Minimum Lengths {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.MinimumLength, mappableSourceField.Length, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_MINIMUM_LENGTH_FIELD
                            },
                            Value = mappableSourceField.Length
                        });
                    }

                    if (mappableField.SourceName != mappableSourceField.SourceName)
                    {
                        Logger.LogDebug("Source names {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.SourceName, mappableSourceField.SourceName, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD
                            },
                            Value = mappableSourceField.SourceName
                        });
                    }

                    if (fieldRefValuePairs.Count > 0)
                    {
                        await UpdateProcessingFieldObjectAsync(workspaceArtifactId, relativityObjectRef, fieldRefValuePairs);
                    }

                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while updating the Processing Field Object");
            }

            string NormalizeFieldValues(string input)
            {
                if (input == @"")
                {
                    return null;
                }
                return input;
            }
        }
        /// <summary>
        /// Gets a list of Mappable Source Field Objects that corresponds with the current instances of the Processing Field Object for a given Workspace.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <returns>List<string></returns>
        public async Task<List<MappableField>> GetProcessingFieldObjectMappableFieldsAsync(int workspaceArtifactId)
        {
            using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
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

                    List<MappableField> output = new List<MappableField>();

                    foreach (RelativityObject resultObject in queryResult.Objects)
                    {
                        FieldValuePair sourceNameFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD];
                        FieldValuePair friendlyNameFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_FRIENDLY_NAME_FIELD];
                        FieldValuePair categoryFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD];
                        FieldValuePair descriptionFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_DESCRIPTION_FIELD];
                        FieldValuePair minimumLengthFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_MINIMUM_LENGTH_FIELD];
                        FieldValuePair dataTypeFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD];
                        FieldValuePair mappedFieldsFieldPair = resultObject[GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD];

                        Relativity.ObjectManager.V1.Models.Choice categoryChoice = new Relativity.ObjectManager.V1.Models.Choice();


                        if (categoryFieldPair.Value != null)
                        {
                            categoryChoice = (Relativity.ObjectManager.V1.Models.Choice)categoryFieldPair.Value;
                        }

                        Relativity.ObjectManager.V1.Models.Choice dataTypeChoice = new Relativity.ObjectManager.V1.Models.Choice();

                        if (dataTypeFieldPair != null)
                        {
                            dataTypeChoice = (Relativity.ObjectManager.V1.Models.Choice)dataTypeFieldPair.Value;
                        }

                        List<Relativity.ObjectManager.V1.Models.Choice> mappedFields = new List<Relativity.ObjectManager.V1.Models.Choice>();

                        if (mappedFieldsFieldPair.Value != null)
                        {
                            mappedFields = (List<Relativity.ObjectManager.V1.Models.Choice>)mappedFieldsFieldPair.Value;
                        }

                        string[] mappedFieldNames = new string[0];

                        if (mappedFields.Count > 0)
                        {
                            mappedFieldNames = mappedFields.Select(x => x.Name).ToArray();
                        }


                        MappableField mappableField = new MappableField
                        {
                            ArtifactId = resultObject.ArtifactID,
                            SourceName = Convert.ToString(sourceNameFieldPair.Value),
                            FriendlyName = Convert.ToString(friendlyNameFieldPair.Value),
                            Category = categoryChoice.Name,
                            Description = Convert.ToString(descriptionFieldPair.Value),
                            MinimumLength = Convert.ToInt32(minimumLengthFieldPair.Value),
                            DataType = dataTypeChoice.Name,
                            MappedFields = mappedFieldNames
                        };

                        output.Add(mappableField);
                    }

                    return output;
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception, "Unable to get a list of Source Names from the Proccesing Field Object in Workspace: {workspaceArtifactId}", workspaceArtifactId);
                }
            }
            return new List<MappableField>();
        }
        /// <summary>
        /// Takes a list of FieldRefValuePairs and a ref to an instance of the ProcessingFieldObject and updates it based on the FieldRefValuePair input list
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <param name="fields"></param>
        /// <param name="fieldValues"></param>
        /// <returns>MassCreateResult</returns>
        async Task<UpdateResult> UpdateProcessingFieldObjectAsync(int workspaceArtifactId, RelativityObjectRef relativityObject, List<FieldRefValuePair> fieldValuePairs)
        {
            using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    var updateRequest = new UpdateRequest
                    {
                        Object = relativityObject,
                        FieldValues = fieldValuePairs
                    };

                    return await objectManager.UpdateAsync(workspaceArtifactId, updateRequest);
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception, "The Relativity Object is not valid for updating.");
                }
            }
            return new UpdateResult();
        }        
        /// <summary>
        /// Takes a list of FieldRefs and a list of fieldValues and mass creates instances of the Processing Field Object based on these lists.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <param name="fields"></param>
        /// <param name="fieldValues"></param>
        /// <returns>MassCreateResult</returns>
        async Task<MassCreateResult> CreateProcessingFieldObjects(int workspaceArtifactId, IReadOnlyList<FieldRef> fields, IReadOnlyList<IReadOnlyList<object>> fieldValues)
        {
            using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    if (fieldValues.Count > 0)
                    {
                        if (fieldValues.Count <= GlobalVariable.PROCESSING_FIELD_OBJECT_MASS_CREATE_BATCH_SIZE)
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
                            List<List<object>> batchfieldValues = new List<List<object>>();
                            int loopCount = 0;

                            foreach (List<object> fieldValue in fieldValues)
                            {
                                batchfieldValues.Add(fieldValue);
                                loopCount += 1;

                                if (batchfieldValues.Count >= GlobalVariable.PROCESSING_FIELD_OBJECT_MASS_CREATE_BATCH_SIZE)
                                {
                                    var massCreateRequest = new MassCreateRequest
                                    {
                                        ObjectType = new ObjectTypeRef { Guid = GlobalVariable.PROCESSING_FIELD_OBJECT },
                                        Fields = fields,
                                        ValueLists = batchfieldValues
                                    };
                                    await objectManager.CreateAsync(workspaceArtifactId, massCreateRequest);

                                    batchfieldValues = new List<List<object>>();
                                }
                                if (loopCount >= fieldValues.Count && batchfieldValues.Count > 0)
                                {
                                    var massCreateRequest = new MassCreateRequest
                                    {
                                        ObjectType = new ObjectTypeRef { Guid = GlobalVariable.PROCESSING_FIELD_OBJECT },
                                        Fields = fields,
                                        ValueLists = batchfieldValues
                                    };
                                    return await objectManager.CreateAsync(workspaceArtifactId, massCreateRequest);                        
                                } else if (loopCount >= fieldValues.Count)
                                {
                                    return new MassCreateResult();
                                }

                            }
                        } 
                    }
                    else
                    {
                        return new MassCreateResult();
                    }
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception, "Failed to mass create instances of the Processing Field Object in Workspace: {workspaceArtifactId}", workspaceArtifactId);
                }

            }
            return new MassCreateResult();
        }
    }
}
