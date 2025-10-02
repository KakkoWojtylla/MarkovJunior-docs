using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine;

/// <summary>
/// Compiles model definitions into runnable interpreter instances.
/// </summary>
public interface IInterpreterFactory
{
    Interpreter CreateInterpreter(ModelDefinition definition);
}
