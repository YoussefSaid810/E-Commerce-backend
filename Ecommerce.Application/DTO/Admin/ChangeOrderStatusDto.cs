using System;

namespace Ecommerce.Application.DTO.Admin
{
    public class ChangeOrderStatusDto
    {
        public Guid OrderId { get; set; }
        public string NewStatus { get; set; } = null!;
    }
}
