using Microsoft.VisualStudio.TestTools.UnitTesting;
using Taurit.TodoistTools.Review.Models;

namespace Taurit.TodoistTools.Review.UnitTests
{
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
            sut.originalTime = 0; // minutes

            // Act
            var newContent = sut.contentWithTime;

            // Assert
            Assert.AreNotEqual(sut.content, sut.contentWithTime);
            Assert.AreEqual("Buy carrots (10 min)", sut.contentWithTime);
        }

        [TestMethod]
        public void When_EstimatedTimeWasNotChangedByTheUser_Expect_NoChangeInContent()
        {
            // Arrange
            var sut = new TodoTask();
            sut.content = "Buy carrots (10 min) in the shop";
            sut.time = 10; // minutes
            sut.originalTime = 10; // minutes

            // Act
            var newContent = sut.contentWithTime;

            // Assert
            Assert.AreEqual(sut.content, sut.contentWithTime);
            Assert.AreEqual("Buy carrots (10 min) in the shop", sut.contentWithTime);
        }
        
    }
}
