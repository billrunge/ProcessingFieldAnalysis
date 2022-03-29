using Relativity.API;
using Relativity.Services.FieldMapping;
using System;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class InvariantField
    {
        public IHelper Helper { get; set; }
        public IAPILog Logger { get; set; }

        public InvariantField(IHelper helper, IAPILog logger)
        {
            Helper = helper;
            Logger = logger;
        }
        /// <summary>
        /// Gets an array of IFieldMapper.MappableSourceFields 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="logger"></param>
        /// <returns>MappableSourceField[]</returns>
        public async Task<MappableSourceField[]> GetInvariantFieldsAsync(int workspaceArtifactId)
        {
            MappableSourceField[] result = null;
            using (IFieldMapping proxy = Helper.GetServicesManager().CreateProxy<IFieldMapping>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    result = await proxy.GetInvariantFieldsAsync(workspaceArtifactId);
                }
                catch (Exception exception)
                {
                    Logger.LogError(exception, "FieldMapping Service GetInvariantFieldsAsync call failed for Workspace ID {0}", workspaceArtifactId);
                }
            }
            return result;
        }
    }
}
