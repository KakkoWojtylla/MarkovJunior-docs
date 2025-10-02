using System;
using System.Collections.Generic;
using MarkovJunior.Engine.Definitions;
using MarkovJunior.Engine.Runtime;

namespace MarkovJunior.Engine;

/// <summary>
/// Coordinates model compilation, execution and frame delivery through a sink.
/// </summary>
public sealed class EngineRunner
{
    private readonly IInterpreterFactory _interpreterFactory;

    public EngineRunner(IInterpreterFactory interpreterFactory)
    {
        _interpreterFactory = interpreterFactory ?? throw new ArgumentNullException(nameof(interpreterFactory));
    }

    public void Run(ModelDefinition model, IGenerationSink sink)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (sink is null) throw new ArgumentNullException(nameof(sink));

        ModelExecutionSettings execution = model.Execution;

        IReadOnlyList<int>? seeds = execution.Seeds;
        Random meta = seeds is null ? new Random() : null;

        for (int runIndex = 0; runIndex < execution.Runs; runIndex++)
        {
            int seed;
            if (seeds != null && runIndex < seeds.Count)
            {
                seed = seeds[runIndex];
            }
            else
            {
                meta ??= new Random();
                seed = meta.Next();
            }

            int? maxSteps = execution.Steps;
            bool gif = execution.EmitGif;
            GenerationRunContext context = new GenerationRunContext(runIndex, seed, gif, maxSteps);
            sink.BeginRun(model, context);

            using GenerationSession session = new GenerationSession(model, _interpreterFactory);
            session.Start(seed, new GenerationSessionOptions
            {
                EmitIntermediateFrames = gif,
                MaxSteps = maxSteps
            });

            session.RunUntilComplete(frame => sink.HandleFrame(model, context, frame));
            sink.CompleteRun(model, context);
        }
    }
}
