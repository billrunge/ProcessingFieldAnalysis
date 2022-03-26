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
        /// <summary>
        /// Creates a Choice for a given Field (identified by Guid)
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="choiceName"></param>
        /// <param name="fieldGuid"></param>
        /// <param name="logger"></param>
        /// <returns>int</returns>
        public async Task<int> CreateChoiceAsync(IHelper helper, int workspaceArtifactId, string choiceName, Guid fieldGuid, IAPILog logger)
        {
            try
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
            catch (Exception e)
            {
                logger.LogError(e, "Failed to create Choice: {choiceName} on Field: {fieldGuid} in Workspace: {workspaceArtifactId}", choiceName, fieldGuid, workspaceArtifactId);
            }
            return 0;
        }

        /// <summary>
        /// Takes a Field Guid and a Choice name, checks if there is a Choice that
        /// already exists with that name for that Field, and if so returns its ChoiceRef
        /// If there is not Choice on the Field with he supplied name, it will create the Choice 
        /// and return the new Choice's ChoiceRef
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="singleChoiceFieldGuid"></param>
        /// <param name="choiceName"></param>
        /// <param name="logger"></param>
        /// <returns>ChoiceRef</returns>
        public async Task<ChoiceRef> GetSingleChoiceChoiceRefByNameAsync(IHelper helper, int workspaceArtifactId, Guid singleChoiceFieldGuid, string choiceName, IAPILog logger)
        {
            try
            {
                Dictionary<string, int> existingChoices = GetChoicesByField(helper, workspaceArtifactId, singleChoiceFieldGuid, logger);
                int choiceArtifactId;

                if (existingChoices.ContainsKey(choiceName))
                {
                    existingChoices.TryGetValue(choiceName, out choiceArtifactId);
                }
                else
                {
                    choiceArtifactId = await CreateChoiceAsync(helper, workspaceArtifactId, choiceName, singleChoiceFieldGuid, logger);
                }
                return new ChoiceRef { ArtifactID = choiceArtifactId };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get Single ChoiceRef for Choice: {choiceName}: on Field: {singleChoiceFieldGuid} in Workspace: {workspaceArtifactId}", choiceName, singleChoiceFieldGuid, workspaceArtifactId);
            }
            return new ChoiceRef();
        }
        /// <summary>
        /// Takes a Field Guid and an array of Choice names, checks if there are Choices that
        /// already exists with those names for that Field, and if so returns a list of those ChoiceRefs
        /// If there is not Choice on the Field with the supplied name, it will create the Choice 
        /// and return the new Choice's ChoiceRef in the output list
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="multipleChoiceFieldGuid"></param>
        /// <param name="choiceNames"></param>
        /// <param name="logger"></param>
        /// <returns>List<ChoiceRef></returns>
        public async Task<List<ChoiceRef>> GetMultipleChoiceRefsByNameAsync(IHelper helper, int workspaceArtifactId, Guid multipleChoiceFieldGuid, string[] choiceNames, IAPILog logger)
        {
            try
            {
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
                            mappedFieldArtifactId = await CreateChoiceAsync(helper, workspaceArtifactId, choice, multipleChoiceFieldGuid, logger);
                        }

                        choiceRefs.Add(new ChoiceRef { ArtifactID = mappedFieldArtifactId });
                    }
                }
                return choiceRefs;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get Multiple ChoiceRefs for Choices: {choiceNames}: on Field: {multipleChoiceFieldGuid} in Workspace: {workspaceArtifactId}", choiceNames, multipleChoiceFieldGuid, workspaceArtifactId);
            }
            return new List<ChoiceRef>();
        }

        /// <summary>
        /// Gets a list of Choice names associated with a specific Field (identified by Guid)
        /// Currently uses SQL, but would like to replace with API
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="workspaceArtifactId"></param>
        /// <param name="fieldGuid"></param>
        /// <param name="logger"></param>
        /// <returns>Dictionary<string, int></returns>
        public Dictionary<string, int> GetChoicesByField(IHelper helper, int workspaceArtifactId, Guid fieldGuid, IAPILog logger)
        {
            try
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
            catch (Exception e)
            {
                logger.LogError(e, "Failed to get Choices for Field: {fieldGuid} in Workspace: {workspaceArtifactId}", fieldGuid, workspaceArtifactId);
            }
            return new Dictionary<string, int>();
        }
    }
}
