﻿using CsvHelper.Configuration;

namespace StaffIncomeCSVParser
{
    public class ResultingCSVRow
    {
        public int FundCode { get; set; }
        public string? Name { get; set; }
        public decimal TotalMonthlyRecurringGifts { get; set; }
        public decimal TotalMonthlyGoal { get; set; }
        public string? GoalPercentage { get; set; }
    }

    public sealed class ResultingCSVRowCSVMap : ClassMap<ResultingCSVRow>
    {
        public ResultingCSVRowCSVMap()
        {
            Map(m => m.FundCode).Name("Fund Code");
            Map(m => m.Name).Name("Name");
            Map(m => m.TotalMonthlyRecurringGifts).Name("Total Monthly Recurring Gifts");
            Map(m => m.TotalMonthlyGoal).Name("Total Monthly Goal");
            Map(m => m.GoalPercentage).Name("Percentage Of Goal Raised");
        }
    }
}