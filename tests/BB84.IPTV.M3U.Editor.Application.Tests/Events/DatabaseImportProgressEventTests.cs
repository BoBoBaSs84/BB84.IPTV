using BB84.IPTV.M3U.Editor.Application.Events;

namespace BB84.IPTV.M3U.Editor.Application.Tests.Events;

[TestClass]
public sealed class DatabaseImportProgressEventTests
{
	[TestMethod]
	public void ConstructorShouldInitializePropertiesCorrectly()
	{
		string repositoryName = "TestRepo";
		int recordsImported = 150;
		int completedTasks = 3;
		int totalTasks = 5;

		DatabaseImportProgressEvent dbImportEvent =
			new(repositoryName, recordsImported, completedTasks, totalTasks);

		Assert.AreNotEqual(Guid.Empty, dbImportEvent.Id);
		Assert.AreEqual(repositoryName, dbImportEvent.RepositoryName);
		Assert.AreEqual(recordsImported, dbImportEvent.RecordsImported);
		Assert.AreEqual(completedTasks, dbImportEvent.CompletedTasks);
		Assert.AreEqual(totalTasks, dbImportEvent.TotalTasks);
		Assert.AreEqual(60, dbImportEvent.ProgressPercentage, "Progress percentage should be (CompletedTasks/TotalTasks)*100");
	}
}
