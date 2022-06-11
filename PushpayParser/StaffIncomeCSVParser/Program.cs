using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StaffIncomeCSVParser
{
    internal class Program
    {
        static void Main()
        {
            Dictionary<int, decimal> totalAmounts = new();
            List<PushpayRecurringCSVRow> records = LoadRecurringPushpayPaymentsCSV(out string csvPath);
            List<FundGoalsCSVRow> goalValues = LoadGoalsAndFundsCSV();
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
                string goalPercentageString;
                if (goalValues.Where(i => i.Account == item.Key).Sum(i => i.Goal) > 0)
                {
                    goalPercentageString = (decimal.Round((item.Value / goalValues.Where(i => i.Account == item.Key).Sum(i => i.Goal) * 100), 2).ToString() + "%");
                }
                else
                {
                    goalPercentageString = ($"No Goal Value Found or goal amount is 0");
                }
                results.Add(new ResultingCSVRow { FundCode = item.Key, Name = idNamePair[item.Key], TotalMonthlyRecurringGifts = item.Value, TotalMonthlyGoal = goalValues.Where(i => i.Account == item.Key).Sum(i => i.Goal), GoalPercentage = goalPercentageString });
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

        private static List<FundGoalsCSVRow> LoadGoalsAndFundsCSV()
        {
            Console.WriteLine($"Please enter in the full path to the csv file of the Goals/Limits that you want to parse (ex: \"C:\\Users\\guest\\Downloads\\Goals.csv\"");
            string? goalsCSVPath = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(goalsCSVPath) || goalsCSVPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                Console.WriteLine("Please enter a valid path to the csv file");
                goalsCSVPath = Console.ReadLine();
            }
            if (goalsCSVPath[0] == '"' && goalsCSVPath[goalsCSVPath.Length - 1] == '"')
            {
                goalsCSVPath = goalsCSVPath.Replace("\"", string.Empty);
            }

            List<FundGoalsCSVRow> goalValues;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Replace Whitespace
                PrepareHeaderForMatch = args => Regex.Replace(args.Header, @"\s", string.Empty),
            };
            using (var reader = new StreamReader(goalsCSVPath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<FundGoalsCSVRowCSVMap>();
                IEnumerable<FundGoalsCSVRow> csvRecords = csv.GetRecords<FundGoalsCSVRow>();
                goalValues = csvRecords.ToList();
            }

            return goalValues;
        }

        private static List<PushpayRecurringCSVRow> LoadRecurringPushpayPaymentsCSV(out string csvPath)
        {
            Console.WriteLine($"Hello,{Environment.NewLine}Please enter in the full path to the csv file you want to parse (ex: \"C:\\Users\\guest\\Downloads\\RecurringPayments.csv\"");
            string? tempPath = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(tempPath) || tempPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                Console.WriteLine("Please enter a valid path to the csv file");
                tempPath = Console.ReadLine();
            }
            if (tempPath[0] == '"' && tempPath[tempPath.Length - 1] == '"')
            {
                tempPath = tempPath.Replace("\"", string.Empty);
            }
            csvPath = tempPath;
            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<PushpayRecurringCSVMap>();
            IEnumerable<PushpayRecurringCSVRow> csvRecords = csv.GetRecords<PushpayRecurringCSVRow>();
            return csvRecords.ToList();
        }
    }
}