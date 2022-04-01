namespace ProcessingFieldAnalysisKepler.Interfaces.ProcessingFieldAnalysis.v1.Models
{
    /// <summary>
    /// QueueModel Data Model.
    /// </summary>
    public class QueueModel
    {
        /// <summary>
        /// Name property.
        /// </summary>
        public string Name { get; set; }
        public string Message { get; set; }
        public bool IsEnabled { get; set; }
    }
}
