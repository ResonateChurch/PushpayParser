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
        public bool ActiveStatus { get; set; }
    }

    public sealed class PushpayRecurringCSVMap : ClassMap<PushpayRecurringCSVRow>
    {
        public PushpayRecurringCSVMap()
        {
            Map(m => m.Amount).Name("Amount", "amount");
            Map(m => m.FundName).Name("FundName", "Fund Name");
            Map(m => m.FundCode).Name("FundCode", "Fund Code");
            Map(m => m.ActiveStatus).Name("Status", "status").TypeConverter<StatusConverter<bool>>();
        }
    }

    public class StatusConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text.ToLower().Contains("active"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value switch
            {
                true => "Active",
                false => "Cancelled",
                _ => "Status Unknown",
            };
        }
    }
}