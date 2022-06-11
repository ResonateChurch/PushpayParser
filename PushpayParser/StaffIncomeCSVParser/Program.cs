using CsvHelper;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StaffIncomeCSVParser
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine($"Hello,{Environment.NewLine}Please enter in the full path to the csv file you want to parse (ex: \"C:\\Users\\guest\\Downloads\\RecurringPayments.csv\"");
            string? csvPath;
            csvPath = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(csvPath) || csvPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                Console.WriteLine("Please enter a valid path to the csv file");
                csvPath = Console.ReadLine();
            }

            List<PushpayRecurringCSVRow> records;
            Dictionary<int, decimal> totalAmounts = new();
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<PushpayRecurringCSVMap>();
                IEnumerable<PushpayRecurringCSVRow> csvRecords = csv.GetRecords<PushpayRecurringCSVRow>();
                records = csvRecords.ToList();
            }
            // Add up all amounts and associate with a FundCode
            records.ForEach(i =>
            {
                if (i.ActiveStatus is Status.Active)
                {
                    if (int.TryParse(i.FundCode, out _))
                    {
                        int fundCode = int.Parse(i.FundCode);
                        if (totalAmounts.ContainsKey(fundCode))
                        {
                            if (!totalAmounts.TryGetValue(fundCode, out decimal amount))
                            {
                                throw new InvalidOperationException($"Fund {fundCode} cannot be found in dictionary");
                            }
                            totalAmounts[fundCode] = amount + (i.Amount * i.GivingFrequency.NumberOfOccurancesAMonth());
                        }
                        else
                        {
                            totalAmounts.Add(int.Parse(i.FundCode), (i.Amount * i.GivingFrequency.NumberOfOccurancesAMonth()));
                        }
                    }
                }
            });
            // Parse Names from FundName and Associate with FundCode
            Dictionary<int, string> idNamePair = new();
            records.ForEach(i =>
            {
                if (i.ActiveStatus is Status.Active)
                {
                    if (!string.IsNullOrEmpty(i.FundName))
                    {
                        if (i.FundName.Any(char.IsDigit))
                        {
                            string fundCodeFromName = Regex.Match(i.FundName, @"\d+").Value;
                            if (int.TryParse(fundCodeFromName, out int fundCodeFromNameInt))
                            {
                                if (!idNamePair.ContainsKey(fundCodeFromNameInt))
                                {
                                    string fundName = i.FundName.Replace(fundCodeFromName, string.Empty).Trim();
                                    idNamePair.Add(fundCodeFromNameInt, fundName);
                                }
                            }
                        }
                    }
                }
            });
            string outputPath = csvPath.Replace(".csv", "-completed.csv");
            List<ResultingCSVRow> results = new();
            foreach (KeyValuePair<int, decimal> item in totalAmounts)
            {
                results.Add(new ResultingCSVRow { FundCode = item.Key, Name = idNamePair[item.Key], TotalMonthlyRecurringGifts = item.Value });
            }
            results.Sort(comparison: (a, b) => a.FundCode.CompareTo(b.FundCode));
            using (var writer = new StreamWriter(outputPath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ResultingCSVRowCSVMap>();
                csv.WriteRecords(results);
            }
            Console.WriteLine($"Parsed CSV has been saved to {outputPath}");
        }
    }
}