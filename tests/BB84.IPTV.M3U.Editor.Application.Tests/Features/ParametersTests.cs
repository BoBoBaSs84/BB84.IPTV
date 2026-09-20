// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Features;

[TestClass]
public sealed class ParametersTests
{
	[TestMethod]
	public void DefaultsShouldBeTheFirstPageWithTheLargestSize()
	{
		TestParameters parameters = new();

		Assert.AreEqual(1, parameters.PageNumber);
		Assert.AreEqual(Parameters.MaxPageSize, parameters.PageSize);
		Assert.AreEqual(0, parameters.Skip);
	}

	[TestMethod]
	[DataRow(0, 1)]
	[DataRow(-5, 1)]
	[DataRow(3, 3)]
	public void PageNumberShouldNeverBeSmallerThanOne(int value, int expected)
	{
		TestParameters parameters = new() { PageNumber = value };

		Assert.AreEqual(expected, parameters.PageNumber);
	}

	[TestMethod]
	[DataRow(0, Parameters.MinPageSize)]
	[DataRow(50, Parameters.MinPageSize)]
	[DataRow(250, 250)]
	[DataRow(5000, Parameters.MaxPageSize)]
	public void PageSizeShouldStayWithinTheAllowedRange(int value, int expected)
	{
		TestParameters parameters = new() { PageSize = value };

		Assert.AreEqual(expected, parameters.PageSize);
	}

	[TestMethod]
	public void SkipShouldBeTheItemsOfThePagesBefore()
	{
		TestParameters parameters = new() { PageNumber = 4, PageSize = 250 };

		Assert.AreEqual(750, parameters.Skip);
	}

	private sealed class TestParameters : Parameters
	{ }
}