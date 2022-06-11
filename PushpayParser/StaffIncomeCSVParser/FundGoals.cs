using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace StaffIncomeCSVParser
{
    public class FundGoalsCSVRow
    {
        public int Account { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public decimal Goal { get; set; }
    }

    public sealed class FundGoalsCSVRowCSVMap : ClassMap<FundGoalsCSVRow>
    {
        public FundGoalsCSVRowCSVMap()
        {
            Map(m => m.Account).Name("Account", "account", "Acct", "acct");
            Map(m => m.FirstName).Name("First Name", "Firstname", "First", "first");
            Map(m => m.LastName).Name("Last Name", "Lastname", "Last", "last");
            Map(m => m.Goal).Name("Goal", "Limit", "Goal/Limit", "Goal / Limit", $"Goal/{Environment.NewLine}Limit", $"Goal /\nLimit", $"\"Goal /\nLimit\"", $"\"Goal /{Environment.NewLine}Limit\"").TypeConverter<AmountConverter<decimal>>();
        }

        // Converts format x,xxx to xxxx as an int (ex: 3,000 to an int of 3000)
        public class AmountConverter<T> : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                return decimal.Parse(text, NumberStyles.AllowThousands);
            }

            public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                return value switch
                {
                    decimal i => i.ToString("n"),
                    _ => value.ToString() ?? "Error Converting Object to String",
                };
            }

        }
    }
}