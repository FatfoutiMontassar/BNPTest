using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;

namespace CSharpBNPTest
{
    public enum CheckRowsVerdict { AllGood = 0, RowsAreNotOfTheSameSize = 1, ValuesTypesAreNotAsExpected = 2, SomeValuesAreEmpty = 3};
    public enum CheckColumnsVerdict { AllGood = 0, SomeColumnsAreMissing = 1, ColumnsValuesAreNotAsExpected = 2};
    public static class ExtensionsClass
    {
        public static String[] header = new String[5] { "CorrelationId", "NumberOfTrades", "Limit", "Trade_Text", "TradeID" };
        public static CheckRowsVerdict VerifyRows(this DataRowCollection rows)
        {
            int itemValue = 0;
            foreach (DataRow row in rows)
            {
                var itemArray = row.ItemArray;
                if (itemArray.Length != 5)
                    return CheckRowsVerdict.RowsAreNotOfTheSameSize;
                foreach (var item in itemArray)
                {
                    if (item.ToString().Length == 0)
                        return CheckRowsVerdict.SomeValuesAreEmpty;
                    if (!int.TryParse(item.ToString(), out itemValue))
                        return CheckRowsVerdict.ValuesTypesAreNotAsExpected;
                }
            }
            return CheckRowsVerdict.AllGood;
        }
        public static CheckColumnsVerdict VerifyColumns(this DataColumnCollection columns)
        {
            if (columns.Count != 5)
                return CheckColumnsVerdict.SomeColumnsAreMissing;
            var values = (from c in columns.Cast<DataColumn>()
                select c.ColumnName).ToList();
            for (int cnt = 0; cnt < 5; cnt++)
            {
                if (values[cnt].Length == 0)
                    return CheckColumnsVerdict.SomeColumnsAreMissing;
                if (!values.Contains(header[cnt]))
                    return CheckColumnsVerdict.ColumnsValuesAreNotAsExpected;
            }
            return CheckColumnsVerdict.AllGood;
        }
    }
}
