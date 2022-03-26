using kCura.Agent;
using Relativity.API;
using Relativity.ObjectManager.V1.Models;
using Relativity.Services.FieldMapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    [kCura.Agent.CustomAttributes.Name("Processing Field Analysis Manager Agent")]
    [System.Runtime.InteropServices.Guid("9bbfb2c2-da53-4472-9b28-62457ea7bfe7")]
    public class ProcessingFieldAnalysisManagerAgent : AgentBase
    {
        /// <summary>
        /// Agent logic goes here
        /// </summary>
        public override async void Execute()
        {
            IAPILog logger = Helper.GetLoggerFactory().GetLogger();

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                List<FieldRef> fields = new List<FieldRef>()
                {
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_HASH_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_FRIENDLY_NAME_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_DESCRIPTION_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_MINIMUM_LENGTH_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD},
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD}
                };

                //helpers
                InvariantField invariantField = new InvariantField();
                ProcessingFieldObject processingFieldObject = new ProcessingFieldObject();
                ApplicationWorkspace appWorkspace = new ApplicationWorkspace();

                DataTable installedWorkspaceArtifactIds = appWorkspace.RetrieveApplicationWorkspaces(Helper.GetDBContext(-1));

                foreach (DataRow workspaceArtifactIdRow in installedWorkspaceArtifactIds.Rows)
                {
                    int workspaceArtifactId = (int)workspaceArtifactIdRow["CaseID"];

                    MappableSourceField[] mappableSourceFields = await invariantField.GetInvariantFieldsAsync(Helper, workspaceArtifactId, logger);

                    List<string> existingProcessingFields = new List<string>();
                    existingProcessingFields = await processingFieldObject.GetProcessingFieldNameListFromWorkspace(Helper, workspaceArtifactId, logger);

                    List<IReadOnlyList<object>> fieldValues = new List<IReadOnlyList<object>>();
                    foreach (MappableSourceField mappableSourceField in mappableSourceFields)
                    {
                        if (!existingProcessingFields.Contains(mappableSourceField.SourceName))
                        {
                            Dictionary<string, int> existingCategoryChoices = new Dictionary<string, int>();
                            existingCategoryChoices = processingFieldObject.GetFieldChoiceNames(Helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD, logger);

                            Dictionary<string, int> existingDataTypeChoices = new Dictionary<string, int>();
                            existingDataTypeChoices = processingFieldObject.GetFieldChoiceNames(Helper, workspaceArtifactId, GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD, logger);

                            int categoryChoiceArtifactId;
                            int dataTypeChoiceArtifactId;

                            if (existingDataTypeChoices.ContainsKey(mappableSourceField.DataType))
                            {
                                existingDataTypeChoices.TryGetValue(mappableSourceField.DataType, out dataTypeChoiceArtifactId);
                            }
                            else
                            {
                                dataTypeChoiceArtifactId = await appWorkspace.CreateChoiceAsync(Helper, workspaceArtifactId, mappableSourceField.DataType, GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD);
                            }

                            if (existingCategoryChoices.ContainsKey(mappableSourceField.Category))
                            {
                                existingCategoryChoices.TryGetValue(mappableSourceField.Category, out categoryChoiceArtifactId);
                            }
                            else
                            {
                                categoryChoiceArtifactId = await appWorkspace.CreateChoiceAsync(Helper, workspaceArtifactId, mappableSourceField.Category, GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD);
                            }

                            IReadOnlyList<object> fieldValue = new List<object>()
                            {
                                ComputeHashString(mappableSourceField.SourceName),
                                mappableSourceField.FriendlyName,
                                new ChoiceRef { ArtifactID = categoryChoiceArtifactId },
                                mappableSourceField.Description,
                                mappableSourceField.Length,
                                new ChoiceRef { ArtifactID = dataTypeChoiceArtifactId },
                                mappableSourceField.SourceName
                            };
                            fieldValues.Add(fieldValue);
                        }
                    }
                    await processingFieldObject.CreateProcessingFieldObjects(Helper, workspaceArtifactId, logger, fields, fieldValues);
                }
                RaiseMessage("Completed.", 1);
            }
            catch (Exception ex)
            {
                //Your Agent caught an exception
                logger.LogError(ex, "There was an exception.");
                RaiseError(ex.Message, ex.Message);
            }

            return;
        }

        static string ComputeHashString(string rawData)
        {
            // Create a SHA256   
            using (SHA512 sha512Hash = SHA512.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha512Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Returns the name of agent
        /// </summary>
        public override string Name
        {
            get
            {
                return "Processing Field Analysis Manager Agent";
            }
        }

    }
}