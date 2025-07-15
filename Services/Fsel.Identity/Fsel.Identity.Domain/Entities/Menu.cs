using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Identity.Domain.Entities
{
    public class Menu : Entity
    {
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public EnumMenuCategory Category { get; set; }
        public int Index { get; set; }
        public string? ConfigStr { get; set; }

        [NotMapped]
        public MenuConfig? Config
        {
            get { return ConvertHelper.Deserialize<MenuConfig>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }

        public PermissionGroup? PermissionGroup { get; set; }
    }
}
