using Casascius.Bitcoin;
using CasasciusHelper.Core.Services;

namespace CasasciusHelper.Core;

/// <summary>
/// Wraps Casascius ported code
/// </summary>
/// <remarks>All interractions with old Casascius cadebase must use this facade</remarks>
public interface ICasasciusFacade
{
    /// <summary>
    /// Generate a new random MiniKey
    /// </summary>
    /// <returns></returns>
    string GenerateMiniKey();

    /// <summary>
    /// Check if the given MiniKey is valid
    /// </summary>
    /// <remarks>It is recommended to use <see cref="IMiniKeyService.CheckMiniKey(byte[])"/> method instead</remarks>
    bool CheckMiniKey(string miniKey);
}

/// <inheritdoc cref="ICasasciusFacade"/>
public class CasasciusFacade : ICasasciusFacade
{
    public string GenerateMiniKey() => MiniKeyPair.CreateRandom(ExtraEntropy.GetEntropy()).MiniKey;

    public bool CheckMiniKey(string miniKey) => MiniKeyPair.IsValidMiniKey(miniKey) == 1;
}
