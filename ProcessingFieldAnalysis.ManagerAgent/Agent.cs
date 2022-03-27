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
            IAPILog logger = Helper.GetLoggerFactory().GetLogger();
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ProcessingFieldObject processingFieldObject = new ProcessingFieldObject();
                Workspace workspace = new Workspace();
                InvariantField invariantField = new InvariantField();
                ProcessingField processingField = new ProcessingField();

                List<int> installedWorkspaceArtifactIds = workspace.GetWorkspaceArtifactIdsWhereApplicationIsInstalled(Helper.GetDBContext(-1), logger);

                foreach (int workspaceArtifactId in installedWorkspaceArtifactIds)
                {
                    MappableSourceField[] mappableSourceFields = await invariantField.GetInvariantFieldsAsync(Helper, workspaceArtifactId, logger);
                    List<MappableField> existingProcessingFields = await processingField.GetProcessingFieldObjectMappableFieldsAsync(Helper, workspaceArtifactId, logger);
                    await processingFieldObject.PopulateProcessingFieldObjectAsync(Helper, workspaceArtifactId, mappableSourceFields, existingProcessingFields, logger);
                    await processingFieldObject.UpdateProcessingFieldObjectAsync(Helper, workspaceArtifactId, mappableSourceFields, existingProcessingFields, logger);
                }

                RaiseMessage("Completed.", 1);
            }
            catch (Exception e)
            {
                logger.LogError(e, "The Processing Field Analysis Manager Agent encountered an issue");
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