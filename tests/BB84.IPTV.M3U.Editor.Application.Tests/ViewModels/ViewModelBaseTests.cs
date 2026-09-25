// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

namespace BB84.IPTV.M3U.Editor.Application.Tests.ViewModels;

[TestClass]
public sealed class ViewModelBaseTests
{
	[TestMethod]
	public async Task InvokeShouldGoThroughTheContextWhenItIsCalledFromAnotherThread()
	{
		RecordingContext context = new();
		TestViewModel sut = WithContext(context, () => new TestViewModel());

		await Task.Run(() => sut.Run()).ConfigureAwait(false);

		Assert.AreEqual(1, context.Posts, "The error callback of a command runs on a thread pool thread.");
		Assert.AreEqual(1, sut.Calls);
	}

	[TestMethod]
	public void InvokeShouldRunStraightAwayOnTheThreadTheContextBelongsTo()
	{
		RecordingContext context = new();
		TestViewModel sut = WithContext(context, () => new TestViewModel());

		WithContext(context, () =>
		{
			sut.Run();
			return true;
		});

		Assert.AreEqual(0, context.Posts);
		Assert.AreEqual(1, sut.Calls);
	}

	[TestMethod]
	public void InvokeShouldRunStraightAwayWhenThereIsNoContext()
	{
		TestViewModel sut = WithContext(null, () => new TestViewModel());

		sut.Run();

		Assert.AreEqual(1, sut.Calls);
	}

	/// <summary>
	/// Creates something while the given context is the current one, the way the container builds
	/// a view model on the thread the user interface belongs to.
	/// </summary>
	private static T WithContext<T>(SynchronizationContext? context, Func<T> create)
	{
		SynchronizationContext? previous = SynchronizationContext.Current;

		try
		{
			SynchronizationContext.SetSynchronizationContext(context);
			return create();
		}
		finally
		{
			SynchronizationContext.SetSynchronizationContext(previous);
		}
	}

	/// <summary>
	/// Counts what was posted to it and runs it straight away, so a test does not have to wait.
	/// </summary>
	private sealed class RecordingContext : SynchronizationContext
	{
		private int _posts;

		public int Posts
			=> _posts;

		public override void Post(SendOrPostCallback d, object? state)
		{
			_ = Interlocked.Increment(ref _posts);
			d(state);
		}
	}

	private sealed class TestViewModel : ViewModelBase
	{
		public int Calls { get; private set; }

		public void Run()
			=> Invoke(() => Calls++);
	}
}
