namespace ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1.Models
{
    /// <summary>
    /// QueueModel Data Model.
    /// </summary>
    public class WorkspaceModel
    {
        /// <summary>
        /// Name property.
        /// </summary>
        public string TextIdentifier { get; set; }
        public int ArtifactId { get; set; }
        public bool DoesTableExist { get; set; }
    }
}
