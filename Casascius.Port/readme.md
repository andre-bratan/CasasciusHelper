# Casascius Port to .Net Core 8

**Disclaimer:**
```
THIS SOFTWARE PORT IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
```

## CryptSharp

This is a part of CryptSharp library taken from [Casascius code base](https://github.com/casascius/Bitcoin-Address-Utility/tree/dcfc3b99a3df1427fc19fcfbe18c1bfedfdad4eb/CryptSharp).  

No significant changes were made except cosmetic ones (like removing unused "using"s and switching to file-scoped namespaces).

## "Model"

This is a "Model"-only part (core logic) of old Cascasius [Bitcoin Address Utility](https://github.com/casascius/Bitcoin-Address-Utility/tree/dcfc3b99a3df1427fc19fcfbe18c1bfedfdad4eb) ported to:
- .NET Core (v8) 
- [BouncyCastle.Cryptography](https://github.com/bcgit/bc-csharp) library (v2.6.2).

Changes made:
- as "RIPEMD160" was [removed from .NET Core](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.ripemd160?view=netframework-4.8)
  - [x] switched to using BouncyCastle's RIPEMD160 implementation
  - [ ] ~~switched to using [custom RIPEMD160 implementation](https://github.com/darrenstarr/RIPEMD160.net)~~ made by _Darren R. Starr_ (thank you!)
  - both work fine, but the latter was declined in favor of "less dependencies"
- new version of _BouncyCastle_ does not support "compressed" parameter in `Org.BouncyCastle.Math.EC.ECCurve.CreatePoint` method. So:
  - used `Org.BouncyCastle.Math.EC.ECPoint.GetEncoded(compressed)`
  - then `Org.BouncyCastle.Math.EC.ECCurve.DecodePoint(encoded)` instead.
- new version of _BouncyCastle_ does not support `Org.BouncyCastle.Math.EC.ECPoint.X` (and `Y`) property. So:
  - used `Org.BouncyCastle.Math.EC.ECPoint.Normalize` 
  - then `Org.BouncyCastle.Math.EC.ECPoint.AffineXCoord` (and `AffineYCoord`) instead.
- to avoid `SCrypt` namespace conflicts (between `CryptSharp.Utility.SCrypt` and `Org.BouncyCastle.Crypto.Generators.SCrypt`) the following using statement was addded: `using SCrypt = CryptSharp.Utility.SCrypt;`

>[!Warning]  
> Some changes made may not be "the right" from a cryptographic point of view! USE THE RESULT AT YOUR OWN RISK!

You can run/debug the `Casascius.Port.Tests.CasasciusPortTests.Test_PortQuality` "smoke" test to investigate further  
OR  
See the provided "_2025.08.24_Casascius port to NetCore8.patch_" file for more details and your convenience.
