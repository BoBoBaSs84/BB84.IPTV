// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using System.Reflection;

using BB84.IPTV.M3U.Editor.Application.Common;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Common;

[TestClass]
public sealed class LogEventsTests
{
	/// <summary>
	/// The category a range belongs to, in the order the ranges are handed out.
	/// </summary>
	private static readonly string[] Categories =
		[nameof(LogEvents.Application), nameof(LogEvents.Database), nameof(LogEvents.File), nameof(LogEvents.Web),
		 nameof(LogEvents.Logo), nameof(LogEvents.Settings), nameof(LogEvents.Events), nameof(LogEvents.Presentation)];

	[TestMethod]
	public void EveryEventIdShouldBeUsedOnce()
	{
		int[] eventIds = [.. GetEventIds().Select(entry => entry.Value)];

		Assert.AreEqual(eventIds.Length, eventIds.Distinct().Count(), "An event id says which entry it is, so it is used once.");
	}

	[TestMethod]
	public void EveryEventIdShouldStayInTheRangeOfItsCategory()
	{
		foreach ((string category, string name, int value) in GetEventIds())
		{
			int lowerBound = (Array.IndexOf(Categories, category) + 1) * 1000;

			Assert.IsTrue(
				value > lowerBound && value < lowerBound + 1000,
				$"'{category}.{name}' is {value}, which is outside of {lowerBound + 1} to {lowerBound + 999}.");
		}
	}

	/// <summary>
	/// Returns every event id with the category it belongs to.
	/// </summary>
	/// <returns>The category, the name and the value of every event id.</returns>
	private static IEnumerable<(string Category, string Name, int Value)> GetEventIds()
		=> typeof(LogEvents)
			.GetNestedTypes(BindingFlags.Public)
			.SelectMany(category => category
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Select(field => (category.Name, field.Name, (int)field.GetValue(null)!)));

	[TestMethod]
	public void EveryCategoryShouldHaveARange()
	{
		string[] categories = [.. typeof(LogEvents).GetNestedTypes(BindingFlags.Public).Select(category => category.Name)];

		Assert.AreEqual(Categories.Length, categories.Length, "A category without a range has no event id to hand out.");
		CollectionAssert.AreEquivalent(Categories, categories);
	}
}
