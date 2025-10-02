using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine;

/// <summary>
/// Receives generation frames emitted by the <see cref="EngineRunner"/>.
/// </summary>
public interface IGenerationSink
{
    /// <summary>
    /// Notifies the sink that a new generation run has started.
    /// </summary>
    /// <param name="model">The model being executed.</param>
    /// <param name="context">Metadata describing the current run.</param>
    void BeginRun(ModelDefinition model, GenerationRunContext context);

    /// <summary>
    /// Emits a snapshot of the model's grid state.
    /// </summary>
    /// <param name="model">The model being executed.</param>
    /// <param name="context">Metadata describing the current run.</param>
    /// <param name="frame">The snapshot data.</param>
    void HandleFrame(ModelDefinition model, GenerationRunContext context, GenerationFrame frame);

    /// <summary>
    /// Notifies the sink that the current generation run has finished.
    /// </summary>
    /// <param name="model">The model being executed.</param>
    /// <param name="context">Metadata describing the current run.</param>
    void CompleteRun(ModelDefinition model, GenerationRunContext context);
}
