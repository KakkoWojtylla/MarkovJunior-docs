#if GODOT
using System;
using Godot;
using Godot.Collections;
using MarkovJunior.Engine;
using MarkovJunior.Engine.Definitions;
using MarkovJunior.Engine.Runtime;

namespace MarkovJunior.Engine.Godot;

/// <summary>
/// Godot node that owns a <see cref="GenerationSession"/> and exposes signals for
/// frame progression and lifecycle events.
/// </summary>
[GlobalClass]
public partial class GenerationSessionNode : Node
{
    private GenerationSession? _session;

    /// <summary>The model definition that will be executed when starting the session.</summary>
    public ModelDefinition? Model { get; set; }

    /// <summary>Factory used to compile interpreters for the configured model.</summary>
    public IInterpreterFactory? InterpreterFactory { get; set; }

    /// <summary>Optional session options used when starting generation.</summary>
    public GenerationSessionOptions? Options { get; set; }

    [Signal]
    public delegate void FrameAdvancedEventHandler(Dictionary frame);

    [Signal]
    public delegate void SessionCompletedEventHandler();

    [Signal]
    public delegate void SessionCancelledEventHandler();

    /// <summary>
    /// Starts a new generation run using the provided seed.
    /// </summary>
    public void StartGeneration(int seed)
    {
        if (Model == null) throw new InvalidOperationException("Model must be assigned before starting generation.");
        if (InterpreterFactory == null) throw new InvalidOperationException("InterpreterFactory must be assigned before starting generation.");

        _session?.Dispose();
        _session = new GenerationSession(Model, InterpreterFactory);
        _session.FrameProduced += OnFrameProduced;
        _session.Completed += OnSessionCompleted;
        _session.Cancelled += OnSessionCancelled;
        _session.Start(seed, Options);
    }

    /// <summary>
    /// Advances the session by one frame.
    /// </summary>
    public bool Step()
    {
        if (_session == null)
        {
            return false;
        }

        return _session.TryStep(out _);
    }

    /// <summary>
    /// Runs the underlying session until completion, emitting frames through the
    /// <see cref="FrameAdvanced"/> signal.
    /// </summary>
    public void RunToCompletion()
    {
        _session?.RunUntilComplete();
    }

    /// <summary>
    /// Cancels the running session if present.
    /// </summary>
    public void CancelGeneration()
    {
        _session?.Cancel();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _session?.Dispose();
        _session = null;
    }

    private void OnFrameProduced(GenerationFrame frame)
    {
        EmitSignal(SignalName.FrameAdvanced, FrameToDictionary(frame));
    }

    private void OnSessionCompleted()
    {
        EmitSignal(SignalName.SessionCompleted);
    }

    private void OnSessionCancelled()
    {
        EmitSignal(SignalName.SessionCancelled);
    }

    private static Dictionary FrameToDictionary(GenerationFrame frame)
    {
        Dictionary payload = new Dictionary
        {
            { "width", frame.Width },
            { "height", frame.Height },
            { "depth", frame.Depth },
            { "step", frame.Step },
            { "is_final", frame.IsFinal },
            { "legend", frame.Legend },
            { "state", frame.State }
        };

        var changes = new Array<Dictionary>(frame.Changes.Length);
        foreach (GridChange change in frame.Changes)
        {
            changes.Add(new Dictionary
            {
                { "x", change.X },
                { "y", change.Y },
                { "z", change.Z }
            });
        }

        payload.Add("changes", changes);
        return payload;
    }
}
#endif
