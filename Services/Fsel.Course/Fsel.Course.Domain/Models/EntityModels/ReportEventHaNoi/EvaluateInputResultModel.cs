// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    public class EvaluateInputResultModel
    {
        public IList<TotalEvaluateInputResultModel>? TotalEvaluateInputResults { get; set; }

        public IList<TotalDetailEvaluateInputResultModel>? TotalDetailEvaluateInputResults { get; set; }

        public IList<PercentEvaluateInputResultModel>? PercentEvaluateInputResults { get; set; }

        public IList<LevelEvaluateInputResultModel>? LevelEvaluateInputResults { get; set; }
    }
}
