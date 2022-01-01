
namespace RemarkableSync.OnenoteAddin
{
    partial class RmDownloadForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RmDownloadForm));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.rmTreeView = new System.Windows.Forms.TreeView();
            this.chkImportAsGraphics = new System.Windows.Forms.CheckBox();
            this.tableLayoutGraphicWidth = new System.Windows.Forms.TableLayoutPanel();
            this.lblGraphicWidth = new System.Windows.Forms.Label();
            this.numericGraphicWidth = new System.Windows.Forms.NumericUpDown();
            this.lblGraphicWidthUnit = new System.Windows.Forms.Label();
            this.tableLayoutPanel.SuspendLayout();
            this.tableLayoutGraphicWidth.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericGraphicWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.btnCancel, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.lblInfo, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.btnOk, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.rmTreeView, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.chkImportAsGraphics, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.tableLayoutGraphicWidth, 1, 2);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblInfo
            // 
            resources.ApplyResources(this.lblInfo, "lblInfo");
            this.tableLayoutPanel.SetColumnSpan(this.lblInfo, 2);
            this.lblInfo.Name = "lblInfo";
            // 
            // btnOk
            // 
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // rmTreeView
            // 
            this.tableLayoutPanel.SetColumnSpan(this.rmTreeView, 2);
            resources.ApplyResources(this.rmTreeView, "rmTreeView");
            this.rmTreeView.Name = "rmTreeView";
            // 
            // chkImportAsGraphics
            // 
            resources.ApplyResources(this.chkImportAsGraphics, "chkImportAsGraphics");
            this.chkImportAsGraphics.Name = "chkImportAsGraphics";
            this.chkImportAsGraphics.UseVisualStyleBackColor = true;
            // 
            // tableLayoutGraphicWidth
            // 
            resources.ApplyResources(this.tableLayoutGraphicWidth, "tableLayoutGraphicWidth");
            this.tableLayoutGraphicWidth.Controls.Add(this.lblGraphicWidth, 0, 0);
            this.tableLayoutGraphicWidth.Controls.Add(this.numericGraphicWidth, 1, 0);
            this.tableLayoutGraphicWidth.Controls.Add(this.lblGraphicWidthUnit, 2, 0);
            this.tableLayoutGraphicWidth.Name = "tableLayoutGraphicWidth";
            // 
            // lblGraphicWidth
            // 
            resources.ApplyResources(this.lblGraphicWidth, "lblGraphicWidth");
            this.lblGraphicWidth.Name = "lblGraphicWidth";
            // 
            // numericGraphicWidth
            // 
            resources.ApplyResources(this.numericGraphicWidth, "numericGraphicWidth");
            this.numericGraphicWidth.Name = "numericGraphicWidth";
            this.numericGraphicWidth.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // lblGraphicWidthUnit
            // 
            resources.ApplyResources(this.lblGraphicWidthUnit, "lblGraphicWidthUnit");
            this.lblGraphicWidthUnit.Name = "lblGraphicWidthUnit";
            // 
            // RmDownloadForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "RmDownloadForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RmDownloadForm_FormClosing);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.tableLayoutGraphicWidth.ResumeLayout(false);
            this.tableLayoutGraphicWidth.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericGraphicWidth)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TreeView rmTreeView;
        private System.Windows.Forms.CheckBox chkImportAsGraphics;
        private System.Windows.Forms.TableLayoutPanel tableLayoutGraphicWidth;
        private System.Windows.Forms.Label lblGraphicWidth;
        private System.Windows.Forms.NumericUpDown numericGraphicWidth;
        private System.Windows.Forms.Label lblGraphicWidthUnit;
    }
}