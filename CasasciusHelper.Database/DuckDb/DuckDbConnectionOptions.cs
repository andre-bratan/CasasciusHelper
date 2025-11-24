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
namespace B.DatabaseUtils.DuckDb;

// ReSharper disable once ClassNeverInstantiated.Global
public class DuckDbConnectionOptions
{
    // All Options definitions (taken from https://duckdb.org/docs/configuration/overview.html)
    public const string OPTION_ACCESS_MODE = "access_mode";
    public const string OPTION_FILE_SEARCH_PATH = "file_search_path";
    public const string OPTION_HOME_DIRECTORY = "home_directory";
    public const string OPTION_MEMORY_LIMIT = "memory_limit";
    public const string OPTION_SCHEMA = "schema";
    public const string OPTION_SEARCH_PATH = "search_path";
    public const string OPTION_TEMP_DIRECTORY = "temp_directory";
    public const string OPTION_TIMEZONE = "TimeZone";

    /// <summary>
    /// Options that cannot be set from connection string and thus need to be applied as separate "SET" statements after the connection is open
    /// </summary>
    // TODO: re-check these options in future DuckDb versions
    public static readonly IReadOnlyList<string> OptionsToConfigureAfterOpen = new List<string>()
    {
        OPTION_TIMEZONE,
        OPTION_HOME_DIRECTORY,
        OPTION_FILE_SEARCH_PATH
    };
}
