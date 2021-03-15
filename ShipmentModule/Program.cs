using System;
using CommandLine;
using ShipmentModule;

namespace ShipmentUPS
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var objResult = Parser.Default.ParseArguments<Options>(args);
            objResult.WithParsed(o => new ExecuteModule(o));
        }
    }
}
