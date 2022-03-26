using Relativity.ObjectManager.V1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class Field
    {
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
