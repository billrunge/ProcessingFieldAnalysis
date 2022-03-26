using Relativity.API;
using Relativity.Services.FieldMapping;
using System;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class InvariantField
    {
        /// <summary>
        /// Gets an array of IFieldMapper.MappableSourceFields 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <returns>MappableSourceField[]</returns>
        public async Task<MappableSourceField[]> GetInvariantFieldsAsync(IHelper helper, int workspaceArtifactId, IAPILog logger)
        {
            MappableSourceField[] result = null;
            using (IFieldMapping proxy = helper.GetServicesManager().CreateProxy<IFieldMapping>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    result = await proxy.GetInvariantFieldsAsync(workspaceArtifactId);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "FieldMapping Service GetInvariantFieldsAsync call failed for Workspace ID {0}", workspaceArtifactId);
                }
            }
            return result;
        }
    }
}
