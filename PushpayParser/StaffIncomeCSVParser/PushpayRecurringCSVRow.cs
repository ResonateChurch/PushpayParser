using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace StaffIncomeCSVParser
{
    public class PushpayRecurringCSVRow
    {
        public decimal Amount { get; set; }
        public string? FundName { get; set; }
        public string? FundCode { get; set; }
        public Status ActiveStatus { get; set; }
    }

    public sealed class PushpayRecurringCSVMap : ClassMap<PushpayRecurringCSVRow>
    {
        public PushpayRecurringCSVMap()
        {
            Map(m => m.Amount).Name("Amount", "amount");
            Map(m => m.FundName).Name("FundName", "Fund Name");
            Map(m => m.FundCode).Name("FundCode", "Fund Code");
            Map(m => m.ActiveStatus).Name("Status", "status").TypeConverter<StatusConverter<Status>>();
        }
    }

    public class StatusConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text.ToLower().Contains("active"))
            {
                return Status.Active;
            }
            else if (text.ToLower().Contains("paused"))
            {
                return Status.Paused;
            }
            else if (text.ToLower().Contains("cancelled"))
            {
                return Status.Cancelled;
            }
            else
            {
                return Status.Unknown;
            }
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value switch
            {
                Status.Active => Status.Active.ToString(),
                Status.Paused => Status.Paused.ToString(),
                Status.Cancelled => Status.Cancelled.ToString(),
                Status.Unknown => Status.Unknown.ToString(),
                _ => Status.Unknown.ToString(),
            };
        }
    }

    public enum Status
    {
        Active = 1,
        Paused = 2,
        Cancelled = 3,
        Unknown = 4
    }
}