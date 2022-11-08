using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Kepler.Services;
using ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1.Models;

namespace ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1
{
    /// <summary>
    /// MyService Service Interface.
    /// </summary>
    [WebService("Publish Service")]
    [ServiceAudience(Audience.Public)]
    [RoutePrefix("Publish")]
    public interface IPublish : IDisposable
    {
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/EnableProcessingFieldObjectMaintenance/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpPost]
        [Route("{workspaceId:int}")]
        Task<PublishModel> PublishFiles(int workspaceId);

    }
}
