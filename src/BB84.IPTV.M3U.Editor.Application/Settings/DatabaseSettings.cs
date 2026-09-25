// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Settings.Base;
using BB84.SourceGenerators.Attributes;

namespace BB84.IPTV.M3U.Editor.Application.Settings;

/// <summary>
/// Represents the database settings of the application, which include configuration options related to database operations,
/// such as command timeout and maximum batch size for bulk operations.
/// </summary>
public sealed class DatabaseSettings : SettingsBase
{
	private int _commandTimeout = 30;
	private int _maxBatchSize = 500;

	/// <summary>
	/// Gets or sets the command timeout for database operations, which specifies the maximum amount of time (in seconds)
	/// that a database command is allowed to execute before being terminated.
	/// </summary>
	[GenerateIniFileValue]
	public int CommandTimeout { get => _commandTimeout; set => SetProperty(ref _commandTimeout, value); }

	/// <summary>
	/// Gets or sets the maximum batch size for database operations, which determines the number of records
	/// that can be processed in a single batch when performing bulk operations, such as inserts, updates, or deletes.
	/// </summary>
	[GenerateIniFileValue]
	public int MaxBatchSize { get => _maxBatchSize; set => SetProperty(ref _maxBatchSize, value); }
}
