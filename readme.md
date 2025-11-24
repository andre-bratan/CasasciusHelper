# Casascius Helper

Casascius codebase resurrected... Well, not really, not fully :) 

<img src="Screenshots/2025.11.24_Home%20page.png" alt="Main page" style="width:100%; display: block; margin: auto;">

### What is Casascius?

- [Physical Bitcoins by Casascius](https://www.casascius.com/)
- [Casascius Bitcoin Analyzer](https://casascius.uberbills.com/)

### Features

- Decode ancient "[mini-keys](https://en.bitcoin.it/wiki/Mini_private_key_format)" (those 22 or 30 symbols printed under coins' protective hologram) into WIF ([wallet import format](https://en.bitcoin.it/wiki/Wallet_import_format)) private keys
  - Warning: The application's codebase contains some [example mini-keys](./Casascius.Coins/CasasciusKnownCoins.cs) - they were obtained from open sources over the Internet and are used only for testing/demonstration purposes. **Do not top up their addresses** as their private keys are publicly known!
- Calculate a "mini-key's" corresponding wallet address
- Display known information on Casascius Coins (by using data from third-party sources - see below).
  - The application caches needed data in its local database.
- Able to **search for a correct private key (in case when some of its parts are lost/worn/unreadable)**.
  - Yes, it's a simple "brute-force" search,
  - **No, it will not help you get someone's else private key xD**
  - It is recommend to use this recovery method only if you are not sure in **up to 6 symbols** of your mini-key).
    - The more the "uncertainty level" the longer it takes to enumarate all possible combinations.
    - By default, the application is configured to accept up to 4 symbols of uncertainty.
- Sign a message using a "mini-key", verify it using a corresponding public key (address). 
  - **Warning: CasasciusHelper is the ASP.NET Blazor Server application. "Server" means your "mini-key" will be sent to the server in order to generate a signature - never use this feature if you don't trust a machine running a copy of this application!** 
- Web UI
- **REST API for possible integration with other utilities** - see the [screenshot](Screenshots/2025.11.24_API.png)
- Docker image

### How to use

- Comile and run or use Docker (see example below)
- Open UI in browser
- Import data by pressing
  - the "Download data" button next to the "Database status" badge - for getting the data from the Internet
  - the "Upload CSV" button next to "Database status" badge - for uploading a data snapshot from a CSV file (you may use one of [presaved](./DataSnapshots/readme.md))
  - Note: Updates are done manually by repeating this process
- Have fun ;)

### Features (code)

- Contains **core part** of old Casascius [Bitcoin-Address-Utility](https://github.com/casascius/Bitcoin-Address-Utility) ported to .Net Core and more up-to-date version of [BouncyCastle.Cryptography](https://github.com/bcgit/bc-csharp)
  - See [port docs](./Casascius.Port/readme.md)
- Contains some parts of old Casascius codebase rewritten using [NBitcoin](https://github.com/MetacoSA/NBitcoin) library 

### Docker

> [!NOTE]  
> As the application is meant to deal with potentially **secret data**, it doesn't make sense to publish its Docker image to the Docker Hub - everyone interested **must audit the code and build it for themselves**. 

1. Build a local Docker image of the application:
```shell
docker build -t casasciushelper:latest .
```
2. You probably want to use a _persistent volume_ to avoid importing data every time the application is restarted:
```shell
docker volume create CasasciusHelper
```
3. Run the application:
```shell
docker run -it --rm --name CasasciusHelper -e ASPNETCORE_ENVIRONMENT=Staging -p 80:8080 -v CasasciusHelper:/app/Data casasciushelper:latest
```
4. Navigate to <http://localhost> in your browser.  
5. Press `Ctrl+C` (in the console you've used to run the application) to stop and exit.

### Possible improvements

- [x] Import current coins status from CSV
- [x] Import current coins status from an online third-party source
- [x] Signing messages and checking signatures
  - [ ] REST enpoints to do the same
- [ ] Balance check (using a public Bitcoin node RPC)
- [ ] Redeem a coin (using a private [Bitcoin node](https://bitcoin.org/en/download) RPC)
  - Note: There are lots of ways to redeem a coin, usually this feature is called something like "Sweep a private key" in supporting Bitcoin wallets.

### Integrated third-party data sources

- [ ] [Casascius Bitcoin Analyzer](https://casascius.uberbills.com/)
- [x] [Casascius Tracker](https://casasciustracker.com)
  - Note: The data source was spotted to go down sometimes (for example, in September 2025). So there are some pre-saved snapshots of its data in the [DataSnapshots](./DataSnapshots/readme.md) folder. These snapshots may be imported using the "Upload CSV" button.

### Software Bill of Materials

Kudos to all who made these wonderful building blocks:
- [Bitcoin Core](https://bitcoin.org/en/bitcoin-core/) and the Blockchain itself (Satoshi Nakamoto)
- [.Net Core](https://dotnet.microsoft.com/en-us/)
- [Blazor](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
- Casascius "[Bitcoin Address Utility](https://github.com/casascius/Bitcoin-Address-Utility)" (including CryptSharp)
- [BouncyCastle.Cryptography](https://github.com/bcgit/bc-csharp)
- [NBitcoin](https://github.com/MetacoSA/NBitcoin)
- [DuckDb](https://duckdb.org/) used with [DuckDB.NET](https://github.com/Giorgi/DuckDB.NET) and [Dapper](https://github.com/DapperLib/Dapper)
- and many more...

### Code style notes

As the project contains parts of old codebase, it was decided to avoid using some of latest C# features:
- Collection initializers
- Primary constructors
- Structs/Records

Probably using file-scoped namespaces was not a good idea also for the ported code as it made patch files more difficult to read...

Other:
- `if (something is null) ...` is preferred over `if (something == null) ...` as "=="/"!=" operators can be overriden in C#.

### Contributing

This project is currently in a **"Portfolio"** state, so pull requests (PRs) are not actively solicited.

### Disclaimers

The project was built _just for fun_.  
I do not own any Casascius coins and I am not affiliated with any related entity.

### Acknowledgments

Thank you for everyone who made this project possible!

Address for acknowledgments if any:  
[![Bitcoin Thanks](https://img.shields.io/badge/Bitcoin-1MB2rSidXHKfKTaGiFcYEVzCf7iANHzpMH-grey?logo=bitcoin&logoColor=white&labelColor=FF9900)](bitcoin:1MB2rSidXHKfKTaGiFcYEVzCf7iANHzpMH)  
