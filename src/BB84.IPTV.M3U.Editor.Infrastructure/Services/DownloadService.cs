// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Net;
using System.Net.Http.Headers;

using BB84.IPTV.M3U.Editor.Application.Abstractions.Infrastructure.Services;
using BB84.IPTV.M3U.Editor.Application.Common;
using BB84.IPTV.M3U.Editor.Application.Contracts.Responses;
using BB84.IPTV.M3U.Editor.Infrastructure.Common;

using Microsoft.Extensions.Logging;

namespace BB84.IPTV.M3U.Editor.Infrastructure.Services;

/// <summary>
/// Represents the service that downloads a file from any host.
/// </summary>
/// <remarks>
/// A logo is hosted wherever its channel keeps it, so this client has no base address, unlike the
/// one that talks to the iptv-org API. A logo that cannot be downloaded is logged and skipped, it
/// never fails a whole run.
/// </remarks>
/// <param name="httpClientFactory">The http client factory instance to use.</param>
/// <param name="logger">The logger instance to use.</param>
internal sealed class DownloadService(IHttpClientFactory httpClientFactory, ILogger<DownloadService> logger) : IDownloadService
{
	public async Task<LogoDownloadResponse?> DownloadAsync(string url, string? eTag = null, CancellationToken cancellationToken = default)
	{
		if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || uri.Scheme is not ("http" or "https"))
			return null;

		try
		{
			using HttpClient client = httpClientFactory.CreateClient(Constants.DownloadClientName);
			using HttpRequestMessage request = new(HttpMethod.Get, uri);

			if (!string.IsNullOrWhiteSpace(eTag) && EntityTagHeaderValue.TryParse(eTag, out EntityTagHeaderValue? tag))
				request.Headers.IfNoneMatch.Add(tag);

			using HttpResponseMessage response = await client
				.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
				.ConfigureAwait(false);

			if (response.StatusCode is HttpStatusCode.NotModified)
				return new LogoDownloadResponse { NotModified = true, ETag = eTag };

			if (!response.IsSuccessStatusCode)
			{
				Log.LogoDownloadRefused(logger, url, (int)response.StatusCode);
				return null;
			}

			return new LogoDownloadResponse
			{
				Content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false),
				ETag = response.Headers.ETag?.ToString(),
				ContentType = response.Content.Headers.ContentType?.ToString()
			};
		}
		// Only a run the user stopped is passed on. A request that ran into the timeout of the
		// client also reports itself as cancelled, but nothing asked for it: that is a logo which
		// could not be downloaded, and it must not end the run.
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			Log.LogoDownloadFailed(logger, url, exception);
			return null;
		}
	}
}