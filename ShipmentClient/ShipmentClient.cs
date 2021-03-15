using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using ShipmentClient.Properties;
using System.ComponentModel;

namespace ShipmentReports
{
    public partial class ShipmentClient : Form
    {
        #region Form

        public ShipmentClient()
        {
            InitializeComponent();
        }
        private void ShipmentClient_Load(object sender, EventArgs e)
        {
            txtSummaryDate.Text = DateTime.Now.ToString("d", new CultureInfo("de-DE"));
            textDate1.Text = DateTime.Now.AddMonths(-1).ToString("d", new CultureInfo("de-DE"));
            textDate2.Text = DateTime.Now.ToString("d", new CultureInfo("de-DE"));
        }

        #endregion

        #region Delete
        private void btnDeleteUPS_Click(object sender, EventArgs e)
        {
            DisableGui();
            string strCmd = string.Format("--mode=1 --cmd=DELETE --number={0}", txtTrackingnumber.Text);
            StartWorker(strCmd);
        }

        private void btnDeleteDHL_Click(object sender, EventArgs e)
        {
            DisableGui();
            string strCmd = string.Format("--mode=2 --cmd=DELETE --number={0}", txtTrackingnumber.Text);
            StartWorker(strCmd);
        }

        #endregion

        #region Report Today

        private void btnUPSSummary_Click(object sender, EventArgs e)
        {
            DisableGui();

            string strCmd = string.Empty;

            if (CheckDate(txtSummaryDate.Text))
            {
                strCmd = string.Format("--mode=1 --cmd=REPORT --datebegin={0} --dateend={1}", txtSummaryDate.Text, txtSummaryDate.Text);
                StartWorker(strCmd);
            }
            else
            {
                MessageBox.Show("Check the date format: dd.mm.yyyy");
            }
        }

        private void btnDHLSummary_Click(object sender, EventArgs e)
        {
            DisableGui();

            string strCmd = string.Empty;

            if (CheckDate(txtSummaryDate.Text))
            {
                strCmd = string.Format("--mode=2 --cmd=REPORT --datebegin={0} --dateend={1}", txtSummaryDate.Text, txtSummaryDate.Text);
                StartWorker(strCmd);
            }
            else
            {
                MessageBox.Show("Check the date format: dd.mm.yyyy");
            }
        }

        private void btnPrintManifest_Click(object sender, EventArgs e)
        {
            DisableGui();

            string strCmd = string.Empty;

            if (CheckDate(txtSummaryDate.Text))
            {
                strCmd = string.Format("--mode=2 --cmd=GETMANIFEST --datebegin={0} --dateend={1}", txtSummaryDate.Text, txtSummaryDate.Text);
                StartWorker(strCmd);
            }
            else
            {
                MessageBox.Show("Check the date format: dd.mm.yyyy");
            }
        }

        private void btnDoManifest_Click(object sender, EventArgs e)
        {
            DisableGui();
            string strCmd = string.Format("--mode=2 --cmd=DOMANIFEST");
            StartWorker(strCmd);
        }

        #endregion

        #region Report Range

        private void btnPrintUPSSummaryRange_Click(object sender, EventArgs e)
        {
            DisableGui();

            string strCmd = string.Empty;

            if (CheckDate(textDate1.Text) && CheckDate(textDate2.Text))
            {
                strCmd = string.Format("--mode=1 --cmd=REPORT --datebegin={0} --dateend={1}", textDate1.Text, textDate2.Text);
                StartWorker(strCmd);
            }
            else
            {
                MessageBox.Show("Check the date format: dd.mm.yyyy");
            }
        }

        private void btnPrintDHLSummaryRange_Click(object sender, EventArgs e)
        {
            DisableGui();

            string strCmd = string.Empty;

            if (CheckDate(textDate1.Text) && CheckDate(textDate2.Text))
            {
                strCmd = string.Format("--mode=2 --cmd=REPORT --datebegin={0} --dateend={1}", textDate1.Text, textDate2.Text);
                StartWorker(strCmd);
            }
            else
            {
                MessageBox.Show("Check the date format: dd.mm.yyyy");
            }
        }

        private void btnPrintManifestRange_Click(object sender, EventArgs e)
        {
            DisableGui();

            string strCmd = string.Empty;

            if (CheckDate(textDate1.Text) && CheckDate(textDate2.Text))
            {
                strCmd = string.Format("--mode=2 --cmd=GETMANIFEST --datebegin={0} --dateend={1}", textDate1.Text, textDate2.Text);
                StartWorker(strCmd);
            }
            else
            {
                MessageBox.Show("Check the date format: dd.mm.yyyy");
            }
        }

        #endregion

        #region Helper

        private void StartWorker(string strOptions)
        {
            var objWorker = new BackgroundWorker();
            objWorker.WorkerReportsProgress = false;
            objWorker.WorkerSupportsCancellation = false;
            objWorker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            objWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            objWorker.RunWorkerAsync(strOptions);
        }

        private void EnableGui()
        {
            btnUPSSummary.Enabled = true;
            btnDHLSummary.Enabled = true;
            btnDeleteUPS.Enabled = true;
            btnDeleteDHL.Enabled = true;
            btnDoManifest.Enabled = true;
            btnPrintManifest.Enabled = true;
            btnPrintManifestRange.Enabled = true;
            btnPrintDHLSummaryRange.Enabled = true;
            btnPrintUPSSummaryRange.Enabled = true;

            txtSummaryDate.Enabled = true;
            txtTrackingnumber.Enabled = true;
            textDate1.Enabled = true;
            textDate2.Enabled = true;
        }

        private void DisableGui()
        {
            txtConsole.Text = string.Empty;

            btnUPSSummary.Enabled = false;
            btnDHLSummary.Enabled = false;
            btnDeleteUPS.Enabled = false;
            btnDeleteDHL.Enabled = false;
            btnDoManifest.Enabled = false;
            btnPrintManifest.Enabled = false;
            btnPrintManifestRange.Enabled = false;
            btnPrintDHLSummaryRange.Enabled = false;
            btnPrintUPSSummaryRange.Enabled = false;

            txtSummaryDate.Enabled = false;
            txtTrackingnumber.Enabled = false;
            textDate1.Enabled = false;
            textDate2.Enabled = false;
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableGui();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker objWorker = sender as BackgroundWorker;

            if (textDate1.Text.Length == 10 && textDate2.Text.Length == 10)
            {
                ExecuteCommandSync(e.Argument);
            }
            else
                MessageBox.Show("Please improve your date in the textbox!");

        }

        private bool CheckDate(string strDate)
        {
            try
            {
                DateTime objDateTime = DateTime.ParseExact(strDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void ExecuteCommandSync(object objOptions)
        {
            try
            {
                string strCmd = Settings.Default.ShipmentModuleExe;
                strCmd = strCmd + " " + objOptions.ToString();

                ProcessStartInfo objStartInfo = new ProcessStartInfo("cmd", "/c " + strCmd);

                objStartInfo.RedirectStandardOutput = true;
                objStartInfo.UseShellExecute = false;
                objStartInfo.CreateNoWindow = true;
                Process objProcess = new Process();
                objProcess.StartInfo = objStartInfo;
                objProcess.Start();

                txtConsole.Text = string.Empty;

                while (!objProcess.StandardOutput.EndOfStream)
                {
                    Invoke(new MethodInvoker(delegate { txtConsole.Text += objProcess.StandardOutput.ReadLine() + Environment.NewLine; }));
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
