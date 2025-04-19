using combit.ListLabel18;
using combit.ListLabel18.DataProviders;
using System.Reflection;
using System.Diagnostics;

namespace ShipmentLib
{
    public class PrintListLabel
    {
        ListLabel _ll;
        private string _strAssembly = Assembly.GetExecutingAssembly().GetName().Name;

        public PrintListLabel()
        {
            _ll = new ListLabel();

            if(!SettingController.LLLicenseInfo.Equals("trial"))
                _ll.LicensingInfo = SettingController.LLLicenseInfo;

            _ll.Unit = LlUnits.Millimeter_1_100;
        }

        public void DesignLLHtml(string strLLFile, string strFile, bool bDesign, bool bPreview)
        {
            try
            {
                string strMethod = MethodBase.GetCurrentMethod().Name;
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing file " + strFile);
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing with options Design=" + bDesign + " Preview=" + bPreview);

                LLDataSource ds = new LLDataSource();
                ds.Html = strFile;
                ds.Picture = strFile;

                _ll.DataSource = new ObjectDataProvider(ds);
                _ll.DataMember = "LLDataSource";
                _ll.AutoMasterMode = LlAutoMasterMode.AsVariables;

                _ll.AutoProjectType = LlProject.List;
                _ll.AutoDestination = LlPrintMode.Normal;
                _ll.AutoShowSelectFile = false;
                _ll.AutoProjectFile = strLLFile;

                _ll.AutoDefineVariable += LL_AutoDefineVariable;


                if (bDesign)
                {
                    _ll.Design();
                }
                else
                {
                    if (bPreview)
                    {
                        _ll.AutoShowPrintOptions = true;
                        _ll.AutoDestination = LlPrintMode.Export;
                    }
                    else
                    {
                        _ll.AutoShowPrintOptions = false;
                        _ll.AutoDestination = LlPrintMode.Normal;
                    }

                    _ll.Print();
                }

                _ll.AutoDefineVariable -= LL_AutoDefineVariable;
            }
            catch (ListLabelException)
            {
                throw;
            }
        }

        private void LL_AutoDefineVariable(object sender, AutoDefineElementEventArgs e)
        {
            if(e.Name.Equals("LLDataSource.Picture"))
                e.FieldType = LlFieldType.Drawing;
        }


        public void DesignLLSummary(string strLLFile, bool bDesign, bool bPreview, ShipmentSummary objShipmentSummary)
        {
            try
            {
                string strMethod = MethodBase.GetCurrentMethod().Name;
                Logger.Instance.Log(TraceEventType.Information, 0, _strAssembly + ":" + strMethod + ": Printing summary with options Design=" + bDesign + " Preview=" + bPreview);

                var objProvider = new ObjectDataProvider(objShipmentSummary);
                objProvider.RootTableName = "Shipment";
                _ll.DataMember = "Shipment";
                _ll.DataSource = objProvider;
                _ll.AutoMasterMode = LlAutoMasterMode.AsVariables;

                _ll.AutoProjectType = LlProject.List;
                _ll.AutoDestination = LlPrintMode.Normal;
                _ll.AutoShowSelectFile = false;
                _ll.AutoProjectFile = strLLFile;

                if (bDesign)
                {
                    _ll.Design();
                }
                else
                {
                    if (bPreview)
                    {
                        _ll.AutoShowPrintOptions = true;
                        _ll.AutoDestination = LlPrintMode.Export;
                    }
                    else
                    {
                        _ll.AutoShowPrintOptions = false;
                        _ll.AutoDestination = LlPrintMode.Normal;
                    }

                    _ll.Print();
                }
            }
            catch (ListLabelException)
            {
                throw;
            }
        }
    }
}
