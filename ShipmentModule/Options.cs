using CommandLine;

namespace ShipmentModule
{
    public enum Mode
    {
        TOOL=0,
        UPS=1,
        DHL=2
    }

    public enum Report
    {
        NO=0,
        YES=1
    }

    public class Options
    {
        public Options()
        {
            mode = -1;
            cmd = string.Empty;
            number = "-1";
            datebegin = string.Empty;
            dateend = string.Empty;
        }
        //--mode=1 --cmd=REPORT --dateBegin=08.01.2016 --dateEnd=09.01.2016
        [Option(Required = true, HelpText = "1=UPS, 2=DHL")]
        public int mode { get; set; }

        [Option(Required = true, HelpText = "UPS/DHL: DELETE, SHIP or REPORT")]
        public string cmd { get; set; }

        [Option(Required = false, HelpText = "Order/Tracking Number ")]
        public string number { get; set; }

        [Option(Required = false, HelpText = "Begin Date of Report")]
        public string datebegin { get; set; }

        [Option(Required = false, HelpText = "End Date of Report")]
        public string dateend { get; set; }
    }
}
