using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CasasciusHelper.Configuration;
using CasasciusHelper.Core;
using CasasciusHelper.Core.Data;
using CasasciusHelper.Core.Data.Import;
using CasasciusHelper.Core.Services;
using CasasciusHelper.Core.State;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CasasciusHelper;

public static class MinimalApiExtensions
{
    public static void ConfigureMinimalApiEndpoints(this WebApplication application)
    {
        application.MapGet("/address/isCasascius",
                   ([FromServices] ApplicationState applicaitonState, [FromServices] ICasasciusDataCache casasciusDataCache, string address) =>
                   {
                       if (!applicaitonState.IsReady)
                           return Results.BadRequest("Application database is empty");

                       var result = casasciusDataCache.IsCasasciusAddress(address);
                       return Results.Ok(result);
                   });

        application.MapGet("/casascius/getAddresses",
                   ([FromServices] ApplicationState applicaitonState, [FromServices] ICasasciusDataCache casasciusDataCache, string addressFilter) =>
                   {
                       if (addressFilter.Length < ApplicationConfiguration.MIN_ADDRESS_FILETER_LENGTH)
                           return Results.BadRequest($"Filter must be at least {ApplicationConfiguration.MIN_ADDRESS_FILETER_LENGTH} characters long");

                       if (!applicaitonState.IsReady)
                           return Results.BadRequest("Application database is empty");

                       var result = casasciusDataCache.SearchCasasciusAddresses(addressFilter);
                       return Results.Ok(result);
                   });

        application.MapGet(
            "/miniKey/check",
            ([FromServices] IMiniKeyService miniKeyService, string minikey) =>
            {
                var miniKeyBytes = Encoding.UTF8.GetBytes(minikey);
                var result = miniKeyService.CheckMiniKey(miniKeyBytes);

                return result;
            }
        );

        application.MapGet("/miniKey/generate", ([FromServices] ICasasciusFacade casasciusFacade, int quantity = 1) =>
        {
            if (quantity is < 1 or > 100)
                return Results.BadRequest("Quantity must be between 1 and 100");

            var results = new List<string>();
            for (var i = 0; i < quantity; i++)
                results.Add(casasciusFacade.GenerateMiniKey());

            return Results.Ok(results);
        });

        application.MapGet(
            "/miniKey/getAddress",
            ([FromServices] IMiniKeyService miniKeyService, string minikey) =>
            {
                try
                {
                    var result = miniKeyService.GetAddressFromMiniKey(minikey);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            }
        );

        application.MapGet(
            "/miniKey/getWif",
            ([FromServices] IMiniKeyService miniKeyService, string minikey, bool? skipCheck) =>
            {
                try
                {
                    var result = miniKeyService.GetWifPrivateKey(minikey, skipCheck ?? false);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            }
        );

        application.MapGet(
            "/miniKey/isCasascius",
            (
                [FromServices] ApplicationState applicaitonState,
                [FromServices] ICasasciusDataCache casasciusDataCache,
                [FromServices] IMiniKeyService miniKeyService,
                string minikey
            ) =>
            {
                if (!applicaitonState.IsReady)
                    return Results.BadRequest("Application database is empty");

                try
                {
                    var address = miniKeyService.GetAddressFromMiniKey(minikey);
                    var result = casasciusDataCache.IsCasasciusAddress(address);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            }
        );

        application.MapGet(
            "/miniKey/getSolution",
            ([FromServices] IMiniKeySolver miniKeySolver, string minikey, CancellationToken cancellationToken) =>
            {
                try
                {
                    var uncertaintyContext = miniKeySolver.GetUncertaintyContext(minikey);
                    if (uncertaintyContext.Length > ApplicationConfiguration.MAX_MINIKEY_UNCERTAINTY_ALLOWED)
                        return Results.BadRequest("Too much uncertainty");

                    var searchResult = miniKeySolver.SearchKey(minikey, cancellationToken);

                    // prevent returning empty response in case if search result is null
                    var result = searchResult is not null
                        ? Results.Ok(searchResult)
                        : Results.Ok(Array.Empty<string>());
                    return result;
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            }
        );

        application.MapGet(
            "/miniKey/getSolutions",
            ([FromServices] IMiniKeySolver miniKeySolver, string minikey, CancellationToken cancellationToken) =>
            {
                try
                {
                    var uncertaintyContext = miniKeySolver.GetUncertaintyContext(minikey);
                    if (uncertaintyContext.Length > ApplicationConfiguration.MAX_MINIKEY_UNCERTAINTY_ALLOWED)
                        return Results.BadRequest("Too much uncertainty");

                    var results = miniKeySolver.SearchKeys(minikey, cancellationToken);
                    return Results.Ok(results);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            }
        );

        application.MapGet("/health", ([FromServices] ApplicationState applicaitonState) => applicaitonState.IsHealthy);

        application.MapGet("/ping", () => "Pong");

        application.MapGet("/state", ([FromServices] ApplicationState applicaitonState) => applicaitonState);

        application.MapPost(
                "/uploadData",
                async ([FromServices] ICasasciusTrackerCsvReader casasciusTrackerCsvReader, HttpRequest request) =>
                {
                    if (!request.HasFormContentType || request.Form.Files.Count == 0)
                        return Results.BadRequest("No file uploaded.");

                    var file = request.Form.Files[0];
                    if (file.ContentType != "text/csv")
                        return Results.BadRequest("Invalid file type. Please upload a CSV file.");

                    bool importResult;
                    await using (var stream = file.OpenReadStream(/*maxAllowedSize: 10 * 1024 * 1024*/))
                        importResult = await casasciusTrackerCsvReader.ImportStreamUsingTempFile(stream);

                    if (!importResult)
                        return Results.StatusCode(500);

                    return Results.Ok();
                }
            );
    }
}
