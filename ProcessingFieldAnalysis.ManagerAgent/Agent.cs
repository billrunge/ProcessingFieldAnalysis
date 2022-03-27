using kCura.Agent;
using Relativity.API;
using System;
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
                await processingFieldObject.PopulateProcessingFieldObjectAsync(Helper, logger);
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