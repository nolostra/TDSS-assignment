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
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LinenManagementSystem.Tests.IntegrationTests
{
    public class CartLogControllerIntegrationTests
    {
        private readonly CartLogController _controller;
        private readonly Mock<ICartLogService> _mockService;
        private readonly Mock<ILogger<CartLogController>> _mockLogger;
        private readonly ClaimsPrincipal _user;

        public CartLogControllerIntegrationTests()
        {
            _mockService = new Mock<ICartLogService>();
            _mockLogger = new Mock<ILogger<CartLogController>>();
            var claims = new List<Claim>
                {
                    new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2") // Example employee ID
                };
            _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
            _controller = new CartLogController(_mockLogger.Object, _mockService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = _user }
                }
            };
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
            var returnValue = Assert.IsAssignableFrom<IEnumerable<CartLogFetch>>(okResult.Value); // Ensure the value is of the expected type
            Assert.Single(returnValue);
        }


        public class NotFoundResponse
        {
            public string Message { get; set; }
        }
        [Fact]
        public async Task GetCartLogs_ReturnsNotFound_WhenNoLogsExist()
        {
            // Arrange: Simulate no cart logs by returning an empty list
            _mockService.Setup(service => service.GetCartLogsAsync("CLEAN", "HOME", 1))
                        .ReturnsAsync(new List<CartLogFetch>()); // Simulating no cart logs

            // Act
            var result = await _controller.GetCartLogs("CLEAN", "HOME", 1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var returnValue = notFoundResult.Value; // Directly get the value

            // Use dynamic or specific property name access
            Assert.Equal("No cart logs found.", ((dynamic)returnValue).message);
        }

        [Fact]
        public async Task GetCartLogs_ReturnsBadRequest_ForInvalidInput()
        {
            // Act
            var result = await _controller.GetCartLogs(null, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
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
            var createdResult = Assert.IsType<OkObjectResult>(result); // Expecting CreatedAtActionResult
            Assert.Equal(fetchCartLog.CartLogId, ((dynamic)createdResult.Value).cartLogs.CartLogId); // Check ID // Verifying that returned CartLogId matches the one upserted
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
            var cartLogId = 26;
            var employeeId = 2;
            _mockService.Setup(service => service.DeleteCartLogAsync(cartLogId, employeeId))
                        .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteCartLog(cartLogId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteCartLog_ReturnsNotFound_WhenCartLogDoesNotExist()
        {
            // Arrange
            var cartLogId = 26;
            var employeeId = 2;
            _mockService.Setup(service => service.DeleteCartLogAsync(cartLogId, employeeId))
                        .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCartLog(cartLogId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
