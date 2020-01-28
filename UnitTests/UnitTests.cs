using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpBNPTest;

namespace UnitTests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod, Owner("MontaF")]
        public void FullUnitTest()
        {
            string inputFile = GetTestFilePath("UTinput.xml");
            var testProcess = new ProcessFile(inputFile);

            StreamWriter log;
            var logPath = GetTestFilePath("server.log");
            if (!File.Exists(logPath))
                log = new StreamWriter(logPath);
            else
                log = File.AppendText(logPath);

            TradeResult tradeResult = testProcess.Run(writeCsv: false, log);

            Assert.AreEqual(3, tradeResult.TradeValues.Count, "TradeValues list is missing some values.");

            Assert.AreEqual(new Tuple<int, int, TradeResult.TradeState>(200, 2, TradeResult.TradeState.Pending), tradeResult.TradeValues[0]);
            Assert.AreEqual(new Tuple<int, int, TradeResult.TradeState>(222, 1, TradeResult.TradeState.Rejected), tradeResult.TradeValues[1]);
            Assert.AreEqual(new Tuple<int, int, TradeResult.TradeState>(234, 3, TradeResult.TradeState.Accepted), tradeResult.TradeValues[2]);
        }
        [TestMethod, Owner("MontaF")]
        public void BadFileUnitTest()
        {
            string inputFile = GetTestFilePath("inputWithError.xml");
            var testProcess = new ProcessFile(inputFile);

            StreamWriter log;
            var logPath = GetTestFilePath("BFserver.log");
            if (!File.Exists(logPath))
                log = new StreamWriter(logPath);
            else
                log = File.AppendText(logPath);

            TradeResult tradeResult = testProcess.Run(writeCsv: false, log);

            Assert.IsNull(tradeResult, "TradeValues list should be null since the input file is corrupted.");
        }

        public string GetTestFilePath(string fileName)
        {
            string testFilesFolder = @"C:\BNPTest\UnitTests";
            var inputFiles = Directory.GetFiles(testFilesFolder, fileName);
            if (inputFiles.Length == 0)
                throw new Exception("No file was found with the given fileName");
            if(inputFiles.Length > 1)
                throw new Exception("More than one file was found, please use a more precise fileName");
            return inputFiles[0];
        }
    }
}
