using Xunit;
using Microsoft.AspNetCore.Mvc;
using LinenManagementSystem.Controllers;
using LinenManagementSystem.Models;
using LinenManagementSystem.Services;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinenManagementSystem.DTOs;

namespace LinenManagementSystem.Tests.IntegrationTests
{
    public class CartLogControllerIntegrationTests
    {
        private readonly CartLogController _controller;
        private readonly Mock<ICartLogService> _mockService;
        private readonly Mock<ILogger<CartLogController>> _mockLogger;

        public CartLogControllerIntegrationTests()
        {
            _mockService = new Mock<ICartLogService>();
            _mockLogger = new Mock<ILogger<CartLogController>>();
            _controller = new CartLogController(_mockLogger.Object, _mockService.Object);
        }

        [Fact]
        public async Task GetCartLogs_ReturnsOkResult_WithCartLogs()
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
                    Linen = [] // Initialize with appropriate type
                },
                // Add another cart log here if necessary
            };

            // Mock service to return the correct type
            _mockService.Setup(service => service.GetCartLogsAsync("CLEAN", "HOME", 1))
                        .ReturnsAsync(cartLogs);

            // Act
            var result = await _controller.GetCartLogs("CLEAN", "HOME", 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<CartLogFetch>>(okResult.Value);
            Assert.Equal(1, returnValue.Count);  // Adjusted to 1 based on the sample data
        }

        [Fact]
        public async Task GetCartLogs_ReturnsNotFound_WhenNoLogsExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetCartLogsAsync("CLEAN", "HOME", 1))
                        .ReturnsAsync(new List<CartLogFetch>()); // Simulating no cart logs

            // Act
            var result = await _controller.GetCartLogs("CLEAN", "HOME", 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);  // Expecting NotFoundResult if no logs exist
        }

        [Fact]
        public async Task GetCartLogs_ReturnsBadRequest_ForInvalidInput()
        {
            // Act
            var result = await _controller.GetCartLogs(null, null, null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpsertCartLog_ReturnsCreatedAtAction_WhenCartLogIsUpserted()
        {
            // Arrange
            var newCartLog = new CartLogInsert
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
                Linen = [] // Initialize with appropriate type
            };

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
            };

            var cartLogFetch = new CartLogFetch
            {
                CartLogId = 26,
                ReceiptNumber = "hehehehehehhehe--",
                ReportedWeight = 50,
                ActualWeight = 51,
                Comments = "Extra blanket received",
                DateWeighed = DateTime.Parse("2024-10-08T13:41:00"),
                Cart = new CartDto { CartId = 1, Name = "Cart - Small", Weight = 20, Type = "CLEAN" },
                Location = new LocationDto { LocationId = 1, Name = "101A", Type = "CLEAN_ROOM" },
                Employee = new EmployeeDtoFetch { EmployeeId = 2, Name = "John" },
                Linen = [] // Initialize with appropriate type
            };

            _mockService.Setup(service => service.UpsertCartLogAsync(newCartLog, 2))
                        .ReturnsAsync(fetchCartLog); // Ensure to return the object

            // Act
            var result = await _controller.UpsertCartLog(newCartLog);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<CartLogFetch>(createdAtActionResult.Value);
            Assert.Equal(newCartLog.CartLogId, returnValue.CartLogId);  // Verifying that returned CartLogId matches the one upserted
        }

        [Fact]
        public async Task UpsertCartLog_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("ReceiptNumber", "Required");
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
                Linen = [] // Initialize with appropriate type
            };

            // Act
            var result = await _controller.UpsertCartLog(newCartInsertData);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCartLog_ReturnsOkResult_WhenCartLogDeleted()
        {
            // Arrange
            var cartLogId = 1;
            var employeeId = 1;
            _mockService.Setup(service => service.DeleteCartLogAsync(cartLogId, employeeId))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCartLog(cartLogId);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteCartLog_ReturnsNotFound_WhenCartLogDoesNotExist()
        {
            // Arrange
            var cartLogId = 99;
            var employeeId = 1;
            _mockService.Setup(service => service.DeleteCartLogAsync(cartLogId, employeeId))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCartLog(cartLogId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
