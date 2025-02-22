using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using UserManagementAPI.Controllers;

namespace UserManagementAPI.Tests
{
    [TestFixture]
    public class UserControllerTests
    {
        private UserController _controller;
        private Mock<ILogger<UserController>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<UserController>>();
            _controller = new UserController(_loggerMock.Object);
        }

        [Test]
        public void CreateUser_ValidUser_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var user = new User { Name = "Test User" };

            // Act
            var result = _controller.CreateUser(user);

            // Assert
            var actionResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(actionResult);
            var returnValue = actionResult.Value as User;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(user.Name, returnValue.Name);
            Assert.Greater(returnValue.Id, 0);
        }

        [Test]
        public void CreateUser_InvalidUser_ReturnsBadRequest()
        {
            // Act
            var result = _controller.CreateUser(null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        }

        [Test]
        public void GetUser_ExistingId_ReturnsUser()
        {
            // Arrange
            var user = new User { Name = "Test User" };
            _controller.CreateUser(user);

            // Act
            var result = _controller.GetUser(user.Id);

            // Assert
            var actionResult = result.Result as OkObjectResult;
            Assert.IsNotNull(actionResult);
            var returnValue = actionResult.Value as User;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(user.Name, returnValue.Name);
        }

        [Test]
        public void GetUser_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = _controller.GetUser(999);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public void UpdateUser_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var user = new User { Name = "Test User" };
            _controller.CreateUser(user);
            var updatedUser = new User { Name = "Updated User" };

            // Act
            var result = _controller.UpdateUser(user.Id, updatedUser);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public void UpdateUser_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var updatedUser = new User { Name = "Updated User" };

            // Act
            var result = _controller.UpdateUser(999, updatedUser);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public void DeleteUser_ExistingId_ReturnsNoContent()
        {
            // Arrange
            var user = new User { Name = "Test User" };
            _controller.CreateUser(user);

            // Act
            var result = _controller.DeleteUser(user.Id);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public void DeleteUser_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = _controller.DeleteUser(999);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }

        [Test]
        public void GetAllUsers_ReturnsAllUsers()
        {
            // Arrange
            var user1 = new User { Name = "Test User 1" };
            var user2 = new User { Name = "Test User 2" };
            _controller.CreateUser(user1);
            _controller.CreateUser(user2);

            // Act
            var result = _controller.GetAllUsers();

            // Assert
            var actionResult = result.Result as OkObjectResult;
            Assert.IsNotNull(actionResult);
            var returnValue = actionResult.Value as List<User>;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(2, returnValue.Count);
        }
    }
}