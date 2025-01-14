// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.IRIS.Models
{
    using System.Collections.Generic;

    public class IRISSendSMSRequestModels
    {
        public IList<SMSRequestModel> SendingList { get; set; } = new List<SMSRequestModel>();
    }

    public class SMSRequestModel
    {
        public string? BrandName { get; set; }
        public string? IsCheckDuplicate { get; set; }
        public string? Priority { get; set; }
        public string? SmsId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Content { get; set; }
        public string? ContentType { get; set; }
    }
}
