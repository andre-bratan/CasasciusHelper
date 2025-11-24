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
namespace B.DatabaseUtils.DuckDb.Configuration;

public class DuckDbSettings
{
    /// <summary>
    /// Database filename (without path)
    /// </summary>
    /// <remarks>Can also be ":memory:" as well as ":memory:?cache=shared"</remarks>
    public string DataSource { get; set; } = ":memory:";

    /// <summary>
    /// A comma separated list of directories to search for input files
    /// </summary>
    /// <remarks>Important if you don't want to specify paths in CSV file references</remarks>
    public string? FileSearchPath { get; set; }

    /// <summary>
    /// Default database file location
    /// </summary>
    public string? HomeDirectory { get; set; }

    /// <summary>
    /// Set the directory to which to write temp files
    /// </summary>
    /// <remarks>Set to <c>NULL</c> or <c>empty string</c> to disable</remarks>
    public string TempDirectory { get; set; } = "";

    /// <summary>
    /// The maximum memory of the system (e.g., 1GB)
    /// </summary>
    /// <remarks>Default is 80% of RAM</remarks>
    public string MemoryLimit { get; set; } = "1Gb";

    /// <summary>
    /// Conneciton Time Zone
    /// </summary>
    public string TimeZone { get; set; } = "UTC";
}
