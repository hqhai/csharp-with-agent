namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class DeductCoinOfStudentCommandModel
    {
        public Guid? UserId { get; set; }
        public long NumberOfCoinsDeducted { get; set; }
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission? Mission { get; set; }
        public Guid? ObjectId { get; set; }
        public object? Config { get; set; }

        public IList<TokenHistoryTranslationModel>? Translations { get; set; }
    }
}
