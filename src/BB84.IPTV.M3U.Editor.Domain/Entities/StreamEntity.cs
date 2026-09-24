using BB84.IPTV.M3U.Editor.Domain.Entities.Base;

namespace BB84.IPTV.M3U.Editor.Domain.Entities;

public sealed class StreamEntity : EntityBase
{
	public string? Channel { get; set; }
	public string? Feed { get; set; }
	public required string Title { get; set; }
	public required string Url { get; set; }
	public string? Referrer { get; set; }
	public string? UserAgent { get; set; }
	public string? Quality { get; set; }
}
