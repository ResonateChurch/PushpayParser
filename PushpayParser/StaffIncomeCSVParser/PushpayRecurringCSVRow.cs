using CsvHelper.Configuration;

namespace StaffIncomeCSVParser
{
    public class PushpayRecurringCSVRow
    {
        public decimal Amount { get; set; }
        public string? FundName { get; set; }
        public string? FundCode { get; set; }
        public Status ActiveStatus { get; set; }
        public Frequency GivingFrequency { get; set; }
    }

    public sealed class PushpayRecurringCSVMap : ClassMap<PushpayRecurringCSVRow>
    {
        public PushpayRecurringCSVMap()
        {
            Map(m => m.Amount).Name("Amount", "amount");
            Map(m => m.FundName).Name("FundName", "Fund Name");
            Map(m => m.FundCode).Name("FundCode", "Fund Code");
            Map(m => m.ActiveStatus).Name("Status", "status").TypeConverter<StatusConverter<Status>>();
            Map(m => m.GivingFrequency).Name("Frequency", "frequency").TypeConverter<FrequencyConverter<Frequency>>();
        }
    }
}