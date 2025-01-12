// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    public class LearningResultReportModel : LearningResultModel
    {
        public IList<OverallModuleReportSkillModel> OverallModuleReports { get; set; } = new List<OverallModuleReportSkillModel>();
    }
}
