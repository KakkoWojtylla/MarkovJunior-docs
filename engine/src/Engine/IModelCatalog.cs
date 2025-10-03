using MarkovJunior.Engine.Definitions;

namespace MarkovJunior.Engine;

/// <summary>
/// Provides access to parsed MarkovJunior models.
/// </summary>
public interface IModelCatalog
{
    IEnumerable<ModelDefinition> GetModels();

    bool TryGet(string name, out ModelDefinition? model);
}
