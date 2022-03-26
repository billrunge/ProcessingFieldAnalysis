using Relativity.API;
using Relativity.ObjectManager.V1.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class ProcessingField
    {
        public async Task<Relativity.ObjectManager.V1.Models.MassCreateResult> CreateProcessingFieldObjects(IHelper helper, int workspaceArtifactId, IAPILog logger, IReadOnlyList<Relativity.ObjectManager.V1.Models.FieldRef> fields, IReadOnlyList<IReadOnlyList<object>> fieldValues)
        {
            using (IObjectManager objectManager = helper.GetServicesManager().CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
            {
                try
                {
                    if (fieldValues.Count > 0)
                    {
                        var massCreateRequest = new Relativity.ObjectManager.V1.Models.MassCreateRequest
                        {
                            ObjectType = new Relativity.ObjectManager.V1.Models.ObjectTypeRef { Guid = GlobalVariables.PROCESSING_FIELD_OBJECT },
                            Fields = fields,
                            ValueLists = fieldValues
                        };

                        return await objectManager.CreateAsync(workspaceArtifactId, massCreateRequest);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "The Relativity Object could not be created.");
                }

            }
            return null;
        }
    }
}
