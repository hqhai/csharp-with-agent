// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fsel.Identity.Domain.Models.EntityModels
{
    public class UserGroupModel : BaseModel
    {
        public string? GroupName { get; set; }
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }

        [JsonIgnore]
        public string? LocationIdStr { get; set; }

        [NotMapped]
        public List<string>? LocationIds
        {
            get
            {
                if (string.IsNullOrEmpty(LocationIdStr))
                    return new List<string>();

                try
                {
                    return JsonSerializer.Deserialize<List<string>>(LocationIdStr) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
                LocationIdStr = (value != null && value.Any())
                    ? JsonSerializer.Serialize(value)
                    : null;
            }
        }

        public IList<UserGroupMemberShipModel>? Members { get; set; }
    }
}
