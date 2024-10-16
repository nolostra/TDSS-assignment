using Xunit;
using Microsoft.AspNetCore.Mvc;
using LinenManagementSystem.Controllers;
using LinenManagementSystem.Models;
using LinenManagementSystem.Services;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinenManagementSystem.Tests.IntegrationTests
{
    public class CartLogControllerIntegrationTests
    {
        private readonly CartLogController _controller;
        private readonly Mock<ICartLogService> _mockService;

        public CartLogControllerIntegrationTests()
        {
            _mockService = new Mock<ICartLogService>();
            _controller = new CartLogController(_mockService.Object);
        }

        [Fact]
        public async Task GetCartLogs_ReturnsOkResult_WithCartLogs()
        {
            // Arrange
            var cartLogs = new List<CartLog>
            {
                new CartLog { CartLogId = 1, EmployeeId = 1 },
                new CartLog { CartLogId = 2, EmployeeId = 2 }
            };
            _mockService.Setup(service => service.GetCartLogsAsync()).ReturnsAsync(cartLogs);

            // Act
            var result = await _controller.GetCartLogs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<CartLog>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }

        [Fact]
        public async Task GetCartLogs_ReturnsNotFound_WhenNoLogsExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetCartLogsAsync()).ReturnsAsync((List<CartLog>)null);

            // Act
            var result = await _controller.GetCartLogs();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
