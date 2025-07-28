// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class SchoolSummaryModel
    {
        public string? DistrictName { get; set; }
        public string? SchoolName { get; set; }
        public int? ImportStudent { get; set; }
        public int? RegisterStudent { get; set; }
        public int? TotalStudent { get; set; }
        public int? ActiveStudent { get; set; }
        public decimal? PercentageActiveStudent { get; set; }
        public int? StudentDonePT { get; set; }
        public int? StudentProcessPT { get; set; }
        public decimal? PercentageStudentDoneTotal { get; set; }
        public decimal? PercentageStudentDoneActive { get; set; }
        public int? AmountArchiveA1 { get; set; }
        public decimal? PercentageAmountArchiveA1 { get; set; }
        public int? AmountArchiveA2 { get; set; }
        public decimal? PercentageAmountArchiveA2 { get; set; }
        public int? AmountArchiveB1 { get; set; }
        public decimal? PercentageAmountArchiveB1 { get; set; }
        public int? AmountArchiveB1Plus { get; set; }
        public decimal? PercentageAmountArchiveB1Plus { get; set; }
        public int? AmountArchiveB2 { get; set; }
        public decimal? PercentageAmountArchiveB2 { get; set; }
        public int? AmountArchiveC1 { get; set; }
        public decimal? PercentageAmountArchiveC1 { get; set; }
        public int? TotalItem { get; set; }
        public double? TotalStudentDefault { get; set; }
        public int? StudentNewPT { get; set; }
        public decimal? PercentActiveDefault { get; set; }
        public decimal? PercentDonePtDefault { get; set; }
        public int? TotalStudentJoin { get; set; }
        public decimal? PercentTotalStudentJoin { get; set; }
        public decimal? PercentStudentJoinDefault { get; set; }
    }
}
