// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.OpenId
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class VnEduUserInfoResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("msg")]
        public string Msg { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public StudentData? Data { get; set; }
    }

    public class StudentData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("ma_hoc_sinh")]
        public string StudentCode { get; set; } = string.Empty;

        [JsonPropertyName("site_id")]
        public long SiteId { get; set; }

        [JsonPropertyName("hash_pwd")]
        public string HashPwd { get; set; } = string.Empty;

        [JsonPropertyName("key_login")]
        public string KeyLogin { get; set; } = string.Empty;

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;

        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("site_name")]
        public string SiteName { get; set; } = string.Empty;

        [JsonPropertyName("tinh_id")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("huyen_id")]
        public int DistrictId { get; set; }

        [JsonPropertyName("lop_hoc_id")]
        public string ClassId { get; set; } = string.Empty;

        [JsonPropertyName("lop_hoc")]
        public string ClassName { get; set; } = string.Empty;

        [JsonPropertyName("khoi")]
        public string SchoolGrade { get; set; } = string.Empty;

        [JsonPropertyName("class")]
        public List<ClassInfo>? Class { get; set; }
    }

    public class ClassInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
