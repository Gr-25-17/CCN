using System.Globalization;
using NewsSite.Models.APIs;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class GoldPriceExstensions
    {
        private const string TimeBucketFormat = "yyyyMMddHH";

        public static GoldViewModel ToViewModel(this GoldPrice entity)
        {
            var date = ParseDateUtc(entity.RowKey);

            return new GoldViewModel
            {
                DateLabel = date == DateTime.MinValue ? "Unknown" : date.ToString("MMM dd HH:mm", CultureInfo.InvariantCulture),
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

        private static DateTime ParseDateUtc(string? rowKey)
        {
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                return DateTime.MinValue;
            }

            if (DateTime.TryParseExact(
                    rowKey,
                    TimeBucketFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var bucketDate))
            {
                return bucketDate;
            }

            return DateTime.MinValue;
        }
    }
}
