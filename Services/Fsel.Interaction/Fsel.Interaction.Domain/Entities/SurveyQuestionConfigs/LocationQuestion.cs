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
        public Guid? CityId { get; set; }
        public string? CityName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
    }
}
