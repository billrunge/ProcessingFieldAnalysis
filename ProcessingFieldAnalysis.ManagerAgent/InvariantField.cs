using Relativity.API;
using Relativity.Services.FieldMapping;
using System;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class InvariantField
    {
        public async Task<MappableSourceField[]> GetInvariantFieldsAsync(IHelper helper, int workspaceArtifactId, IAPILog logger)
        {
            MappableSourceField[] result = null;
            using (Relativity.Services.FieldMapping.IFieldMapping proxy = helper.GetServicesManager().CreateProxy<Relativity.Services.FieldMapping.IFieldMapping>(ExecutionIdentity.CurrentUser))
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
