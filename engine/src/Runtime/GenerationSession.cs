using System;
using System.Collections.Generic;
using System.Threading;
using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine.Runtime;

/// <summary>
/// Provides an imperative wrapper over <see cref="Interpreter"/> allowing callers to
/// step through generation, inspect intermediate frames and cancel execution.
/// </summary>
public sealed class GenerationSession : IDisposable
{
    private readonly ModelDefinition _model;
    private readonly IInterpreterFactory _interpreterFactory;

    private Interpreter? _interpreter;
    private IEnumerator<(byte[] state, char[] legend, int width, int height, int depth)>? _enumerator;
    private (byte[] state, char[] legend, int width, int height, int depth) _bufferedFrame;
    private bool _hasBufferedFrame;
    private int _stepIndex;
    private int _changeCursor;
    private bool _isCompleted;
    private bool _isCancelled;
    private bool _started;
    private IDisposable? _loggerScope;
    private bool _completionRaised;

    public GenerationSession(ModelDefinition model, IInterpreterFactory interpreterFactory)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _interpreterFactory = interpreterFactory ?? throw new ArgumentNullException(nameof(interpreterFactory));
    }

    /// <summary>Raised whenever a new frame is produced.</summary>
    public event Action<GenerationFrame>? FrameProduced;

    /// <summary>Raised when the session reaches a terminal frame.</summary>
    public event Action? Completed;

    /// <summary>Raised when the session is cancelled prior to completion.</summary>
    public event Action? Cancelled;

    public bool IsStarted => _started;

    public bool IsCompleted => _isCompleted;

    public bool IsCancelled => _isCancelled;

    public int StepsEmitted => _stepIndex;

    /// <summary>
    /// Initialises the interpreter and prepares the first frame for consumption.
    /// </summary>
    public void Start(int seed, GenerationSessionOptions? options = null)
    {
        if (_started)
        {
            throw new InvalidOperationException("The session has already been started.");
        }

        _interpreter = _interpreterFactory.CreateInterpreter(_model);
        ModelExecutionSettings execution = _model.Execution;

        bool emitIntermediates = options?.EmitIntermediateFrames ?? execution.EmitGif;
        int resolvedMaxSteps = ResolveStepBudget(options?.MaxSteps ?? execution.Steps, emitIntermediates);

        _loggerScope = options?.Logger != null ? InterpreterLogging.PushLogger(options.Logger) : null;

        _enumerator = _interpreter
            .Run(seed, resolvedMaxSteps, emitIntermediates)
            .GetEnumerator();

        _hasBufferedFrame = _enumerator.MoveNext();
        if (_hasBufferedFrame)
        {
            _bufferedFrame = _enumerator.Current;
        }
        else
        {
            CompleteSession();
        }

        _started = true;
        _stepIndex = 0;
        _changeCursor = 0;
    }

    /// <summary>
    /// Attempts to advance the session by a single frame.
    /// </summary>
    public bool TryStep(out GenerationFrame frame)
    {
        if (!_started)
        {
            throw new InvalidOperationException("The session has not been started.");
        }

        if (_isCompleted || _isCancelled || !_hasBufferedFrame)
        {
            frame = default;
            return false;
        }

        var snapshot = _bufferedFrame;
        byte[] stateCopy = CloneBuffer(snapshot.state);
        char[] legendCopy = CloneBuffer(snapshot.legend);
        GridChange[] changes = SnapshotChanges();

        bool hasNext = _enumerator!.MoveNext();
        bool isFinal = !hasNext;

        if (hasNext)
        {
            _bufferedFrame = _enumerator.Current;
        }
        else
        {
            _hasBufferedFrame = false;
        }

        frame = new GenerationFrame(stateCopy, legendCopy, snapshot.width, snapshot.height, snapshot.depth, _stepIndex, isFinal, changes);
        _stepIndex++;

        FrameProduced?.Invoke(frame);
        if (isFinal)
        {
            CompleteSession();
        }

        return true;
    }

    /// <summary>
    /// Drives the session until no further frames are available.
    /// </summary>
    public void RunUntilComplete(Action<GenerationFrame>? onFrame = null, CancellationToken cancellationToken = default)
    {
        while (!_isCompleted && !_isCancelled)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryStep(out GenerationFrame frame))
            {
                break;
            }

            onFrame?.Invoke(frame);

            if (frame.IsFinal)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Cancels the session and releases interpreter resources.
    /// </summary>
    public void Cancel()
    {
        if (_isCancelled || !_started || _isCompleted)
        {
            return;
        }

        _isCancelled = true;
        _isCompleted = true;
        DisposeEnumerator();
        DisposeLoggerScope();
        Cancelled?.Invoke();
    }

    public void Dispose()
    {
        Cancel();
    }

    private static byte[] CloneBuffer(byte[] source)
    {
        byte[] copy = new byte[source.Length];
        Array.Copy(source, copy, source.Length);
        return copy;
    }

    private static char[] CloneBuffer(char[] source)
    {
        char[] copy = new char[source.Length];
        Array.Copy(source, copy, source.Length);
        return copy;
    }

    private GridChange[] SnapshotChanges()
    {
        if (_interpreter == null)
        {
            return Array.Empty<GridChange>();
        }

        int changeEnd = _interpreter.changes.Count;
        int count = changeEnd - _changeCursor;
        if (count <= 0)
        {
            return Array.Empty<GridChange>();
        }

        GridChange[] result = new GridChange[count];
        for (int i = 0; i < count; i++)
        {
            (int x, int y, int z) change = _interpreter.changes[_changeCursor + i];
            result[i] = new GridChange(change.x, change.y, change.z);
        }

        _changeCursor = changeEnd;
        return result;
    }

    private static int ResolveStepBudget(int? requested, bool emitIntermediates)
    {
        if (!requested.HasValue)
        {
            return emitIntermediates ? 1000 : 50000;
        }

        if (requested.Value <= 0)
        {
            return 0;
        }

        return requested.Value;
    }

    private void CompleteSession()
    {
        _isCompleted = true;
        DisposeEnumerator();
        DisposeLoggerScope();
        RaiseCompletion();
    }

    private void DisposeEnumerator()
    {
        _enumerator?.Dispose();
        _enumerator = null;
        _hasBufferedFrame = false;
    }

    private void DisposeLoggerScope()
    {
        _loggerScope?.Dispose();
        _loggerScope = null;
    }

    private void RaiseCompletion()
    {
        if (_completionRaised)
        {
            return;
        }

        _completionRaised = true;
        Completed?.Invoke();
    }
}
