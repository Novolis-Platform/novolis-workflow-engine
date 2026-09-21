namespace Novolis.WorkflowEngine;

/// <summary>
/// Transforms one workflow payload into another payload.
/// </summary>
/// <typeparam name="TIn">The input payload type.</typeparam>
/// <typeparam name="TOut">The output payload type.</typeparam>
public interface IStep<in TIn, TOut>
    where TIn : class
    where TOut : class
{
    /// <summary>
    /// Transforms an input payload.
    /// </summary>
    /// <param name="input">The input payload.</param>
    /// <returns>The transformed payload.</returns>
    Task<TOut> ExecuteAsync(TIn input);
}
