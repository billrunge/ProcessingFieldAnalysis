using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingFieldAnalysis.ManagerAgent
{
    class MappableField
    {
        public int ArtifactId { get; set; }
        public string SourceName { get; set; }
        public string FriendlyName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public int MinimumLength { get; set; }
        public string DataType { get; set; }
        public string[] MappedFields { get; set; }
    }
}
