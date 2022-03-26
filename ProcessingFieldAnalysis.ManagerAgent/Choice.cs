using Relativity.API;
using Relativity.ObjectManager.V1.Models;
using Relativity.ObjectModel.V1.Choice;
using Relativity.ObjectModel.V1.Choice.Models;
using Relativity.Shared.V1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        public async Task<ChoiceRef> GetSingleChoiceChoiceRefByNameAsync(IHelper helper, int workspaceArtifactId, Guid singleChoiceFieldGuid, string choiceName, IAPILog logger)
        {
            Dictionary<string, int> existingChoices = GetChoicesByField(helper, workspaceArtifactId, singleChoiceFieldGuid, logger);
            int choiceArtifactId;

            if (existingChoices.ContainsKey(choiceName))
            {
                existingChoices.TryGetValue(choiceName, out choiceArtifactId);
            }
            else
            {
                choiceArtifactId = await CreateChoiceAsync(helper, workspaceArtifactId, choiceName, singleChoiceFieldGuid);
            }
            return new ChoiceRef { ArtifactID = choiceArtifactId };
        }

        public async Task<List<ChoiceRef>> GetMultipleChoiceRefsByNameAsync(IHelper helper, int workspaceArtifactId, Guid multipleChoiceFieldGuid, string[] choiceNames, IAPILog logger)
        {
            ProcessingFieldObject processingFieldObject = new ProcessingFieldObject();
            Dictionary<string, int> existingChoices = GetChoicesByField(helper, workspaceArtifactId, multipleChoiceFieldGuid, logger);
            List<ChoiceRef> choiceRefs = new List<ChoiceRef>();

            if (choiceNames != null)
            {
                foreach (string choice in choiceNames)
                {
                    int mappedFieldArtifactId;
                    if (existingChoices.ContainsKey(choice))
                    {
                        existingChoices.TryGetValue(choice, out mappedFieldArtifactId);
                    }
                    else
                    {
                        mappedFieldArtifactId = await CreateChoiceAsync(helper, workspaceArtifactId, choice, multipleChoiceFieldGuid);
                    }

                    choiceRefs.Add(new ChoiceRef { ArtifactID = mappedFieldArtifactId });
                }
            }
            return choiceRefs;
        }
        public Dictionary<string, int> GetChoicesByField(IHelper helper, int workspaceArtifactId, Guid fieldGuid, IAPILog logger)
        {
            string sql = $@"
                SELECT C.[ArtifactID],
                       C.[Name]
                FROM   [Code] C
                       JOIN [Field] F
                         ON F.[CodeTypeID] = C.[CodeTypeID]
                       JOIN [ArtifactGuid] A
                         ON A.[ArtifactID] = F.[ArtifactID]
                WHERE  A.[ArtifactGuid] = @FieldGuid";

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter("@FieldGuid", SqlDbType.UniqueIdentifier) {Value = fieldGuid}
            };

            IDBContext dbContext = helper.GetDBContext(workspaceArtifactId);
            DataTable resultDataTable = dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParams);
            Dictionary<string, int> choices = new Dictionary<string, int>();

            foreach (DataRow row in resultDataTable.Rows)
            {
                choices.Add((string)row["Name"], (int)row["ArtifactID"]);
            }
            return choices;
        }
    }
}
