using Fsel.Shared.Enums;

namespace Fsel.Shared.Models.ShareModels
{
    public class CreateHistoryDeductCoinOfStudentCommandModel
    {
        public Guid UserId { get; set; }
        public long InitialToken { get; set; }
        public long VolatileToken { get; set; }
        public long RemainToken { get; set; }
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission? Mission { get; set; }
        public Guid? ObjectId { get; set; }
        public object? Config { get; set; }
        public IList<TokenHistoryTranslationModel>? Translations { get; set; }
    }
}
