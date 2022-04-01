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
        ///   [GET] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/workspace/1015024
        /// Example REST response:
        ///   {"Name":"Relativity Starter Template"}
        /// </remarks>
        [HttpGet]
        [Route("workspace/{workspaceID:int}")]
        Task<QueueModel> GetWorkspaceNameAsync(int workspaceID);

        /// <summary>
        /// Query for a workspace by name
        /// </summary>
        /// <param name="queryString">Partial name of a workspace to query for.</param>
        /// <param name="limit">Limit the number of results via a query string parameter. (Default 10)</param>
        /// <returns>Collection of <see cref="QueueModel"/> containing workspace names that match the query string.</returns>
        /// <remarks>
        /// Example REST request:
        ///   [POST] /Relativity.REST/api/ProcessingFieldAnalysis/v1/Queue/workspace?limit=2
        ///   { "queryString":"a" }
        /// Example REST response:
        ///   [{"Name":"New Case Template"},{"Name":"Relativity Starter Template"}]
        /// </remarks>
        [HttpPost]
        [Route("workspace?{limit}")]
        Task<List<QueueModel>> QueryWorkspaceByNameAsync(string queryString, int limit = 10);
    }
}
