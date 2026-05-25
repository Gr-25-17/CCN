using NewsSite.Models.ViewModels;
using NewsSite.Models.APIs;


namespace NewsSite.Mapping
{
    public static class GoldPriceExstensions
    {


        public static GoldViewModel ToViewModel(this GoldPrice entity)
        {
       
            long ticks = long.Parse(entity.RowKey);
            DateTime date = new DateTime(DateTime.MaxValue.Ticks - ticks);

            return new GoldViewModel
            {
                DateLabel = date.ToString("MMM dd HH:mm"), 
                Price = entity.Close,
                Change = entity.PercentChange
            };
        }

   
        public static GoldPrice ToEntity(this GoldViewModel vm)
        {
            return new GoldPrice
            {
                RowKey = vm.DateLabel,
                Close = vm.Price,
                PercentChange = vm.Change,
                PartitionKey = "Gold"
            };
        }
    }
}
