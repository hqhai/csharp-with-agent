// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class LocationQuestion
    {
    }

    public class LocationAnswer
    {
        public Guid? CountryId { get; set; }
        public string? CountryName { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
    }
}
