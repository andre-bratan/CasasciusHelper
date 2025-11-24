// -----------------------------------------------------------------------------
//  "B.*" - Utility libraries to speed up proof-of-concepts creation
//  Copyright (c) 2022-2025 Andre Bratan
// -----------------------------------------------------------------------------
//
// This file is part of the "B.*" libraries - a set of utilities originally
// created for internal and personal use. It is provided here for use in the
// CasasciusHelper project only, "as is", free of charge as long as this notice
// stays in the codebase.
//
// DISCLAIMER:
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//
// Use of this code in CasasciusHelper constitutes your acceptance of the terms
// stated above. If you wish to use parts of the "B.*" libraries in a different
// context or another project, please contact the author to obtain written
// consent.
// -----------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace B.StringUtils;

/// <summary>
/// Generates random strings
/// </summary>
public interface IRandomStringGenerator
{
    /// <summary>
    /// Generates a random Base64 string of a given length
    /// </summary>
    /// <remarks>Note: the result may include "+" and "/" characters, so it doesn't fit to be used in paths</remarks>
    string GenerateRandomBase64String(int length);
}

public class RandomStringGenerator : IRandomStringGenerator
{
    public string GenerateRandomBase64String(int length)
    {
        var randomBytes = new byte[length];

        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            rng.GetBytes(randomBytes);

        var result = Convert.ToBase64String(randomBytes);

        return result;
    }
}
