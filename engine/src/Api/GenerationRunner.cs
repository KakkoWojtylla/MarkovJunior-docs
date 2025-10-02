using System;
using System.Collections.Generic;
using MarkovJunior.Engine.Definitions;
using MarkovJunior.Engine.Runtime;
using MarkovJunior.Engine;

namespace MarkovJunior.Engine.Api;

/// <summary>
/// Convenience facade over <see cref="GenerationSession"/> for running models entirely in memory.
/// </summary>
public sealed class GenerationRunner
{
    private readonly IInterpreterFactory _interpreterFactory;

    public GenerationRunner(IInterpreterFactory? interpreterFactory = null)
    {
        _interpreterFactory = interpreterFactory ?? new DefinitionInterpreterFactory(new CharacterGridCompiler());
    }

    /// <summary>
    /// Runs the provided model and returns the captured frames.
    /// </summary>
    public GenerationResult Run(ModelDefinition model, GenerationRunnerOptions? options = null)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));

        options ??= new GenerationRunnerOptions();
        var frames = new List<GenerationFrame>();

        using var session = new GenerationSession(model, _interpreterFactory);
        session.Start(options.ResolveSeed(), options.SessionOptions);

        bool captureAll = options.CaptureIntermediateFrames;
        session.RunUntilComplete(frame =>
        {
            if (captureAll || frame.IsFinal)
            {
                frames.Add(frame);
            }
        });

        if (frames.Count == 0)
        {
            throw new InvalidOperationException("The interpreter did not produce any frames for the supplied model.");
        }

        return new GenerationResult(Array.AsReadOnly(frames.ToArray()));
    }

    /// <summary>
    /// Runs the model and projects each frame through the supplied selector.
    /// </summary>
    public GenerationResult<TSymbol> Run<TSymbol>(ModelDefinition model, Func<char, TSymbol> selector, GenerationRunnerOptions? options = null)
    {
        if (selector is null) throw new ArgumentNullException(nameof(selector));
        if (model is null) throw new ArgumentNullException(nameof(model));

        options ??= new GenerationRunnerOptions();
        var frames = new List<TypedGenerationFrame<TSymbol>>();

        using var session = new GenerationSession(model, _interpreterFactory);
        session.Start(options.ResolveSeed(), options.SessionOptions);

        bool captureAll = options.CaptureIntermediateFrames;
        session.RunUntilComplete(frame =>
        {
            if (captureAll || frame.IsFinal)
            {
                frames.Add(frame.ToTyped(selector));
            }
        });

        if (frames.Count == 0)
        {
            throw new InvalidOperationException("The interpreter did not produce any frames for the supplied model.");
        }

        return new GenerationResult<TSymbol>(Array.AsReadOnly(frames.ToArray()));
    }

    /// <summary>
    /// Creates and starts a session using the configured interpreter factory.
    /// The caller owns the returned session and must dispose it.
    /// </summary>
    public GenerationSession StartSession(ModelDefinition model, GenerationRunnerOptions? options = null)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));

        options ??= new GenerationRunnerOptions();
        var session = new GenerationSession(model, _interpreterFactory);
        session.Start(options.ResolveSeed(), options.SessionOptions);
        return session;
    }
}
