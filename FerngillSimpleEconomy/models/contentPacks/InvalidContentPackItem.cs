using System;
using System.Text.Json.Serialization;

namespace fse.core.models.contentPacks;

public class InvalidContentPackItem : BaseContentPackItem
{
	public string? Action { get; init; }
	public string? RawText { get; init; }
	public Exception? Exception { get; init; }
}