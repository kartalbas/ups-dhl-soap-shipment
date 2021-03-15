using System;
namespace ShipmentReports
{
    partial class ShipmentClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDeleteDHL = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTrackingnumber = new System.Windows.Forms.TextBox();
            this.btnDeleteUPS = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnPrintManifest = new System.Windows.Forms.Button();
            this.btnDoManifest = new System.Windows.Forms.Button();
            this.btnDHLSummary = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSummaryDate = new System.Windows.Forms.TextBox();
            this.btnUPSSummary = new System.Windows.Forms.Button();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnPrintManifestRange = new System.Windows.Forms.Button();
            this.btnPrintDHLSummaryRange = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textDate2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textDate1 = new System.Windows.Forms.TextBox();
            this.btnPrintUPSSummaryRange = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnDeleteDHL);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtTrackingnumber);
            this.groupBox1.Controls.Add(this.btnDeleteUPS);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(254, 171);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Storno";
            // 
            // btnDeleteDHL
            // 
            this.btnDeleteDHL.Location = new System.Drawing.Point(129, 89);
            this.btnDeleteDHL.Margin = new System.Windows.Forms.Padding(4);
            this.btnDeleteDHL.Name = "btnDeleteDHL";
            this.btnDeleteDHL.Size = new System.Drawing.Size(111, 48);
            this.btnDeleteDHL.TabIndex = 3;
            this.btnDeleteDHL.Text = "DHL";
            this.btnDeleteDHL.UseVisualStyleBackColor = true;
            this.btnDeleteDHL.Click += new System.EventHandler(this.btnDeleteDHL_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 37);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Trackingnummer";
            // 
            // txtTrackingnumber
            // 
            this.txtTrackingnumber.AcceptsReturn = true;
            this.txtTrackingnumber.Location = new System.Drawing.Point(8, 57);
            this.txtTrackingnumber.Margin = new System.Windows.Forms.Padding(4);
            this.txtTrackingnumber.Name = "txtTrackingnumber";
            this.txtTrackingnumber.Size = new System.Drawing.Size(232, 23);
            this.txtTrackingnumber.TabIndex = 1;
            this.txtTrackingnumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnDeleteUPS
            // 
            this.btnDeleteUPS.Location = new System.Drawing.Point(8, 89);
            this.btnDeleteUPS.Margin = new System.Windows.Forms.Padding(4);
            this.btnDeleteUPS.Name = "btnDeleteUPS";
            this.btnDeleteUPS.Size = new System.Drawing.Size(113, 48);
            this.btnDeleteUPS.TabIndex = 2;
            this.btnDeleteUPS.Text = "UPS";
            this.btnDeleteUPS.UseVisualStyleBackColor = true;
            this.btnDeleteUPS.Click += new System.EventHandler(this.btnDeleteUPS_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnPrintManifest);
            this.groupBox2.Controls.Add(this.btnDoManifest);
            this.groupBox2.Controls.Add(this.btnDHLSummary);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.txtSummaryDate);
            this.groupBox2.Controls.Add(this.btnUPSSummary);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(278, 15);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(376, 171);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Tagesbericht";
            // 
            // btnPrintManifest
            // 
            this.btnPrintManifest.Location = new System.Drawing.Point(258, 102);
            this.btnPrintManifest.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrintManifest.Name = "btnPrintManifest";
            this.btnPrintManifest.Size = new System.Drawing.Size(110, 61);
            this.btnPrintManifest.TabIndex = 7;
            this.btnPrintManifest.Text = "2. DHL Print Manifest";
            this.btnPrintManifest.UseVisualStyleBackColor = true;
            this.btnPrintManifest.Click += new System.EventHandler(this.btnPrintManifest_Click);
            // 
            // btnDoManifest
            // 
            this.btnDoManifest.Location = new System.Drawing.Point(258, 15);
            this.btnDoManifest.Margin = new System.Windows.Forms.Padding(4);
            this.btnDoManifest.Name = "btnDoManifest";
            this.btnDoManifest.Size = new System.Drawing.Size(110, 78);
            this.btnDoManifest.TabIndex = 6;
            this.btnDoManifest.Text = "1. DHL Do Manifest Today";
            this.btnDoManifest.UseVisualStyleBackColor = true;
            this.btnDoManifest.Click += new System.EventHandler(this.btnDoManifest_Click);
            // 
            // btnDHLSummary
            // 
            this.btnDHLSummary.Location = new System.Drawing.Point(130, 88);
            this.btnDHLSummary.Margin = new System.Windows.Forms.Padding(4);
            this.btnDHLSummary.Name = "btnDHLSummary";
            this.btnDHLSummary.Size = new System.Drawing.Size(111, 48);
            this.btnDHLSummary.TabIndex = 5;
            this.btnDHLSummary.Text = "DHL";
            this.btnDHLSummary.UseVisualStyleBackColor = true;
            this.btnDHLSummary.Click += new System.EventHandler(this.btnDHLSummary_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Datum";
            // 
            // txtSummaryDate
            // 
            this.txtSummaryDate.AcceptsReturn = true;
            this.txtSummaryDate.Location = new System.Drawing.Point(11, 57);
            this.txtSummaryDate.Margin = new System.Windows.Forms.Padding(4);
            this.txtSummaryDate.Name = "txtSummaryDate";
            this.txtSummaryDate.Size = new System.Drawing.Size(230, 23);
            this.txtSummaryDate.TabIndex = 3;
            this.txtSummaryDate.Text = "14.03.2012";
            this.txtSummaryDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnUPSSummary
            // 
            this.btnUPSSummary.Location = new System.Drawing.Point(11, 89);
            this.btnUPSSummary.Margin = new System.Windows.Forms.Padding(4);
            this.btnUPSSummary.Name = "btnUPSSummary";
            this.btnUPSSummary.Size = new System.Drawing.Size(111, 48);
            this.btnUPSSummary.TabIndex = 4;
            this.btnUPSSummary.Text = "UPS";
            this.btnUPSSummary.UseVisualStyleBackColor = true;
            this.btnUPSSummary.Click += new System.EventHandler(this.btnUPSSummary_Click);
            // 
            // txtConsole
            // 
            this.txtConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConsole.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtConsole.Location = new System.Drawing.Point(16, 193);
            this.txtConsole.Margin = new System.Windows.Forms.Padding(4);
            this.txtConsole.Multiline = true;
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            this.txtConsole.Size = new System.Drawing.Size(1233, 547);
            this.txtConsole.TabIndex = 2;
            this.txtConsole.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnPrintManifestRange);
            this.groupBox3.Controls.Add(this.btnPrintDHLSummaryRange);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.textDate2);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textDate1);
            this.groupBox3.Controls.Add(this.btnPrintUPSSummaryRange);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(683, 15);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox3.Size = new System.Drawing.Size(377, 171);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Tagesbericht";
            // 
            // btnPrintManifestRange
            // 
            this.btnPrintManifestRange.Location = new System.Drawing.Point(258, 58);
            this.btnPrintManifestRange.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrintManifestRange.Name = "btnPrintManifestRange";
            this.btnPrintManifestRange.Size = new System.Drawing.Size(95, 79);
            this.btnPrintManifestRange.TabIndex = 9;
            this.btnPrintManifestRange.Text = "DHL Print Manifest";
            this.btnPrintManifestRange.UseVisualStyleBackColor = true;
            this.btnPrintManifestRange.Click += new System.EventHandler(this.btnPrintManifestRange_Click);
            // 
            // btnPrintDHLSummaryRange
            // 
            this.btnPrintDHLSummaryRange.Location = new System.Drawing.Point(128, 88);
            this.btnPrintDHLSummaryRange.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrintDHLSummaryRange.Name = "btnPrintDHLSummaryRange";
            this.btnPrintDHLSummaryRange.Size = new System.Drawing.Size(114, 48);
            this.btnPrintDHLSummaryRange.TabIndex = 8;
            this.btnPrintDHLSummaryRange.Text = "DHL";
            this.btnPrintDHLSummaryRange.UseVisualStyleBackColor = true;
            this.btnPrintDHLSummaryRange.Click += new System.EventHandler(this.btnPrintDHLSummaryRange_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(125, 37);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Datum End";
            // 
            // textDate2
            // 
            this.textDate2.AcceptsReturn = true;
            this.textDate2.Location = new System.Drawing.Point(127, 57);
            this.textDate2.Margin = new System.Windows.Forms.Padding(4);
            this.textDate2.Name = "textDate2";
            this.textDate2.Size = new System.Drawing.Size(115, 23);
            this.textDate2.TabIndex = 6;
            this.textDate2.Text = "14.03.2012";
            this.textDate2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 37);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Datum Begin";
            // 
            // textDate1
            // 
            this.textDate1.AcceptsReturn = true;
            this.textDate1.Location = new System.Drawing.Point(17, 57);
            this.textDate1.Margin = new System.Windows.Forms.Padding(4);
            this.textDate1.Name = "textDate1";
            this.textDate1.Size = new System.Drawing.Size(100, 23);
            this.textDate1.TabIndex = 5;
            this.textDate1.Text = "14.03.2012";
            this.textDate1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnPrintUPSSummaryRange
            // 
            this.btnPrintUPSSummaryRange.Location = new System.Drawing.Point(17, 89);
            this.btnPrintUPSSummaryRange.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrintUPSSummaryRange.Name = "btnPrintUPSSummaryRange";
            this.btnPrintUPSSummaryRange.Size = new System.Drawing.Size(100, 48);
            this.btnPrintUPSSummaryRange.TabIndex = 7;
            this.btnPrintUPSSummaryRange.Text = "UPS";
            this.btnPrintUPSSummaryRange.UseVisualStyleBackColor = true;
            this.btnPrintUPSSummaryRange.Click += new System.EventHandler(this.btnPrintUPSSummaryRange_Click);
            // 
            // ShipmentClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1262, 753);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ShipmentClient";
            this.Text = "ShipmentClient";
            this.Load += new System.EventHandler(this.ShipmentClient_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTrackingnumber;
        private System.Windows.Forms.Button btnDeleteUPS;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSummaryDate;
        private System.Windows.Forms.Button btnUPSSummary;
        private System.Windows.Forms.TextBox txtConsole;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textDate2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textDate1;
        private System.Windows.Forms.Button btnPrintUPSSummaryRange;
        private System.Windows.Forms.Button btnDeleteDHL;
        private System.Windows.Forms.Button btnDHLSummary;
        private System.Windows.Forms.Button btnPrintManifest;
        private System.Windows.Forms.Button btnDoManifest;
        private System.Windows.Forms.Button btnPrintManifestRange;
        private System.Windows.Forms.Button btnPrintDHLSummaryRange;
    }
}

