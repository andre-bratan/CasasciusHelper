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

using B.StringUtils;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace B.DiskUtils;

public interface IDirectoryAccessChecker
{
    /// <summary>
    /// Checks if it is possible to create, write, read and delete files in the specified <paramref name="directoryPath"/>
    /// </summary>
    /// <remarks>Will return <c>false</c> in case if <paramref name="directoryPath"/> doesn't exist</remarks>
    Task<bool> CanWriteAndReadFiles(string directoryPath, CancellationToken cancellationToken = default);
}

public class DirectoryAccessChecker : IDirectoryAccessChecker
{
    private const string TEST_FILE_EXTENSION = ".tmp";
    private const string TEST_FILE_CONTENTS = "test";

    private readonly IRandomStringGenerator randomStringGenerator;
    private readonly ILogger<DirectoryAccessChecker>? logger;

    public DirectoryAccessChecker(
        IRandomStringGenerator randomStringGenerator,
        ILogger<DirectoryAccessChecker>? logger = null)
    {
        this.randomStringGenerator = randomStringGenerator;
        this.logger = logger;
    }

    public async Task<bool> CanWriteAndReadFiles(string directoryPath, CancellationToken cancellationToken = default)
    {
        var tempFilename = randomStringGenerator.GenerateRandomBase64String(6).Replace('/', '='); // Note: the resulting string will be 8 characters long
        var timeFilenameWithExtension = $"{DateTime.Now:yyyyMMddHHmmss}_{tempFilename}{TEST_FILE_EXTENSION}";
        var tempFilePath = Path.Combine(directoryPath, timeFilenameWithExtension);

        try
        {
            // The following commented code is redundant as WriteAllTextAsync will throw DirectoryNotFoundException in case if the required directory doesn't exist
            // if (!Directory.Exists(directoryPath))
            //     return false;

            await File.WriteAllTextAsync(tempFilePath, TEST_FILE_CONTENTS, cancellationToken);
            var tempFileContents = await File.ReadAllTextAsync(tempFilePath, cancellationToken);
            File.Delete(tempFilePath);

            if (tempFileContents != TEST_FILE_CONTENTS)
                return false;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error while checking directory access");

            return false;
        }

        return true;
    }
}
