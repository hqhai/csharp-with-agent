namespace Fsel.Core.Base.BaseModels
{
    public class PagingItemsModel<T>
    {
        public IEnumerable<T>? Items { get; set; }

        public PagingInfoModel? PagingInfo { get; set; }
    }
}