using System;
using System.Threading.Tasks;
using LinenManagementSystem.Repositories;
using LinenManagementSystem.Models;
using LinenManagementSystem.DTOs;

namespace LinenManagementSystem.Services
{
    public class CartLogService
    {
        private readonly ICartLogRepository _repository;

        public CartLogService(ICartLogRepository repository)
        {
            _repository = repository;
        }

        public async Task<DTOs.CartLog> GetCartLogByIdAsync(int id)
        {
            var cartLog = await _repository.GetCartLogByIdAsync(id);
            if (cartLog == null) throw new Exception("CartLog not found");

            return new DTOs.CartLog
            {
                CartLogId = cartLog.CartLogId,
                ReceiptNumber = cartLog.ReceiptNumber,
                ReportedWeight = cartLog.ReportedWeight,
                ActualWeight = cartLog.ActualWeight,
                Comments = cartLog.Comments,
                DateWeighed = cartLog.DateWeighed,
                Cart = new DTOs.CartDto
                {
                    CartId = cartLog.Cart.CartId,
                    Name = cartLog.Cart.Name,
                    Weight = cartLog.Cart.Weight,
                    Type = cartLog.Cart.Type
                },
                Location = new DTOs.LocationDto
                {
                    LocationId = cartLog.Location.LocationId,
                    Name = cartLog.Location.Name,
                    Type = cartLog.Location.Type
                },
                Employee = new DTOs.EmployeeDto
                {
                    EmployeeId = cartLog.Employee.EmployeeId,
                    Name = cartLog.Employee.Name
                },
                Linens = cartLog.Linens.Select(l => new DTOs.LinenDto
                {
                    LinenId = l.LinenId,
                    Name = l.Name,
                    Count = l.Count
                }).ToList()
            };
        }


        public async Task CreateOrUpdateCartLogAsync(DTOs.CartLog cartLogDto)
        {
            // Map DTO to entity
            var cartLog = new Models.CartLog
            {
                CartLogId = cartLogDto.CartLogId,
                ReceiptNumber = cartLogDto.ReceiptNumber,
                ReportedWeight = cartLogDto.ReportedWeight,
                ActualWeight = cartLogDto.ActualWeight,
                Comments = cartLogDto.Comments,
                DateWeighed = cartLogDto.DateWeighed,
                Cart = new Models.Cart
                {
                    CartId = cartLogDto.Cart.CartId,
                    Name = cartLogDto.Cart.Name,
                    Weight = cartLogDto.Cart.Weight,
                    Type = cartLogDto.Cart.Type
                },
                Location = new Models.Location
                {
                    LocationId = cartLogDto.Location.LocationId,
                    Name = cartLogDto.Location.Name,
                    Type = cartLogDto.Location.Type
                },
                Employee = new Models.Employee
                {
                    EmployeeId = cartLogDto.Employee.EmployeeId,
                    Name = cartLogDto.Employee.Name
                },
                Linens = cartLogDto.Linens.Select(l => new Models.Linen
                {
                    LinenId = l.LinenId,
                    Name = l.Name,
                    Count = l.Count
                }).ToList()
            };

            await _repository.UpsertCartLogAsync(cartLog);
        }

    }
}