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
    [WebService("Queue Service")]
    [ServiceAudience(Audience.Public)]
    [RoutePrefix("Queue")]
    public interface IQueue : IDisposable
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
        [HttpGet]
        [Route("EnableProcessingFieldObjectMaintenance/{workspaceId:int}")]
        Task<QueueModel> EnableProcessingFieldObjectMaintenance(int workspaceId);
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/DisableProcessingFieldObjectMaintenance/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("DisableProcessingFieldObjectMaintenance/{workspaceId:int}")]
        Task<QueueModel> DisableProcessingFieldObjectMaintenance(int workspaceId);
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/ForceProcessingFieldObjectMaintenance/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("ForceProcessingFieldObjectMaintenance/{workspaceId:int}")]
        Task<QueueModel> ForceProcessingFieldObjectMaintenance(int workspaceId);
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/IsProcessingFieldObjectMaintenanceEnabled/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("IsProcessingFieldObjectMaintenanceEnabled/{workspaceId:int}")]
        Task<QueueModel> IsProcessingFieldObjectMaintenanceEnabled(int workspaceId);
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/EnableOtherMetadataAnalysis/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("EnableOtherMetadataAnalysis/{workspaceId:int}")]
        Task<QueueModel> EnableOtherMetadataAnalysis(int workspaceId);
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/DisableOtherMetadataAnalysis/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("DisableOtherMetadataAnalysis/{workspaceId:int}")]
        Task<QueueModel> DisableOtherMetadataAnalysis(int workspaceId);
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/ForceOtherMetadataAnalysis/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("ForceOtherMetadataAnalysis/{workspaceId:int}")]
        Task<QueueModel> ForceOtherMetadataAnalysis(int workspaceId);
        /// <summary>
        /// Get workspace name.
        /// </summary>
        /// <param name="workspaceID">Workspace ArtifactID.</param>
        /// <returns><see cref="QueueModel"/> with the name of the workspace.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/IsOtherMetadataAnalysisEnabled/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("IsOtherMetadataAnalysisEnabled/{workspaceId:int}")]
        Task<QueueModel> IsOtherMetadataAnalysisEnabled(int workspaceId);

        [HttpGet]
        [Route("ForceCustomOtherMetadataAnalysis/{workspaceId:int}")]
        Task<QueueModel> ForceCustomOtherMetadataAnalysis(int workspaceId);
    }
}
