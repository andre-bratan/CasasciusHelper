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
namespace B.EnumUtils;

public static class EnumExtensions
{
    /// <summary>
    /// Checks if integer can be mapped into enumeration value <br />
    /// Warning: Do not use this method with Flags (HasFlag is needed instead of IsDefined for such cases)
    /// </summary>
    public static bool IsDefinedIn<TEnum>(this int intValue) where TEnum : Enum
    {
        var result = Enum.IsDefined(typeof(TEnum), intValue);

        return result;
    }

    /// <summary>
    /// Tries to map integer value into enumeration value
    /// </summary>
    public static TEnum GetEnumValueFor<TEnum>(this int intValue) where TEnum : Enum
    {
        if (!IsDefinedIn<TEnum>(intValue))
            throw new InvalidOperationException($"{intValue} is not a correct value for the '{typeof(TEnum).Name}' enum");

        var result = (TEnum)Enum.ToObject(typeof(TEnum), intValue);

        return result;
    }

    /// <summary>
    /// Checks if string can be mapped into enumeration value <br />
    /// Is able to work with integer parameters passed as a string) <br />
    /// Warning: Do not use this method with Flags (HasFlag is needed instead of IsDefined for such cases)
    /// </summary>
    public static bool IsDefinedIn<TEnum>(this string stringValue) where TEnum : Enum
    {
        if (string.IsNullOrWhiteSpace(stringValue))
            return false;

        if (int.TryParse(stringValue, out var intValue))
            return intValue.IsDefinedIn<TEnum>();

        var result = Enum.TryParse(typeof(TEnum), stringValue, true, out _);

        if (!result)
        {
            // one more try supposing the value may be found by stripping underscores
            var strippedStringValue = stringValue!.Replace("_", "");
            result = Enum.TryParse(typeof(TEnum), strippedStringValue, true, out _);
        }

        // Note: this method can be further improved by making it able to parse EnumMemberAttribute Values

        return result;
    }

    /// <summary>
    /// Tries to map string value into enumeration value <br />
    /// Is able to work with integer parameters passed as a string)
    /// </summary>
    public static TEnum GetEnumValueFor<TEnum>(this string stringValue) where TEnum : Enum
    {
        if (int.TryParse(stringValue, out var intValue))
            return intValue.GetEnumValueFor<TEnum>();

        if (Enum.TryParse(typeof(TEnum), stringValue, true, out var result))
            return (TEnum)result;

        // one more try supposing the value may be found by stripping underscores
        var strippedStringValue = stringValue!.Replace("_", "");
        if (Enum.TryParse(typeof(TEnum), strippedStringValue, true, out result))
            return (TEnum) result;

        // Note: this method can be further improved by making it able to parse EnumMemberAttribute Values

        throw new InvalidOperationException($"{stringValue} is not a correct value for the '{typeof(TEnum).Name}' enum");
    }
}
