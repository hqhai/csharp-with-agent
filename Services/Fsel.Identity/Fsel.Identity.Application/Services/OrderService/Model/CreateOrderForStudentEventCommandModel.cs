// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class CreateOrderForStudentEventCommandModel
    {
        public DateTime ExpiredDate { get; set; }
        public CreateOrderForStudentsEventCommandModel? Student { get; set; }
    }
}
