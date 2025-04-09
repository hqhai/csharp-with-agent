// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fsel.Common.Enums.ErrorCodes;

namespace Fsel.Identity.Domain.Models.CommandModels.UserGroup
{
    public class CreateUserGroupCommandModel
    {
        [Required]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string GroupName { get; set; } = string.Empty;

        public int? DisplayOrder { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

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
    }
} 
