using Relativity.API;
using Relativity.ObjectModel.V1.Choice;
using Relativity.ObjectModel.V1.Choice.Models;
using Relativity.Shared.V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class Choice
    {
        public async Task<int> CreateChoiceAsync(IHelper helper, int workspaceArtifactId, string choiceName, Guid fieldGuid)
        {
            using (IChoiceManager choiceManager = helper.GetServicesManager().CreateProxy<IChoiceManager>(ExecutionIdentity.CurrentUser))
            {
                ChoiceRequest choiceRequest = new ChoiceRequest()
                {
                    Name = choiceName,
                    Field = new ObjectIdentifier() { Guids = new List<Guid>() { fieldGuid } },
                };
                return await choiceManager.CreateAsync(workspaceArtifactId, choiceRequest);
            }
        }
    }
}
