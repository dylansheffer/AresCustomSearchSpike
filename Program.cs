using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlRegexSpike
{
    class Program
    {
        static void Main(string[] args)
        {
            string sqlText = @"SELECT Items.*, Courses.*,  lp.Description as 'Loan Period Description', STUFF ((SELECT ', ' + ItemTags.ItemTag FROM ItemTags WHERE ItemTags.Tagger = 'INSTRUCTOR' AND ItemTags.ItemID = Items.ItemId ORDER BY ItemTags.ItemTag FOR XML PATH('')), 1, 2, '') AS InstructorTags , ItemFlags.*
FROM Items
JOIN Courses ON (Items.CourseID = Courses.CourseId)LEFT JOIN LoanPeriods lp ON (Items.LoanPeriod = lp.LoanPeriodID)
LEFT JOIN ItemFlags ON (Items.ItemId = ItemFlags.ItemId)
WHERE ((([ItemFlags].[Flag] = N'Invalid Active Dates') And ([ItemFlags].[Flag] <> N'Item In Processing')))";

            string complexSqlText = @"SELECT Items.*, Courses.*,  lp.Description as 'Loan Period Description', STUFF ((SELECT ', ' + ItemTags.ItemTag FROM ItemTags WHERE ItemTags.Tagger = 'INSTRUCTOR' AND ItemTags.ItemID = Items.ItemId ORDER BY ItemTags.ItemTag FOR XML PATH('')), 1, 2, '') AS InstructorTags , ItemFlags.*
FROM Items
JOIN Courses ON(Items.CourseID = Courses.CourseId)LEFT JOIN LoanPeriods lp ON(Items.LoanPeriod = lp.LoanPeriodID)
LEFT JOIN ItemFlags ON(Items.ItemId = ItemFlags.ItemId)
WHERE((([ItemFlags].[Flag] > N'Test Flag 1') And([ItemFlags].[Flag] >= N'Test Flag 2') And([ItemFlags].[Flag] < N'Test Flag 1') And([ItemFlags].[Flag] <= null) And(([ItemFlags].[Flag] >= N'Test Flag 1') And([ItemFlags].[Flag] <= N'Test Flag 2')) And not((([ItemFlags].[Flag] >= N'Test Flag 1') And([ItemFlags].[Flag] <= N'Hard Copy Item Cancelled by Instructor'))) And(isnull(CharIndex(N'Test Flag 1', [ItemFlags].[Flag]), 0) > 0) And not((isnull(CharIndex(N'Hard Copy Item Cancelled by Instructor', [ItemFlags].[Flag]), 0) > 0)) And(Right([ItemFlags].[Flag], DATALENGTH(Cast(N'Queue Test 1' as nvarchar(max))) / 2) = (N'Queue Test 1')) And([ItemFlags].[Flag] like N'Invalid Active Dates') And not(([ItemFlags].[Flag] like N'Test Flag 2')) And[ItemFlags].[Flag] in (N'Queue Test 1',N'Invalid Active Dates',N'Test Flag 2') And not([ItemFlags].[Flag] in (N'Queue Test 1', N'Test Flag 2', N'Test Flag 3')) And(([ItemFlags].[Flag]) is null or len([ItemFlags].[Flag]) = 0) And not((([ItemFlags].[Flag]) is null or len([ItemFlags].[Flag]) = 0))) And ([ItemFlags].[Flag] like N'Invalid Active Dates%'))";

            (new SqlRegexTester()).Test(complexSqlText);
        }
    }


    public class SqlRegexTester
    {
        public void Test(string sqlText)
        {
            // '(''|[^'])*'
            string itemFlagEqualityRegex = @"\(\[ItemFlags\]\.\[(?<FieldName>[^\]]*)\] (?<Operator>([<>=]*)) N'(?<FieldValue>(''|[^'])*)'\)";
            string itemFlagBetweenRegex = @"\(\(\[ItemFlags\]\.\[(?<FieldName>[^\]]*)\] >= N'(?<FieldValue1>(''|[^'])*)'\) And\(\[ItemFlags\]\.\[\k<FieldName>\] \<= N'(?<FieldValue2>(''|[^'])*)'\)\)";
            string itemFlagContainsRegex = @"\(isnull\(CharIndex\(N'(?<FieldValue>(''|[^'])*)', \[ItemFlags\]\.\[(?<FieldName>[^\]]*)\]\), 0\) > 0\)";
            string itemFlagLikeRegex = @"\(\[ItemFlags\]\.\[(?<FieldName>[^\]]*)\] like N'(?<FieldValue>(''|[^'])*)'\)";
            string itemFlagEndsWithRegex = @"\(Right\(\[ItemFlags]\.\[(?<FieldName>[^\]]*)\], DATALENGTH\(Cast\(N'(?<FieldValue>(''|[^'])*)' as nvarchar\(max\)\)\) \/ 2\) = \(N'\k<FieldValue>'\)\)";
            string itemFlagIsAnyOfRegex = @"\[ItemFlags]\.\[(?<FieldName>[^\]]*)\] in \((N'(?<Fields>''|[^']*)'[, ]{0,2})+\)";
            string itemFlagBlankRegex = @"\(\[ItemFlags]\.\[(?<FieldName>[^\]]*)\]\) is null or len\(\[ItemFlags].\[\k<FieldName>\]\) = 0\)";

            string itemFlagEqualityRegex2 = @"\(\[ItemFlags\]\.\[(?<FieldName>[^\]]*)\] (?<Operator>([<>=]*)) N'(?<FieldValue>(''|[^'])*)'\)";
            string itemFlagBetweenRegex2 = @"(?<WhereClause>\(\(\[ItemFlags\]\.\[(?<FieldName>[^\]]*)\] >= N'(?<FieldValue1>(''|[^'])*)'\) And\(\[ItemFlags\]\.\[\k<FieldName>\] \<= N'(?<FieldValue2>(''|[^'])*)'\)\))";
            string itemFlagContainsRegex2 = @"(?<WhereClause>\(isnull\(CharIndex\(N'(?<FieldValue>(''|[^'])*)', \[ItemFlags\]\.\[(?<FieldName>[^\]]*)\]\), 0\) > 0\))";
            string itemFlagLikeRegex2 = @"(?<WhereClause>\(\[ItemFlags\]\.\[(?<FieldName>[^\]]*)\] like N'(?<FieldValue>(''|[^'])*)'\))";
            string itemFlagEndsWithRegex2 = @"(?<WhereClause>\(Right\(\[ItemFlags]\.\[(?<FieldName>[^\]]*)\], DATALENGTH\(Cast\(N'(?<FieldValue>(''|[^'])*)' as nvarchar\(max\)\)\) \/ 2\) = \(N'\k<FieldValue>'\)\))";
            string itemFlagIsAnyOfRegex2 = @"(?<WhereClause>\[ItemFlags]\.\[(?<FieldName>[^\]]*)\] in \((N'(?<Fields>''|[^']*)'[, ]{0,2})+\))";
            string itemFlagBlankRegex2 = @"(?<WhereClause>\(\[ItemFlags]\.\[(?<FieldName>[^\]]*)\]\) is null or len\(\[ItemFlags].\[\k<FieldName>\]\) = 0\))";


            var result = Regex.Match(sqlText, itemFlagEqualityRegex);

            Console.WriteLine();

            string replaceText = Regex.Replace(sqlText, itemFlagEqualityRegex, ItemFlagEqualityMatchEvaluator);
            replaceText = Regex.Replace(replaceText, itemFlagBetweenRegex, ItemFlagBetweenMatchEvaluator);
            replaceText = Regex.Replace(replaceText, itemFlagContainsRegex, ItemFlagContainsMatchEvaluator);
            replaceText = Regex.Replace(replaceText, itemFlagLikeRegex, ItemFlagLikeMatchEvaluator);
            replaceText = Regex.Replace(replaceText, itemFlagEndsWithRegex, ItemFlagEndsWithMatchEvaluator);
            replaceText = Regex.Replace(replaceText, itemFlagIsAnyOfRegex, ItemFlagIsAnyOfMatchEvaluator);
            replaceText = Regex.Replace(replaceText, itemFlagBlankRegex, ItemFlagBlankMatchEvaluator);

            string replaceText2 = Regex.Replace(sqlText, itemFlagEqualityRegex2, ItemFlagEqualityMatchEvaluator);
            replaceText2 = Regex.Replace(replaceText2, itemFlagBetweenRegex2, ItemFlagGeneralMatchEvaluator);
            replaceText2 = Regex.Replace(replaceText2, itemFlagContainsRegex2, ItemFlagGeneralMatchEvaluator);
            replaceText2 = Regex.Replace(replaceText2, itemFlagLikeRegex2, ItemFlagGeneralMatchEvaluator);
            replaceText2 = Regex.Replace(replaceText2, itemFlagEndsWithRegex2, ItemFlagGeneralMatchEvaluator);
            replaceText2 = Regex.Replace(replaceText2, itemFlagIsAnyOfRegex2, ItemFlagGeneralMatchEvaluator);
            replaceText2 = Regex.Replace(replaceText2, itemFlagBlankRegex2, ItemFlagGeneralMatchEvaluator);



            var result2 = string.Equals(replaceText, replaceText2, StringComparison.OrdinalIgnoreCase);

        }

        public string ItemFlagGeneralMatchEvaluator(Match match)
        {
            return $"([Items].[ItemID] IN (SELECT [ItemID] FROM [ItemFlags] WHERE {match.Groups["WhereClause"].Value}";
        }

        public string ItemFlagEqualityMatchEvaluator(Match match)
        {
            string op = match.Groups["Operator"].Value;
            string scope = null;
            if (op == "<>")
            {
                scope = "NOT IN";
                op = "=";
            }
            else
            {
                scope = "IN";
            }
            return $"([Items].[ItemId] {scope} (SELECT [ItemId] FROM [ItemFlags] WHERE [{match.Groups["FieldName"].Value}] {op} N'{match.Groups["FieldValue"].Value}'))";
        }

        public string ItemFlagBetweenMatchEvaluator(Match match)
        {
            string FieldName = match.Groups["FieldName"].Value;
            string FieldValue1 = match.Groups["FieldValue1"].Value;
            string FieldValue2 = match.Groups["FieldValue2"].Value;
            return $"([Items].[ItemID] IN (SELECT [ItemID] FROM [ItemFlags] WHERE [ItemFlags].[{FieldName}] >= N'{FieldValue1}' AND [ItemFlags].[{FieldName}] <= N'{FieldValue2}'))";
        }

        public string ItemFlagContainsMatchEvaluator(Match match)
        {
            string FieldName = match.Groups["FieldName"].Value;
            string FieldValue = match.Groups["FieldValue"].Value;

            return $"([Items].[ItemID] IN (SELECT [ItemID] FROM [ItemFlags] WHERE ISNULL(CHARINDEX(N'{FieldValue}', [ItemFlags].[{FieldName}]), 0) > 0))";
        }

        public string ItemFlagLikeMatchEvaluator(Match match)
        {
            string FieldName = match.Groups["FieldName"].Value;
            string FieldValue = match.Groups["FieldValue"].Value;

            return $"([Items].[ItemID] IN (SELECT [ItemID] FROM [ItemFlags] WHERE [ItemFlags].[{FieldName}] LIKE N'{FieldValue}'))";
        }

        public string ItemFlagEndsWithMatchEvaluator(Match match)
        {
            string FieldName = match.Groups["FieldName"].Value;
            string FieldValue = match.Groups["FieldValue"].Value;

            return $"([Items].[ItemID] IN (SELECT [ItemID] FROM [ItemFlags] WHERE (Right([ItemFlags].[{FieldName}], DATALENGTH(Cast(N'{FieldValue}' as nvarchar(max))) / 2) = (N'{FieldValue}'))))";
        }

        public string ItemFlagIsAnyOfMatchEvaluator(Match match)
        {
            string fieldName = match.Groups["FieldName"].Value;
            StringBuilder fieldBuilder = new StringBuilder();
            var captures = match.Groups["Fields"].Captures;

            for (int i = 0; i < captures.Count; i++)
            {
                fieldBuilder.Append("N'");
                fieldBuilder.Append(captures[i]);
                fieldBuilder.Append("'");
                if (i < captures.Count - 1)
                {
                    fieldBuilder.Append(", ");
                }

            }

            return $"([Items].[ItemID] IN (SELECT [ItemID] FROM [ItemFlags] WHERE [ItemFlags].[{fieldName}] IN ({fieldBuilder.ToString()})))";
        }

        public string ItemFlagBlankMatchEvaluator(Match match)
        {
            string fieldName = match.Groups["FieldName"].Value;
            string fieldValue = match.Groups["FieldValue"].Value;

            return $"([Items].[ItemID] IN (SELECT [ItemID] FROM [ItemFlags] WHERE ([ItemFlags].[{fieldName}]) is null or len([ItemFlags].[{fieldName}]) = 0)))";
        }
    }

}