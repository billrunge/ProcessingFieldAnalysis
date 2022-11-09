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
    [WebService("Workspace Service")]
    [ServiceAudience(Audience.Public)]
    [RoutePrefix("Workspace")]
    public interface IWorkspace : IDisposable
    {
        //[HttpPost]
        //      [Route("{workspaceId:int}")]
        //      Task<PublishModel> PublishFiles(List<long> documentArtifactIds, int workspaceId);

        [HttpGet]
        [Route("DoesQueueTableExist/{workspaceArtifactId:int}")]
        Task<bool> DoesWorkspaceQueueTableExist(int workspaceArtifactId);

        [HttpGet]
        [Route("CreateQueueTable/{workspaceArtifactId:int}")]
        Task CreateWorkspaceQueueTable(int workspaceArtifactId);

        [HttpPost]
        [Route("InsertIntoQueueTable/{workspaceArtifactId:int}")]
        Task InsertIntoWorkspaceQueueTableAsync(int workspaceArtifactId, List<int> documentArtifactIds);
    }
}
 