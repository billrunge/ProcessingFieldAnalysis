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
        public async Task PopulateProcessingFieldObjectAsync(IHelper helper, int workspaceArtifactId, MappableSourceField[] mappableSourceFields, List<MappableField> existingProcessingFieldObjects, IAPILog logger)
        {
            //helpers
            //InvariantField invariantField = new InvariantField();
            Choice choice = new Choice();
            ProcessingField processingField = new ProcessingField();
            Field field = new Field();
            Hash hash = new Hash();

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
        public async Task UpdateProcessingFieldObjectAsync(IHelper helper, int workspaceArtifactId, MappableSourceField[] mappableSourceFields, List<MappableField> existingProcessingFields, IAPILog logger)
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
                foreach (MappableField mappableField in existingProcessingFields)
                {
                    RelativityObjectRef relativityObjectRef = new RelativityObjectRef() { ArtifactID = mappableField.ArtifactId };
                    MappableSourceField mappableSourceField = mappableSourceFields.SingleOrDefault(x => x.SourceName == mappableField.SourceName);
                    List<FieldRefValuePair> fieldRefValuePairs = new List<FieldRefValuePair>();

                    if (mappableField.Category != mappableSourceField.Category)
                    {
                        logger.LogDebug("Categories {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.Category, mappableSourceField.Category, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair() { 
                            Field = new FieldRef 
                            { 
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD 
                            }, 
                            Value = await choice.GetSingleChoiceChoiceRefByNameAsync(helper, workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD, mappableSourceField.Category, logger) 
                        });
                    }

                    if (mappableField.DataType != mappableSourceField.DataType)
                    {
                        logger.LogDebug("Data Types {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.DataType, mappableSourceField.DataType, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD
                            },
                            Value = await choice.GetSingleChoiceChoiceRefByNameAsync(helper, workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD, mappableSourceField.DataType, logger)
                        });
                    }

                    if (NormalizeFieldValues(mappableField.Description) != NormalizeFieldValues(mappableSourceField.Description))
                    {
                        logger.LogDebug("Descriptions are not equal for Processing Field: {artifactId}", mappableField.ArtifactId);
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
                        logger.LogDebug("Friendly names {0} and {1} are not equal for Processing Field: {artifactId}", NormalizeFieldValues(mappableField.FriendlyName), NormalizeFieldValues(mappableSourceField.FriendlyName), mappableField.ArtifactId);
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
                            logger.LogDebug("Mapped Fields {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.MappedFields, sourceMappedFields, mappableField.ArtifactId);
                            fieldRefValuePairs.Add(new FieldRefValuePair()
                            {
                                Field = new FieldRef
                                {
                                    Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD
                                },
                                Value = await choice.GetMultipleChoiceRefsByNameAsync(helper, workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, sourceMappedFields, logger)
                            });
                        }

                    }
                    else if (mappableField.MappedFields.Length > 0)
                    {
                        logger.LogDebug("Mapped Fields {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.MappedFields, sourceMappedFields, mappableField.ArtifactId);
                        fieldRefValuePairs.Add(new FieldRefValuePair()
                        {
                            Field = new FieldRef
                            {
                                Guid = GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD
                            },
                            Value = await choice.GetMultipleChoiceRefsByNameAsync(helper, workspaceArtifactId, GlobalVariable.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD, sourceMappedFields, logger)
                        });
                    }

                    if (mappableField.MinimumLength != mappableSourceField.Length)
                    {
                        logger.LogDebug("Minimum Lengths {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.MinimumLength, mappableSourceField.Length, mappableField.ArtifactId);
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
                        logger.LogDebug("Source names {0} and {1} are not equal for Processing Field: {artifactId}", mappableField.SourceName, mappableSourceField.SourceName, mappableField.ArtifactId);
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
                        await processingField.UpdateProcessingFieldObject(helper, workspaceArtifactId, relativityObjectRef, fieldRefValuePairs, logger);
                    }

                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while updating the Processing Field Object");
            }
        }

        public string NormalizeFieldValues(string input)
        {
            if (input == @"")
            {
                return null;
            }
            return input;
        }
    }
}
