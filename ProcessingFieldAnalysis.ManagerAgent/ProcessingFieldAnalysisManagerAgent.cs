using kCura.Agent;
using Relativity.API;
using System;
using System.Net;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    [kCura.Agent.CustomAttributes.Name("Processing Field Analysis Manager Agent")]
    [System.Runtime.InteropServices.Guid("9bbfb2c2-da53-4472-9b28-62457ea7bfe7")]
    public class ProcessingFieldAnalysisManagerAgent : AgentBase
    {
        /// <summary>
        /// Agent logic goes here
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
            catch (Exception ex)
            {
                //Your Agent caught an exception
                logger.LogError(ex, "There was an exception.");
                RaiseError(ex.Message, ex.Message);
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