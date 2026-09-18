using System.Reflection;

namespace BB84.IPTV.M3U.Editor.Presentation.Tests;

public abstract class PresentationTestBase
{
	protected static object? GetNonPublicField(object target, string fieldName)
	{
		FieldInfo? field = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.Name == fieldName);
		return field?.GetValue(target);
	}

	protected static object? GetNonPublicProperty(object target, string propertyName)
	{
		PropertyInfo? property = target.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(p => p.Name == propertyName);
		return property?.GetValue(target);
	}

	protected static void InvokeNonPublicMethod(object target, string methodName, params object[] args)
	{
		MethodInfo? method = target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(m => m.Name == methodName);
		_ = method?.Invoke(target, args);
	}
}
