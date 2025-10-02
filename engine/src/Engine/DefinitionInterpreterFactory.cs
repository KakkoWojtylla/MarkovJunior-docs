using System;
using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine;

/// <summary>
/// Default implementation of <see cref="IInterpreterFactory"/> that compiles
/// XML backed models using the runtime grid builder.
/// </summary>
public sealed class DefinitionInterpreterFactory : IInterpreterFactory
{
    private readonly IGridCompiler<char> _gridCompiler;

    public DefinitionInterpreterFactory(IGridCompiler<char> gridCompiler)
    {
        _gridCompiler = gridCompiler;
    }

    public Interpreter CreateInterpreter(ModelDefinition definition)
    {
        if (definition is null) throw new ArgumentNullException(nameof(definition));

        CompiledGrid<char> compiled = _gridCompiler.CreateGrid(definition.Grid);
        if (compiled is null) throw new InvalidOperationException("Failed to create grid from definition.");

        Interpreter interpreter = Interpreter.FromDefinition(definition, compiled.Runtime);
        return interpreter;
    }
}
