using kCura.Agent;
using Relativity.API;
using Relativity.Services.Objects;
using System;
using System.Net;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    [kCura.Agent.CustomAttributes.Name("Processing Field Analysis Manager Agent")]
    [System.Runtime.InteropServices.Guid("9bbfb2c2-da53-4472-9b28-62457ea7bfe7")]
    public class Relativity_Agent : AgentBase
    {
        /// <summary>
        /// Agent logic goes here
        /// </summary>
        public override void Execute()
        {
            IAPILog logger = Helper.GetLoggerFactory().GetLogger();

            try
            {
                // Update Security Protocol
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //Get the current Agent artifactID
                int agentArtifactId = AgentID;

                //Get a dbContext for the EDDS database
                IDBContext eddsDbContext = Helper.GetDBContext(-1);

                //Get a dbContext for the workspace database
                //int workspaceArtifactId = 01010101; // Update it with the real 
                //IDBContext workspaceDbContext = Helper.GetDBContext(workspaceArtifactId);

                //Get GUID for an artifact
                //int testArtifactId = 10101010;
                //Guid guidForTestArtifactId = Helper.GetGuid(workspaceArtifactId, testArtifactId);

                //Display a message in Agents Tab and Windows Event Viewer
                RaiseMessage("The current time is: " + DateTime.Now.ToLongTimeString(), 1);

                //The Object Manager is the newest and preferred way to interact with Relativity instead of the Relativity Services API(RSAPI). 
                using (IObjectManager objectManager = this.Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
                {

                }

                logger.LogVerbose("Log information throughout execution.");
            }
            catch (Exception ex)
            {
                //Your Agent caught an exception
                logger.LogError(ex, "There was an exception.");
                RaiseError(ex.Message, ex.Message);
            }
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