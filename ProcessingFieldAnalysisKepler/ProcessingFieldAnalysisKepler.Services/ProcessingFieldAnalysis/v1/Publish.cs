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
using Newtonsoft.Json;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Processing.V1.Services;
using Relativity.Processing.V1.Services.Interfaces.DTOs;

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

        public async Task<PublishModel> PublishFiles(List<long> documentArtifactIds, int workspaceId)
        {
            PublishModel model;

            Guid processingFileIdFieldGuid = new Guid("93E1CFEB-F21E-4386-ADC3-846066525FE8");
            List<long> processingFileIds = new List<long>();

            try
            {
                var queryRequest = new Relativity.ObjectManager.V1.Models.QueryRequest()
                {
                    ObjectType = new Relativity.ObjectManager.V1.Models.ObjectTypeRef { ArtifactTypeID = 10 },
                    Condition = $"('Artifact ID' IN [{ string.Join(",", documentArtifactIds) }])",
                    Fields = new List<Relativity.ObjectManager.V1.Models.FieldRef>()
                    {
                    new Relativity.ObjectManager.V1.Models.FieldRef { Guid = processingFileIdFieldGuid }
                    }
                };

                using (Relativity.ObjectManager.V1.Interfaces.IObjectManager objectManager = _helper.GetServicesManager().CreateProxy<Relativity.ObjectManager.V1.Interfaces.IObjectManager>(ExecutionIdentity.System))
                {
                    Relativity.ObjectManager.V1.Models.QueryResult queryResult = await objectManager.QueryAsync(workspaceId, queryRequest, 1, 1000);

                    foreach (Relativity.ObjectManager.V1.Models.RelativityObject result in queryResult.Objects)
                    {
                        Relativity.ObjectManager.V1.Models.FieldValuePair documentFieldPair = result[processingFileIdFieldGuid];

                        bool parseSuccessful = long.TryParse(documentFieldPair.Value.ToString(), out long fileId);

                        if (parseSuccessful)
                        {
                            processingFileIds.Add(fileId);
                            _logger.LogError("Parsing fileId: {fileId}", fileId);
                        }
                        else
                        {
                            _logger.LogError("There was an issue parsing the file ids.");
                        }
                    }
                }



                using (IProcessingDocumentManager proxy = _helper.GetServicesManager().CreateProxy<IProcessingDocumentManager>(ExecutionIdentity.CurrentUser))
                {
                    ProcessingDocumentsRequest request = new ProcessingDocumentsRequest()
                    {
                        Expression = "{\"Type\":\"ConditionalExpression\",\"Property\":\"IsDeleted\",\"Constraint\":\"Is\",\"Value\":false}",
                        ProcessingFileIDs = processingFileIds
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
