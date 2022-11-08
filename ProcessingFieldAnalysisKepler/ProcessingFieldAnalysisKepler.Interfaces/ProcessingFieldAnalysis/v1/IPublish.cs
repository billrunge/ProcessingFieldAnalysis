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
		/// Publish files
		/// </summary>
		/// <param name="documentArtifactIds">a list of Document Artifact ids</param>
		/// <param name="workspaceId">Workspace Artifact ID</param>
		/// <returns>Collection of <see cref="$ext_ServiceName$Model"/> containing workspace names that match the query string.</returns>
		/// <remarks>
		/// Example REST request:
		///   [POST] /Relativity.REST/api/$ext_ServiceModule$/v1/$ext_ServiceName$/workspace?limit=2
		///   { "queryString":"a" }
		/// Example REST response:
		///   [{"Name":"New Case Template"},{"Name":"Relativity Starter Template"}]
		/// </remarks>
		[HttpPost]
        [Route("{workspaceId:int}")]
        Task<PublishModel> PublishFiles(List<long> documentArtifactIds, int workspaceId);

    }
}
 