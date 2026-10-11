namespace Novolis.WorkflowEngine.Graph;

/// <summary>
/// Analyzes graph topology without executing node values.
/// </summary>
public interface IGraphAnalyzer
{
    /// <summary>
    /// Analyzes one graph snapshot.
    /// </summary>
    /// <typeparam name="T">The node value type.</typeparam>
    /// <param name="graph">The graph to analyze.</param>
    /// <param name="options">The consumer policy.</param>
    /// <returns>Deterministic structural analysis.</returns>
    GraphAnalysis Analyze<T>(
        Graph<T> graph,
        GraphAnalysisOptions? options = null);
}
