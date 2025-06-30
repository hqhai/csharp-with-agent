// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.IntegrationModel
{
    public class ClientsIntegrationModel : IntegrationModel
    {
        public DateTime? StartCourse { get; set; }

        public DateTime? EndCourse { get; set; }

        public IList<OrderIntegrationModel> OrderIntegration { get; set; } = new List<OrderIntegrationModel>();
    }
}
