using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.API.Context;
using Relativity.Kepler.Logging;
using Relativity.Services.Exceptions;
using ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1;
using ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1.Exceptions;
using ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1.Models;
using System.Data;
using Relativity.Processing.V1.Services;
using Relativity.Processing.V1.Services.Interfaces.DTOs;
using Newtonsoft.Json;

namespace ProcessingFieldAnalysisKepler.Services.ProcessingFieldAnalysis.v1
{
    public class Publish : IPublish
    {
        private IHelper _helper;
        private ILog _logger;

        // Note: IHelper and ILog are dependency injected into the constructor every time the service is called.
        public Publish(IHelper helper, ILog logger)
        {
            // Note: Set the logging context to the current class.
            _logger = logger.ForContext<Queue>();
            _helper = helper;
        }

        public async Task<PublishModel> PublishFiles(int workspaceId)
        {
            PublishModel model;

            try
            {
                using (IProcessingDocumentManager proxy = _helper.GetServicesManager().CreateProxy<IProcessingDocumentManager>(ExecutionIdentity.CurrentUser))
                {
                    ProcessingDocumentsRequest request = new ProcessingDocumentsRequest() {
                        Expression = "{\"Type\":\"ConditionalExpression\",\"Property\":\"IsDeleted\",\"Constraint\":\"Is\",\"Value\":false}",
                        ProcessingFileIDs = new List<long> {2, 3, 4, 5}
                    };

                    await proxy.PublishDocumentsAsync(workspaceId, request);
                
                }
                    model = new PublishModel
                {
                    Message = $"Processing Field Object Maintenance enabled for Workspace: {workspaceId}"
                };
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not enable Processing Field Object Maintenance for Workspace: {WorkspaceId}.", workspaceId);
                throw new QueueException($"Could not enable Processing Field Object Maintenance for Workspace: {workspaceId}.")
                {
                    FaultSafeObject = new QueueException.FaultSafeInfo()
                    {
                        Information = $"Workspace {workspaceId}",
                        Time = DateTime.Now
                    }
                };
            }

            return model;
        }





        /// <summary>
        /// All Kepler services must inherit from IDisposable.
        /// Use this dispose method to dispose of any unmanaged memory at this point.
        /// See https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose for examples of how to properly use the dispose pattern.
        /// </summary>
        public void Dispose()
        { }
    }
}
