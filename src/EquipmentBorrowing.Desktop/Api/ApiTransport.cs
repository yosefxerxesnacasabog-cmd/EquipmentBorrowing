using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EquipmentBorrowing.Application.Interfaces;

namespace EquipmentBorrowing.Desktop.Api;

internal sealed class ApiTransport(HttpClient http)
{
    private bool needsReadback;
    private sealed record ErrorBody(string Code, string Message);

    internal async Task<T?> ReadAsync<T>(string route, CancellationToken token = default, bool missingAllowed = false)
    {
        try
        {
            using var response = await http.GetAsync(route, token);
            if (response.StatusCode == HttpStatusCode.NotFound && missingAllowed)
                return default;
            if (!response.IsSuccessStatusCode)
            {
                var error = await GetErrorAsync(response, token);
                throw new StorageException(error.Message);
            }
            var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: token)
                ?? throw new JsonException("Empty API response.");
            needsReadback = false;
            return value;
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new StorageException("The API request timed out. Check the API connection and retry.");
        }
        catch (HttpRequestException)
        {
            throw new StorageException("Cannot reach the API. Start the API and check its address, then retry.");
        }
        catch (JsonException)
        {
            throw new StorageException("The API returned an unreadable response. Check the API version and retry.");
        }
    }

    internal async Task<WriteResult> WriteAsync(HttpMethod method, string route, object body)
    {
        if (needsReadback)
            return new(false, "Refresh the current records before deciding whether to retry the last change.", true, WriteFailure.Unknown);
        try
        {
            using var request = new HttpRequestMessage(method, route) { Content = JsonContent.Create(body) };
            using var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await GetErrorAsync(response, default);
                needsReadback = error.Code == "outcome_unknown";
                var failure = response.StatusCode switch
                {
                    HttpStatusCode.NotFound => WriteFailure.Missing,
                    HttpStatusCode.Conflict => WriteFailure.Conflict,
                    HttpStatusCode.BadRequest => WriteFailure.Invalid,
                    _ => needsReadback ? WriteFailure.Unknown : WriteFailure.Server
                };
                return new(false, error.Message, failure is WriteFailure.Missing or WriteFailure.Conflict or WriteFailure.Unknown, failure);
            }
            return new(true, "Changes saved successfully.");
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException)
        {
            needsReadback = true;
            return new(false, "The API connection was lost or timed out. The save outcome is unknown. Reconnect and refresh before deciding to retry.", true, WriteFailure.Unknown);
        }
    }

    private static async Task<ErrorBody> GetErrorAsync(HttpResponseMessage response, CancellationToken token)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorBody>(cancellationToken: token);
            if (error is not null && !string.IsNullOrWhiteSpace(error.Message))
                return error;
        }
        catch (JsonException) { }
        return new("server_error", $"The API returned HTTP {(int)response.StatusCode}. Retry after the server recovers.");
    }
}
