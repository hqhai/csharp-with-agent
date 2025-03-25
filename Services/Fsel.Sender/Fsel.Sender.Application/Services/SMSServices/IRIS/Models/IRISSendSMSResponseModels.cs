// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Application.Services.SMSServices.IRIS.Models
{
    public class IRISSendSMSResponseModels
    {
        public IList<SMSResponseModel>? ResultList { get; set; }
    }

    public class SMSResponseModel
    {
        public string? Code { get; set; }
        public string? Telco { get; set; }
        public string? SmsId { get; set; }
    }
}
