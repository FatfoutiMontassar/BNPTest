using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpBNPTest
{
    public class Trade
    {
        public int CorrelationId { get; set; }
        public int NumberOfTrades { get; set; }
        public int Limit { get; set; }
        public int Value { get; set; }
        public int TradeID { get; set; }

        public Trade(DataRow row)
        {
            CorrelationId = ParseToInt(row["CorrelationId"]);
            NumberOfTrades = ParseToInt(row["NumberOfTrades"]);
            Limit = ParseToInt(row["Limit"]);
            Value = ParseToInt(row["Trade_Text"]);
            TradeID = ParseToInt(row["TradeID"]);
        }

        public int ParseToInt(Object input)
        {
            try
            {
                return int.Parse(input.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e); // save log
                throw;
            }
        }
    }
    public class TradeTable
    {
        public List<Trade> Trades;

        public TradeTable(DataRowCollection rows)
        {
            Trades = new List<Trade>();
            foreach (DataRow row in rows)
            {
                Trades.Add(new Trade(row));
            }
        }

        public TradeResult ComputeTradeResult()
        {
            var groupedTrades = Trades.GroupBy(t => t.CorrelationId)
                .ToDictionary(t => t.Key, t => t.ToList());

            var tradeResult = new TradeResult();

            foreach (var key in groupedTrades.Keys)
            {
                int numberOfTrades = groupedTrades[key].First().NumberOfTrades;
                int tradeLimit = groupedTrades[key].First().Limit;

                tradeResult.AddRow(key,
                    numberOfTrades,
                    numberOfTrades > groupedTrades[key].Count ? TradeResult.TradeState.Pending
                        : (groupedTrades[key].Select(t => t.Value).Sum() > tradeLimit ? TradeResult.TradeState.Rejected
                            : TradeResult.TradeState.Accepted));
            }

            tradeResult.SortRows();
            return tradeResult;
        }
    }
    public class TradeResult
    {
        public List<Tuple<int, int, TradeState>> TradeValues { get; set; }
        public enum TradeState
        {
            Pending,
            Rejected,
            Accepted
        }

        public TradeResult()
        {
            TradeValues = new List<Tuple<int,int, TradeState>>();
        }

        public void AddRow(int correlationID, int numberOfTrades, TradeState state)
        {
            TradeValues.Add(new Tuple<int, int, TradeState>(correlationID, numberOfTrades, state));
        }

        public void SortRows()
        {
            TradeValues.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        }
        public void WriteAsCsv(string filePath)
        {
            var pathElements = filePath.Split(@"\");
            pathElements[pathElements.Length - 1] = pathElements.Last().Replace("input", "results");
            pathElements[pathElements.Length - 1] = pathElements.Last().Replace("xml", "csv");
            var outputPath = string.Join(@"\", pathElements);

            StringBuilder outputContent = new StringBuilder();
            outputContent.AppendLine("CorrelationID,NumberOfTrades,State");
            for (int cnt = 0; cnt < TradeValues.Count; cnt++)
                outputContent.AppendLine(TradeValues[cnt].Item1.ToString() + "," + TradeValues[cnt].Item2.ToString() + "," + TradeValues[cnt].Item3.ToString());

            Console.WriteLine(outputContent.ToString());
            File.WriteAllText(outputPath, outputContent.ToString());
        }
    }
}
