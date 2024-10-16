using Xunit;
using Moq;
using LinenManagementSystem.Services;
using LinenManagementSystem.Repositories;
using LinenManagementSystem.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using LinenManagementSystem.DTOs;

namespace LinenManagementSystem.Tests.UnitTests
{
    public class CartLogServiceTests
    {
        private readonly CartLogService _cartLogService;
        private readonly Mock<ICartLogRepository> _mockRepo;

        public CartLogServiceTests()
        {
            _mockRepo = new Mock<ICartLogRepository>();
            _cartLogService = new CartLogService(_mockRepo.Object);
        }

        [Fact]
        public async Task GetCartLogsAsync_ReturnsCartLogs()
        {
            // Arrange
            var cartLogs = new List<CartLogFetch>
{
    new CartLogFetch
    {
        CartLogId = 26,
    ReceiptNumber = "hehehehehehhehe--",
    ReportedWeight = 50,
    ActualWeight = 51,
    Comments = "Extra blanket received",
    DateWeighed = DateTime.Parse("2024-10-08T13:41:00"),
    Cart = new CartDto
    {
        CartId = 1,
        Name = "Cart - Small",
        Weight = 20,
        Type = "CLEAN"
    },
    Location = new LocationDto
    {
        LocationId = 1,
        Name = "101A",
        Type = "CLEAN_ROOM"
    },
    Employee = new EmployeeDtoFetch
    {
        EmployeeId = 2,
        Name = "John"
    },
    Linen = []
    }
};





            // Mock repository response for the specific parameters
            _mockRepo
                .Setup(repo => repo.GetCartLogsAsync("CLEAN", "HOME", 1))
        .ReturnsAsync(cartLogs); // ReturnsAsync should match the return type of GetCartLogsAsync

            // Act
            var result = await _cartLogService.GetCartLogsAsync("CLEAN", "101A", 2);

            // Assert
            Assert.Equal(2, result.Count()); // IEnumerable, so use Count()
            Assert.Equal(1, result.First().CartLogId); // First log should have CartLogId 1
            Assert.Equal(2, result.Last().CartLogId); // Last log should have CartLogId 2
        }

        [Fact]
        public async Task GetCartLogsAsync_ReturnsEmptyListWhenNoCartLogs()
        {
            // Arrange
            _mockRepo
                .Setup(repo => repo.GetCartLogsAsync("INVALID_TYPE", "INVALID_LOCATION", 0))
                .ReturnsAsync(new List<CartLogFetch>());

            // Act
            var result = await _cartLogService.GetCartLogsAsync("INVALID_TYPE", "INVALID_LOCATION", 0);

            // Assert
            Assert.Empty(result); // Expecting no cart logs
        }

        [Fact]
        public async Task UpsertCartLogAsync_AddsNewCartLog()
        {
            // Arrange
            var fetchCartLog = new CartLog
            {
                CartLogId = 26,
                ReceiptNumber = "hehehehehehhehe--",
                ReportedWeight = 50,
                ActualWeight = 51,
                Comments = "Extra blanket received",
                DateWeighed = DateTime.Parse("2024-10-08T13:41:00"),
                CartId = 1,
                LocationId = 1,
                EmployeeId = 2,
                // Cart = new CartDto
                // {
                //     CartId = 1,
                //     Name = "Cart - Small",
                //     Weight = 20,
                //     Type = "CLEAN"
                // },
                // Location = new LocationDto
                // {
                //     LocationId = 1,
                //     Name = "101A",
                //     Type = "CLEAN_ROOM"
                // },
                // Employee = new EmployeeDtoFetch
                // {
                //     EmployeeId = 2,
                //     Name = "John"
                // },
                // Linen = []
            };

            var newCartInsertData = new CartLogInsert
            {
                CartLogId = 26,
                ReceiptNumber = "hehehehehehhehe--",
                ReportedWeight = 50,
                ActualWeight = 51,
                Comments = "Extra blanket received",
                DateWeighed = DateTime.Parse("2024-10-08T13:41:00"),
                CartId = 1,
                LocationId = 1,
                EmployeeId = 2,
                Linen = []
            };

            // Mock repository for upserting
            _mockRepo
       .Setup(repo => repo.UpsertCartLogAsync(newCartInsertData))
       .Returns(Task.FromResult(fetchCartLog));

            // Act
            var result = await _cartLogService.UpsertCartLogAsync(newCartInsertData, 2);

            // Assert
            Assert.Equal(3, result.CartLogId); // CartLogId should match the new cart log
            Assert.Equal(3, result.EmployeeId); // EmployeeId should match
        }

        [Fact]
        public async Task DeleteCartLogAsync_ReturnsTrue_WhenCartLogExists()
        {
            // Arrange
            var cartLogId = 1;
            var employeeId = 1;

            // Mock repository for successful delete
            _mockRepo
                .Setup(repo => repo.DeleteCartLogAsync(cartLogId, employeeId))
                .ReturnsAsync(true);

            // Act
            var result = await _cartLogService.DeleteCartLogAsync(cartLogId, employeeId);

            // Assert
            Assert.True(result); // Deletion should return true
        }

        [Fact]
        public async Task DeleteCartLogAsync_ReturnsFalse_WhenCartLogDoesNotExist()
        {
            // Arrange
            var cartLogId = 99;
            var employeeId = 1;

            // Mock repository for failed delete
            _mockRepo
                .Setup(repo => repo.DeleteCartLogAsync(cartLogId, employeeId))
                .ReturnsAsync(false);

            // Act
            var result = await _cartLogService.DeleteCartLogAsync(cartLogId, employeeId);

            // Assert
            Assert.False(result); // Deletion should return false
        }
    }
}
