using Fsel.Common.Helpers;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Shared.Models.ShareModels
{
    public class MenuModel : BaseModel
    {
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
    }
}
