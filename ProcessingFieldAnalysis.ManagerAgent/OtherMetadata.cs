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
    class OtherMetadata
    {
        public async Task<Dictionary<int, string>> GetOtherMetadataList(IHelper helper, int workspaceArtifactId, IAPILog logger)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    Workspace workspace = new Workspace();
                    string otherMetadataFieldName = workspace.GetTextIdentifierByGuid(helper, workspaceArtifactId, GlobalVariable.DOCUMENT_OBJECT_OTHER_METADATA_FIELD, logger);

                    var queryRequest = new QueryRequest()
                    {
                        Fields = new List<FieldRef>() { new FieldRef{ Guid = GlobalVariable.DOCUMENT_OBJECT_OTHER_METADATA_FIELD }},
                        ObjectType = new ObjectTypeRef { Guid = GlobalVariable.DOCUMENT_OBJECT },
                        Condition = $"'{otherMetadataFieldName}' ISSET"
                    };

                    QueryResult queryResult = await objectManager.QueryAsync(workspaceArtifactId, queryRequest, 1, Int32.MaxValue);

                    Dictionary<int, string> otherMetadataList = new Dictionary<int, string>();

                    foreach (RelativityObject resultObject in queryResult.Objects)
                    {
                        otherMetadataList.Add(resultObject.ArtifactID, (string)resultObject[GlobalVariable.DOCUMENT_OBJECT_OTHER_METADATA_FIELD].Value);
                    }
                    return otherMetadataList;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unable to get a list of Other Metadata from Workspace: {workspaceArtifactId}", workspaceArtifactId);
                }
                return new Dictionary<int, string>();
            }
        }

        public async Task ParseOtherMetadataField(IHelper helper, int workspaceArtifactId, List<MappableField> existingProcessingFields, IAPILog logger)
        {
            Dictionary<int, string> workspaceOtherMetadataFieldList = await GetOtherMetadataList(helper, workspaceArtifactId, logger);

            foreach(KeyValuePair<int, string> otherMetadataField in workspaceOtherMetadataFieldList)
            {
                List<int> linkedProcessingFields = new List<int>();
                foreach (MappableField mappableField in existingProcessingFields)
                {

                    if (otherMetadataField.Value.Contains(mappableField.SourceName))
                    {
                        linkedProcessingFields.Add(mappableField.ArtifactId);
                    }
                }
                if (linkedProcessingFields.Count > 0)
                {
                    await UpdateOtherMetadataFieldAsync(helper, workspaceArtifactId, otherMetadataField.Key, linkedProcessingFields, logger);
                }
            }
        }

        public async Task<UpdateResult> UpdateOtherMetadataFieldAsync(IHelper helper, int workspaceArtifactId, int documentArtifactId, List<int> linkedProcessingFields, IAPILog logger)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    List <RelativityObjectRef> linkedProcessingFieldObjectRefs = new List<RelativityObjectRef>();
                    foreach (int linkedProcessingFieldArtifactId in linkedProcessingFields)
                    {
                        linkedProcessingFieldObjectRefs.Add(new RelativityObjectRef { ArtifactID = linkedProcessingFieldArtifactId });
                    }
                    List<FieldRefValuePair> fieldRefValuePairs = new List<FieldRefValuePair>();
                    FieldRef processingFilesDocumentField = new FieldRef() { Guid = GlobalVariable.DOCUMENT_OBJECT_PROCESSING_FILES_MULTI_OBJECT_FIELD };
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
                    logger.LogError(exception, "There was an error updating the Other Metadata field for Document {documentArtifactId} in Workspace: {workspaceArtifactId} with values: {linkedProcessingFields}", documentArtifactId, workspaceArtifactId, linkedProcessingFields);
                }
            }
            return new UpdateResult();
        }

    }
}
