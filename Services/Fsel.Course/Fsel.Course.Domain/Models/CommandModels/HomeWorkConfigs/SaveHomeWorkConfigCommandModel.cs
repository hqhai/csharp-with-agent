// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.HomeWorkConfigs
{
    public class SaveHomeWorkConfigCommandModel
    {
        public Guid? Id { get; set; }
        public int NumberRetry { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid HomeWorkId { get; set; }
        public Guid CurriculumId { get; set; }
    }
}
