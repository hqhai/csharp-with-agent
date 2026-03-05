// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class ExportPdfByTemplateCommandModel
    {
        public string Top { get; set; } = "1cm";
        public string Bottom { get; set; } = "1cm";
        public string Right { get; set; } = "1cm";
        public string Left { get; set; } = "1cm";
        public bool Landscape { get; set; }
        public object? Params { get; set; }
        public EnumSenderTemplate Template { get; set; }
    }
}
