using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace StaffIncomeCSVParser
{
    public enum Frequency
    {
        Weekly = 1,
        EveryTwoWeeks = 2,
        Monthly = 3,
        FirstAnd15thOfMonth = 4,
        Unknown = 5
    }

    public class FrequencyConverter<T> : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text.ToLower().Contains("every week"))
            {
                return Frequency.Weekly;
            }
            else if (text.ToLower().Contains("every 2 weeks") || text.ToLower().Contains("every two weeks"))
            {
                return Frequency.EveryTwoWeeks;
            }
            else if (text.ToLower().Contains("every month"))
            {
                return Frequency.Monthly;
            }
            else if (text.ToLower().Contains("1st & 15th monthly") || text.ToLower().Contains("1st and 15th monthly"))
            {
                return Frequency.FirstAnd15thOfMonth;
            }
            else
            {
                return Frequency.Unknown;
            }
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value switch
            {
                Frequency.Weekly => Frequency.Weekly.ToString(),
                Frequency.EveryTwoWeeks => Frequency.EveryTwoWeeks.ToString(),
                Frequency.FirstAnd15thOfMonth => Frequency.EveryTwoWeeks.ToString(),
                Frequency.Monthly => Frequency.Monthly.ToString(),
                Frequency.Unknown => Frequency.Unknown.ToString(),
                _ => Frequency.Unknown.ToString()
            };
        }

    }

    static class DurationExtensions
    {
        public static int NumberOfOccurancesAMonth(this Frequency frequency)
        {
            return frequency switch
            {
                Frequency.Weekly => (int)Math.Floor(4.34523809),
                Frequency.EveryTwoWeeks => (int)Math.Floor(4.34523809 / 2),
                Frequency.Monthly => 1,
                Frequency.FirstAnd15thOfMonth => 2,
                _ => 1,
            };
        }
    }
}