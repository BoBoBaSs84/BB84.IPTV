// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

/// <summary>
/// Represents a stream entity in the IPTV M3U editor domain, which is one playable source of a
/// channel or feed.
/// </summary>
public sealed class StreamEntity : EntityBase
{
	/// <summary>
	/// Gets or sets the channel identifier the stream belongs to.
	/// </summary>
	public string? Channel { get; set; }

	/// <summary>
	/// Gets or sets the feed identifier the stream belongs to, which is not set for a stream that
	/// is not tied to one feed.
	/// </summary>
	public string? Feed { get; set; }

	/// <summary>
	/// Gets or sets the title of the stream.
	/// </summary>
	public required string Title { get; set; }

	/// <summary>
	/// Gets or sets the URL the stream is played from.
	/// </summary>
	public required string Url { get; set; }

	/// <summary>
	/// Gets or sets the value of the <c>Referer</c> request header the stream needs, if any.
	/// </summary>
	public string? Referrer { get; set; }

	/// <summary>
	/// Gets or sets the value of the <c>User-Agent</c> request header the stream needs, if any.
	/// </summary>
	public string? UserAgent { get; set; }

	/// <summary>
	/// Gets or sets the largest quality the stream is served in, e.g. <c>1080p</c>.
	/// </summary>
	public string? Quality { get; set; }
}
