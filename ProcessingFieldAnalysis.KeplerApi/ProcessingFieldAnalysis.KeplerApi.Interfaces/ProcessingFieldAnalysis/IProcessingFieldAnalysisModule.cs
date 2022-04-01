using Relativity.Kepler.Services;

namespace ProcessingFieldAnalysisApi.Interfaces.ProcessingFieldAnalysis
{
    /// <summary>
    /// ProcessingFieldAnalysis Module Interface.
    /// </summary>
    [ServiceModule("ProcessingFieldAnalysis Module")]
    [RoutePrefix("ProcessingFieldAnalysis", VersioningStrategy.Namespace)]
    public interface IProcessingFieldAnalysisModule
    {
    }
}