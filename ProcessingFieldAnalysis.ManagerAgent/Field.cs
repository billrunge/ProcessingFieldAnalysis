using Relativity.ObjectManager.V1.Models;
using System.Collections.Generic;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class Field
    {
        /// <summary>
        /// A list of FieldRefs to interface to the fields in the IFieldMapping.MappableSourceField object
        /// </summary>
        public List<FieldRef> fields = new List<FieldRef>()
                {
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_HASH_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_FRIENDLY_NAME_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_CATEGORY_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_DESCRIPTION_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_MINIMUM_LENGTH_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_DATA_TYPE_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_SOURCE_NAME_FIELD },
                    new FieldRef{ Guid = GlobalVariables.PROCESSING_FIELD_OBJECT_MAPPED_FIELDS_FIELD }
                };
    }
}
