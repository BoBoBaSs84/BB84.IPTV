// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
namespace BB84.IPTV.M3U.Editor.Application.Enumerators;

/// <summary>
/// Represents the result of a notification dialog.
/// </summary>
public enum NotificationResult
{
	/// <summary>
	/// No result.
	/// </summary>
	None = 0,

	/// <summary>
	/// The user selected OK.
	/// </summary>
	OK = 1,

	/// <summary>
	/// The user selected Cancel.
	/// </summary>
	Cancel = 2,

	/// <summary>
	/// The user selected Yes.
	/// </summary>
	Yes = 3,

	/// <summary>
	/// The user selected No.
	/// </summary>
	No = 4,

	/// <summary>
	/// The user selected Retry.
	/// </summary>
	Retry = 5
}
