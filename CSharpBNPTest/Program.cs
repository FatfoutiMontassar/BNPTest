using System;
using System.Data;
using System.IO;

namespace CSharpBNPTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // change this path to the path of the 'BNPTest' folder containing the input files.
            const string directoryPath = @"C:\BNPTest";

            // we can also use a package like serilog, but this is simpler
            //var logPath = Path.Join(directoryPath + "server.log");
            var logPath = directoryPath + @"\server.log";
            StreamWriter log;
            
            if (!File.Exists(logPath))
                log = new StreamWriter(logPath);
            else
                log = File.AppendText(logPath);

            var inputFiles = Directory.GetFiles(directoryPath, "input*.xml");
            
            log.WriteLine(DateTime.Now);
            log.WriteLine("{0} input file{1} found.", inputFiles.Length, inputFiles.Length > 1 ? "s were" : " was");

            if (inputFiles.Length == 0)
            {
                log.WriteLine("Make sure you chose the right path for your input files.");
                Environment.Exit(0);
            }

            log.WriteLine("Start processing ...");
            foreach (var file in inputFiles)
            {
                log.WriteLine("Start processing file: {0}", file);
                var newProcess = new ProcessFile(file);
                newProcess.Run(writeCsv: true, log);
                log.WriteLine("Processing file: {0} finished.\n", file);
            }

            log.Dispose();
            log.Close();
            Environment.Exit(0);
        }
    }
    public class ProcessFile
    {
        private string FilePath { get; set; }

        public ProcessFile(string filePath)
        {
            FilePath = filePath;
        }

        public TradeResult Run(bool writeCsv, StreamWriter log)
        {
            // we should use 'using' to dispose the files from the memory after working with them.
            // this isn't enough => toImprove
            using (DataSet dataSet = new DataSet())
            {
                dataSet.ReadXml(FilePath);
                var tables = dataSet.Tables;

                if (tables.Count > 1)
                    log.WriteLine("file contains more than one table, we'll use the first and ignore the rest.");

                var tradesTable = tables[0];

                var columnsVerificationResult = tradesTable.Columns.VerifyColumns();
                if (columnsVerificationResult != CheckColumnsVerdict.AllGood)
                {
                    log.WriteLine("ERR: " + columnsVerificationResult.ToString());
                    return null;
                }

                var rowsVerificationResult = tradesTable.Rows.VerifyRows();
                if (rowsVerificationResult != CheckRowsVerdict.AllGood)
                {
                    log.WriteLine("ERR: " + rowsVerificationResult.ToString());
                    return null;
                }

                TradeTable tradeTable = new TradeTable(tradesTable.Rows);

                // use this command to visualize the tradeTable in the console
                //tradeTable.Visualize();

                TradeResult tradeResult = tradeTable.ComputeTradeResult();
                
                if(writeCsv)
                    tradeResult.WriteAsCsv(FilePath);
                
                return tradeResult;
            }
        }
    }
}
