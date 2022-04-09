using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class OtherMetadata
    {
        public IAPILog Logger { get; set; }
        public IHelper Helper { get; set; }

        public OtherMetadata (IHelper helper, IAPILog logger)
        {
            Helper = helper;
            Logger = logger;
        }

        /// <summary>
        /// This method takes a list of existingProcessingFields and then uses the UpdateOtherMetadataFieldAsync method to update the Other Metadata field for Documents
        /// Linking them to Processing Fields that represent missing/unmapped metadata
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="existingProcessingFields"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task ParseOtherMetadataFieldAndLinkMissingProcessingFieldsAsync(int workspaceArtifactId, List<int>documentArtifactIds, List<MappableField> existingProcessingFields, int batchSize, int requestStart = 1)
        {
            try
            {
                List<OtherMetadataResultObject> workspaceOtherMetadataFieldList = await GetOtherMetadataListAsync(workspaceArtifactId, documentArtifactIds, requestStart, batchSize);

                foreach (OtherMetadataResultObject otherMetadataResult in workspaceOtherMetadataFieldList)
                {
                    List<int> linkedProcessingFields = new List<int>();
                    foreach (MappableField mappableField in existingProcessingFields)
                    {

                        if (otherMetadataResult.OtherMetadataFieldValue.Contains($"{mappableField.SourceName}="))
                        {
                            linkedProcessingFields.Add(mappableField.ArtifactId);
                        }
                    }

                    List<RelativityObjectValue> unmappedFieldsObjectValues = new List<RelativityObjectValue>();
                    unmappedFieldsObjectValues = otherMetadataResult.MissingMetadataFieldValue;

                    List<int> unmappedProcessingFields = new List<int>();
                    foreach (RelativityObjectValue unmappedFieldObjectValue in unmappedFieldsObjectValues)
                    {
                        unmappedProcessingFields.Add(unmappedFieldObjectValue.ArtifactID);
                    }

                    bool areEqual = Enumerable.SequenceEqual(linkedProcessingFields.OrderBy(e => e), unmappedProcessingFields.OrderBy(e => e));

                    if (linkedProcessingFields.Count > 0 && !areEqual)
                    {
                        await UpdateOtherMetadataFieldAsync(workspaceArtifactId, otherMetadataResult.ArtifactId, linkedProcessingFields);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "There was an issue parsing the Other Metadata field list retrieved from Workspace: {workspaceArtifactId}", workspaceArtifactId);
            }
        }
        /// <summary>
        /// This function retrieves a list of the OtherMetadata field from Documents in a Workspace using Object Manager
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        async Task<List<OtherMetadataResultObject>> GetOtherMetadataListAsync(int workspaceArtifactId, List<int> documentArtifactIds, int requestStart, int length)
        {
            using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    Workspace workspace = new Workspace(Helper, Logger);
                    string otherMetadataFieldName = workspace.GetTextIdentifierByGuid(workspaceArtifactId, GlobalVariable.DOCUMENT_OBJECT_OTHER_METADATA_FIELD);

                    var queryRequest = new QueryRequest()
                    {
                        Fields = new List<FieldRef>() {
                            new FieldRef{ Guid = GlobalVariable.DOCUMENT_OBJECT_OTHER_METADATA_FIELD },
                            new FieldRef{ Guid = GlobalVariable.DOCUMENT_OBJECT_UNMAPPED_METADATA_MULTI_OBJECT_FIELD }
                        },
                        ObjectType = new ObjectTypeRef { Guid = GlobalVariable.DOCUMENT_OBJECT },
                        Condition = $"'Artifact ID' IN {JsonConvert.SerializeObject(documentArtifactIds)}"
                    };

                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, requestStart, length);

                    List<OtherMetadataResultObject> otherMetadataList = new List<OtherMetadataResultObject>();

                    foreach (RelativityObject resultObject in queryResult.Objects)
                    {
                        List<RelativityObjectValue> missingMetadataFieldValues = new List<RelativityObjectValue>();

                        if (resultObject[GlobalVariable.DOCUMENT_OBJECT_UNMAPPED_METADATA_MULTI_OBJECT_FIELD].Value != null)
                        {
                            missingMetadataFieldValues = (List<RelativityObjectValue>)resultObject[GlobalVariable.DOCUMENT_OBJECT_UNMAPPED_METADATA_MULTI_OBJECT_FIELD].Value;
                        }

                        OtherMetadataResultObject otherMetadataResultObject = new OtherMetadataResultObject()
                        {
                            ArtifactId = resultObject.ArtifactID,
                            OtherMetadataFieldValue = (string)resultObject[GlobalVariable.DOCUMENT_OBJECT_OTHER_METADATA_FIELD].Value,
                            MissingMetadataFieldValue = missingMetadataFieldValues
                        };
                        otherMetadataList.Add(otherMetadataResultObject);
                    }
                    return otherMetadataList;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Unable to get a list of Other Metadata from Workspace: {workspaceArtifactId}", workspaceArtifactId);
                }
                return new List<OtherMetadataResultObject>();
            }
        }
        /// <summary>
        /// This method actually make the call to Object Manager to link Documents to the Processing Field Object via a multi-object field
        /// </summary>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="documentArtifactId"></param>
        /// <param name="linkedProcessingFields"></param>
        /// <returns></returns>
        async Task<UpdateResult> UpdateOtherMetadataFieldAsync(int workspaceArtifactId, int documentArtifactId, List<int> linkedProcessingFields)
        {
            using (IObjectManager objectManager = Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    List<RelativityObjectRef> linkedProcessingFieldObjectRefs = new List<RelativityObjectRef>();
                    foreach (int linkedProcessingFieldArtifactId in linkedProcessingFields)
                    {
                        linkedProcessingFieldObjectRefs.Add(new RelativityObjectRef { ArtifactID = linkedProcessingFieldArtifactId });
                    }
                    List<FieldRefValuePair> fieldRefValuePairs = new List<FieldRefValuePair>();
                    FieldRef processingFilesDocumentField = new FieldRef() { Guid = GlobalVariable.DOCUMENT_OBJECT_UNMAPPED_METADATA_MULTI_OBJECT_FIELD };
                    FieldRefValuePair fieldRefValuePair = new FieldRefValuePair() { Field = processingFilesDocumentField, Value = linkedProcessingFieldObjectRefs };
                    fieldRefValuePairs.Add(fieldRefValuePair);

                    RelativityObjectRef relativityObjectRef = new Relativity.ObjectManager.V1.Models.RelativityObjectRef { ArtifactID = documentArtifactId };

                    var updateRequest = new UpdateRequest()
                    {
                        Object = relativityObjectRef,
                        FieldValues = fieldRefValuePairs
                    };

                    return await objectManager.UpdateAsync(workspaceArtifactId, updateRequest);
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception, "There was an error updating the Other Metadata field for Document {documentArtifactId} in Workspace: {workspaceArtifactId} with values: {linkedProcessingFields}", documentArtifactId, workspaceArtifactId, linkedProcessingFields);
                }
            }
            return new UpdateResult();
        }



    }

    class OtherMetadataResultObject
    {
        public int ArtifactId { get; set; }
        public string OtherMetadataFieldValue { get; set; }
        public List<RelativityObjectValue> MissingMetadataFieldValue { get; set; }
    }
}
