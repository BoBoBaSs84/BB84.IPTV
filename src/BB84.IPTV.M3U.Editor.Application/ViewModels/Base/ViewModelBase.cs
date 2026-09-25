using BB84.Notifications;

namespace BB84.IPTV.M3U.Editor.Application.ViewModels.Base;

/// <summary>
/// The base class for all view models.
/// </summary>
public abstract class ViewModelBase : NotifiableObject
{
	private readonly SynchronizationContext? _synchronizationContext = SynchronizationContext.Current;

	/// <summary>
	/// Runs the action on the thread the view model was created on, the one the user interface
	/// belongs to.
	/// </summary>
	/// <remarks>
	/// A subscription of the event service and the error callback of an asynchronous command both
	/// run on a thread pool thread. A property a control is bound to must not be set there: the
	/// control verifies the thread while it handles the change and throws, which ends the
	/// application.
	/// </remarks>
	/// <param name="action">The action to run.</param>
	/// <exception cref="ArgumentNullException">Thrown when the action is <see langword="null"/>.</exception>
	protected void Invoke(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		if (_synchronizationContext is null || _synchronizationContext == SynchronizationContext.Current)
			action();
		else
			_synchronizationContext.Post(_ => action(), null);
	}
}
