using Microsoft.VisualStudio.TestTools.UnitTesting;
using Taurit.TodoistTools.Review.Models;

namespace Taurit.TodoistTools.Review.UnitTests;

[TestClass]
public class TodoTaskTests
{
    [TestMethod]
    public void When_EstimatedTimeWasChangedByTheUser_Expect_ContentToUpdate()
    {
        // Arrange
        var sut = new TodoTask();
        sut.content = "Buy carrots";
        sut.time = 10; // minutes

        // Act
        var newContent = sut.contentWithTime;

        // Assert
        Assert.AreNotEqual(sut.content, newContent);
        Assert.AreEqual("Buy carrots (10 min)", newContent);
    }

    [TestMethod]
    public void When_EstimatedTimeWasNotChangedByTheUser_Expect_NoChangeInContent()
    {
        // Arrange
        var sut = new TodoTask();
        sut.content = "Buy carrots (10 min) in the shop";
        sut.SetOriginalDurationInMinutes(10);
        sut.time = 10; // minutes

        // Act
        var newContent = sut.contentWithTime;

        // Assert
        Assert.AreEqual(sut.content, newContent);
        Assert.AreEqual("Buy carrots (10 min) in the shop", newContent);
    }


    [TestMethod]
    public void When_EstimatedTimeIsDefined_Expect_AnotherEstimatedTimeWontBeAppended()
    {
        // Arrange
        var sut = new TodoTask();
        sut.content = "Buy carrots (10 min) in the shop";
        sut.SetOriginalDurationInMinutes(20);
        sut.time = 10; // minutes

        // Act
        var newContent = sut.contentWithTime;

        // Assert
        Assert.AreEqual(sut.content, newContent);
        Assert.AreEqual("Buy carrots (10 min) in the shop", newContent);
    }

    [TestMethod]
    public void When_EstimatedTimeIsDefinedAs0min_Expect_AnotherEstimatedTimeWontBeAppended()
    {
        // Arrange
        var sut = new TodoTask();
        sut.content = "Buy carrots in the shop (0 min)";
        sut.SetOriginalDurationInMinutes(0);
        sut.time = 10; // minutes

        // Act
        var newContent = sut.contentWithTime;

        // Assert
        Assert.AreEqual(sut.content, newContent);
        Assert.AreEqual("Buy carrots in the shop (0 min)", newContent);
    }
}
