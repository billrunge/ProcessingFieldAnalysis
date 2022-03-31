using kCura.Agent;
using Relativity.API;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Net;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    [kCura.Agent.CustomAttributes.Name("Processing Field Analysis Manager Agent")]
    [System.Runtime.InteropServices.Guid("9bbfb2c2-da53-4472-9b28-62457ea7bfe7")]
    public class Agent : AgentBase
    {
        /// <summary>
        /// Agent Execute() entry point.
        /// </summary>
        public override async void Execute()
        {
            IAPILog Logger = Helper.GetLoggerFactory().GetLogger();
            try
            {


                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Workspace workspace = new Workspace(Helper, Logger);
                InvariantField invariantField = new InvariantField(Helper, Logger);
                ProcessingField processingField = new ProcessingField(Helper, Logger);
                OtherMetadata otherMetadata = new OtherMetadata(Helper, Logger);
                EddsQueue eddsQueue = new EddsQueue(Helper, Logger);

                eddsQueue.CreateProcessingFieldManagerQueueTable();
                eddsQueue.PopulateProcessingFieldManagerQueueTable();

                //                List<int> installedWorkspaceArtifactIds = workspace.GetWorkspaceArtifactIdsWhereApplicationIsInstalled();

                List<int> workspacesToManageProcessingFields = eddsQueue.GetListOfWorkspaceArtifactIdsToManageProcessingFields();

                foreach (int workspaceArtifactId in workspacesToManageProcessingFields)
                {
                    if (eddsQueue.StartProcessingFieldObjectMaintenance(workspaceArtifactId))
                    {
                        MappableSourceField[] mappableSourceFields = await invariantField.GetInvariantFieldsAsync(workspaceArtifactId);
                        List<MappableField> existingProcessingFields = await processingField.GetProcessingFieldObjectMappableFieldsAsync(workspaceArtifactId);
                        await processingField.PopulateProcessingFieldObjectAsync(workspaceArtifactId, mappableSourceFields, existingProcessingFields);
                        await processingField.UpdateProcessingFieldObjectsAsync(workspaceArtifactId, mappableSourceFields, existingProcessingFields);
                        eddsQueue.EndProcessingFieldObjectMaintenance(workspaceArtifactId);
                    }
                    //await otherMetadata.ParseOtherMetadataFieldAndLinkMissingProcessingFieldsAsync(workspaceArtifactId, existingProcessingFields, GlobalVariable.OTHER_METADATA_FIELD_PARSING_BATCH_SIZE);
                }

                Dictionary<int, bool> workspacesToAnalyzeOtherMetadata = eddsQueue.GetListOfWorkspaceArtifactIdsToAnalyzeOtherMetadata();
                WorkspaceQueue workspaceQueue = new WorkspaceQueue(Helper, Logger);

                foreach (KeyValuePair<int, bool> workspaceToAnalyze in workspacesToAnalyzeOtherMetadata)
                {
                    if (workspaceToAnalyze.Value)
                    {

                    }
                    else
                    {
                        workspaceQueue.CreateWorkspaceQueueTable(workspaceToAnalyze.Key);
                        await workspaceQueue.PopulateWorkspaceQueueTableAsync(workspaceToAnalyze.Key, GlobalVariable.WORKSPACE_QUEUE_TABLE_POPULATION_BATCH_SIZE);

                    }
                }

                RaiseMessage("Completed.", 1);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "The Processing Field Analysis Manager Agent encountered an issue");
                RaiseError("The Processing Field Analysis Manager Agent encountered an issue", e.ToString());
            }
            return;
        }

        /// <summary>
        /// Returns the name of agent
        /// </summary>
        public override string Name
        {
            get
            {
                return "Processing Field Analysis Manager Agent";
            }
        }

    }
}