// Copyright: 2026 Robert Peter Meyer
// License: MIT
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
using BB84.IPTV.M3U.Editor.Application.Features;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Features;

[TestClass]
public sealed class PagedListTests
{
	[TestMethod]
	public void MetaDataShouldDescribeAPageInTheMiddle()
	{
		PagedList<int> page = new([4, 5, 6], 10, 2, 3);

		Assert.HasCount(3, page);
		Assert.AreEqual(2, page.MetaData.CurrentPage);
		Assert.AreEqual(4, page.MetaData.TotalPages);
		Assert.AreEqual(3, page.MetaData.PageSize);
		Assert.AreEqual(10, page.MetaData.TotalCount);
		Assert.IsTrue(page.MetaData.HasPrevious);
		Assert.IsTrue(page.MetaData.HasNext);
	}

	[TestMethod]
	public void MetaDataShouldReportNoPagesForAnEmptyResult()
	{
		PagedList<int> page = new([], 0, 1, 100);

		Assert.IsEmpty(page);
		Assert.AreEqual(0, page.MetaData.TotalPages);
		Assert.IsFalse(page.MetaData.HasPrevious);
		Assert.IsFalse(page.MetaData.HasNext);
	}

	[TestMethod]
	public void ConstructorShouldNotDivideByAPageSizeOfZero()
	{
		PagedList<int> page = new([], 10, 1, 0);

		Assert.AreEqual(0, page.MetaData.TotalPages);
	}

	[TestMethod]
	public void ToPagedListShouldTakeTheRequestedPage()
	{
		PagedList<int> page = Enumerable.Range(1, 10).ToPagedList(3, 3);

		Assert.AreSequenceEqual([7, 8, 9], page);
		Assert.AreEqual(10, page.MetaData.TotalCount);
		Assert.AreEqual(4, page.MetaData.TotalPages);
	}

	[TestMethod]
	public void ToPagedListShouldTakeThePageOfTheParameters()
	{
		TestParameters parameters = new() { PageNumber = 2, PageSize = 100 };

		PagedList<int> page = Enumerable.Range(1, 150).ToPagedList(parameters);

		Assert.HasCount(50, page);
		Assert.AreEqual(101, page[0]);
	}

	[TestMethod]
	public void ToPagedListShouldReturnAnEmptyPageBehindTheLastOne()
	{
		PagedList<int> page = Enumerable.Range(1, 5).ToPagedList(4, 3);

		Assert.IsEmpty(page);
		Assert.AreEqual(5, page.MetaData.TotalCount);
		Assert.IsFalse(page.MetaData.HasNext);
	}

	private sealed class TestParameters : Parameters
	{ }
}