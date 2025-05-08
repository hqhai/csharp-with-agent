namespace Fsel.Shared.Models.ShareModels
{
    public class AddCoinWhenCoursePurchasedCommandModel
    {
        public IList<Guid>? UserIds { get; set; }

        public double Coins { get; set; }

        public int Month { get; set; }

        public Guid? ObjectId { get; set; }
    }
}
