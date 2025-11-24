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
namespace B.DiskUtils;

public interface IPathUtils
{
    /// <summary>
    /// Fixes path to use correct directory separator
    /// </summary>
    /// <remarks>This method trims trailing directory separator character</remarks>
    string NormalizePath(string path);

    // /// <summary>
    // /// Fixes path to use correct directory separator
    // /// </summary>
    // /// <remarks>This method trims trailing directory separator character</remarks>
    // string NormalizePath(string path);
}

/// <inheritdoc cref="IPathUtils"/>
public class PathUtils : IPathUtils
{
    private static readonly char WrongDirectorySeparator = Path.DirectorySeparatorChar == '/' ? '\\' : '/';

    public string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (path.Contains(WrongDirectorySeparator))
            path = path.Replace(WrongDirectorySeparator, Path.DirectorySeparatorChar);

        return path;
    }
}
