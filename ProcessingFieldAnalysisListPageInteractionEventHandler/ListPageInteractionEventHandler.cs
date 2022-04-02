using kCura.EventHandler;
using Relativity.API;
using Relativity.Services.Objects;
using System;
using System.Net;

namespace ProcessingFieldAnalysisListPageInteractionEventHandler
{
    [kCura.EventHandler.CustomAttributes.Description("Processing Field Analysis List Page InteractionEventHandler")]
    [System.Runtime.InteropServices.Guid("9bb71612-c676-4ac3-9842-d83df953b394")]
    public class ListPageInteractionEventHandler : kCura.EventHandler.ListPageInteractionEventHandler
    {
        public override Response PopulateScriptBlocks()
        {
            // Update Security Protocol
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //Create a response object with default values
            kCura.EventHandler.Response retVal = new kCura.EventHandler.Response();
            retVal.Success = true;
            retVal.Message = string.Empty;

            Int32 currentWorkspaceArtifactID = Helper.GetActiveCaseID();

            //The Object Manager is the newest and preferred way to interact with Relativity instead of the Relativity Services API(RSAPI).
            using (IObjectManager objectManager = this.Helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {

            }

            IAPILog logger = Helper.GetLoggerFactory().GetLogger();
            logger.LogVerbose("Log information throughout execution.");

            return retVal;
        }

        public override string[] ScriptFileNames => new string[] { "toolbar.js" };

        //public override string[] AdditionalHostedFileNames => new string[] { "cat.jpg", "dog.jpg" };
    }
}
