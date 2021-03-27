using System;
using System.Diagnostics;
using ShipmentLib;
using System.Reflection;

namespace ShipmentModule
{
    public class ExecuteModule
    {
        private Options _objOptions;
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        private string _sOptions;
        public ExecuteModule(Options objOptions)
        {
            string strMethod = "ExecuteModule";

            _objOptions = objOptions;
            _sOptions = " --mode=" + _objOptions.mode + " --cmd=" + _objOptions.cmd + " --number=" + _objOptions.number + " --datebegin=" + _objOptions.datebegin + " --dateend=" + _objOptions.dateend;
            Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": " + _sOptions);
            BranchToModules();
        }

        private void BranchToModules()
        {
            switch(_objOptions.mode)
            {
                case 0:
                    HandleTool();
                    break;
                case 1:
                    HandleUPS();
                    break;
                case 2:
                    HandleDHL();
                    break;
            }
        }

        private void HandleTool()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;
            if (_objOptions.cmd.Equals("MOVETRACKINGNUMBERS"))
            {
                DateTime objDateBegin = ShipmentTools.ParseDate(_objOptions.datebegin, false);
                DateTime objDateEnd = ShipmentTools.ParseDate(_objOptions.dateend, false);
                WegaDB.MoveTrackingnumbers(objDateBegin, objDateEnd);
            }

        }

        private void HandleDHL()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ShipmentDHL.ShipmentDhlClient objRequest = new ShipmentDHL.ShipmentDhlClient(_objOptions.number, _objOptions.cmd);

            //DHL: REPORT
            if (_objOptions.cmd.Equals("REPORT"))
            {
                objRequest.PrintSummary(_objOptions.datebegin, _objOptions.dateend);
            }
            //DHL: DOMANIFEST
            else if (_objOptions.cmd.Equals("DOMANIFEST"))
            {
                objRequest.DoManifestTodayDD();
            }
            //DHL: GETMANIFEST
            else if (_objOptions.cmd.Equals("GETMANIFEST"))
            {
                objRequest.GetManifestDD(_objOptions.datebegin, _objOptions.dateend);
            }
            //DHL: DELETE
            else if (_objOptions.cmd.Equals("DELETE"))
            {
                objRequest.DeleteOldShipmentDD();
            }
            //DHL: Ship
            else if (_objOptions.cmd.Equals("SHIP"))
            {
                objRequest.CreateShipment();
            }
            //Quit
            else
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Input Error! Command line not understood: " + _sOptions);

        }

        private void HandleUPS()
        {
            string strMethod = MethodBase.GetCurrentMethod().Name;

            ShipmentUPS.ShipmentRequestBuilder objRequest = new ShipmentUPS.ShipmentRequestBuilder(_objOptions.number);

            //UPS: REPORT
            if (_objOptions.cmd.Equals("REPORT"))
            {
                objRequest.PrintSummary(_objOptions.datebegin, _objOptions.dateend);
            }
            //UPS: DELETE
            else if (_objOptions.cmd.Equals("DELETE"))
            {
                if (_objOptions.number.Length < 12)
                    new ShipmentException(null, _strAssembly + ":" + strMethod + ": Input Error! Length of UPS Trackingnumber is lower than 12 chars!");

                objRequest.DeleteOldShipment(objRequest, _objOptions.number);
            }
            //UPS: Ship
            else if (_objOptions.cmd.Equals("SHIP"))
            {
                objRequest = new ShipmentUPS.ShipmentRequestBuilder(_objOptions.number);

                ShipmentUPS.Shipment objShipment = objRequest.InitShipment();
                if (objShipment == null)
                    new ShipmentException(new Exception(), _strAssembly + ":" + strMethod + ": Initializing shipment FAILED!");

                objRequest.CreateNewShipment(objRequest, objShipment);

                Environment.Exit(0);
            }
            //Quit
            else
                new ShipmentException(null, _strAssembly + ":" + strMethod + ": Input Error! Command line not understood: " + _sOptions);
        }
    }
}
