// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Reflection;

using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;

namespace BB84.IPTV.M3U.Editor.Tests.Infrastructure;

/// <summary>
/// Runs test code on the UI thread of the headless Avalonia session.
/// </summary>
internal static class UiTest
{
	private static readonly HeadlessUnitTestSession Session =
		HeadlessUnitTestSession.GetOrStartForAssembly(Assembly.GetExecutingAssembly());

	/// <summary>
	/// Runs <paramref name="action"/> on the UI thread.
	/// </summary>
	/// <param name="action">The test body.</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public static Task RunAsync(Action action)
		=> Session.Dispatch(action, CancellationToken.None);

	/// <summary>
	/// Shows <paramref name="content"/> in a window and collects the binding messages Avalonia logs.
	/// </summary>
	/// <param name="content">The control to show, its data context is already set.</param>
	/// <returns>The collected binding messages, the window is closed afterwards.</returns>
	public static IReadOnlyList<string> Show(Control content)
	{
		IReadOnlyList<string> messages = [];
		InWindow(content, sink => messages = [.. sink.Messages]);

		return messages;
	}

	/// <summary>
	/// Shows <paramref name="content"/> in a window and runs <paramref name="body"/> while it is open.
	/// </summary>
	/// <param name="content">The control to show, its data context is already set.</param>
	/// <param name="body">The test body, it gets the sink with the binding messages so far.</param>
	public static void InWindow(Control content, Action<BindingErrorSink> body)
	{
		using BindingErrorSink sink = new();
		Window window = new() { Content = content, Width = 1280, Height = 720 };

		try
		{
			window.Show();
			Settle();
			body(sink);
		}
		finally
		{
			window.Close();
		}
	}

	/// <summary>
	/// Shows the <paramref name="window"/> and lets the layout and the bindings run.
	/// </summary>
	/// <param name="window">The window to show.</param>
	public static void ShowWindow(Window window)
	{
		window.Show();
		Settle();
	}

	/// <summary>
	/// Runs the pending dispatcher jobs and a render pass, so layout and bindings are applied.
	/// </summary>
	public static void Settle()
	{
		Dispatcher.UIThread.RunJobs();
		AvaloniaHeadlessPlatform.ForceRenderTimerTick();
		Dispatcher.UIThread.RunJobs();
	}
}