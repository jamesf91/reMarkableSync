
namespace RemarkableSync.OnenoteAddin
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutRemarkable = new System.Windows.Forms.TableLayoutPanel();
            this.labelRemarkable = new System.Windows.Forms.Label();
            this.labelOTC = new System.Windows.Forms.Label();
            this.textOtc = new System.Windows.Forms.TextBox();
            this.btnRemarkableApply = new System.Windows.Forms.Button();
            this.tableLayoutMyscript = new System.Windows.Forms.TableLayoutPanel();
            this.labelAppKey = new System.Windows.Forms.Label();
            this.labelHmacKey = new System.Windows.Forms.Label();
            this.textAppKey = new System.Windows.Forms.TextBox();
            this.textHmacKey = new System.Windows.Forms.TextBox();
            this.btnMyScriptApply = new System.Windows.Forms.Button();
            this.labelMyscript = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutRemarkable.SuspendLayout();
            this.tableLayoutMyscript.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutRemarkable, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutMyscript, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(10);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(484, 261);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutRemarkable
            // 
            this.tableLayoutRemarkable.ColumnCount = 3;
            this.tableLayoutRemarkable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutRemarkable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRemarkable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutRemarkable.Controls.Add(this.labelRemarkable, 0, 0);
            this.tableLayoutRemarkable.Controls.Add(this.labelOTC, 0, 1);
            this.tableLayoutRemarkable.Controls.Add(this.textOtc, 1, 1);
            this.tableLayoutRemarkable.Controls.Add(this.btnRemarkableApply, 2, 1);
            this.tableLayoutRemarkable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRemarkable.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutRemarkable.Name = "tableLayoutRemarkable";
            this.tableLayoutRemarkable.RowCount = 2;
            this.tableLayoutRemarkable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutRemarkable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutRemarkable.Size = new System.Drawing.Size(478, 90);
            this.tableLayoutRemarkable.TabIndex = 0;
            // 
            // labelRemarkable
            // 
            this.labelRemarkable.AutoSize = true;
            this.tableLayoutRemarkable.SetColumnSpan(this.labelRemarkable, 3);
            this.labelRemarkable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRemarkable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRemarkable.Location = new System.Drawing.Point(3, 0);
            this.labelRemarkable.Name = "labelRemarkable";
            this.labelRemarkable.Size = new System.Drawing.Size(472, 36);
            this.labelRemarkable.TabIndex = 0;
            this.labelRemarkable.Text = "reMarkable Tablet Setup";
            this.labelRemarkable.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOTC
            // 
            this.labelOTC.AutoSize = true;
            this.labelOTC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelOTC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOTC.Location = new System.Drawing.Point(3, 36);
            this.labelOTC.Name = "labelOTC";
            this.labelOTC.Size = new System.Drawing.Size(113, 54);
            this.labelOTC.TabIndex = 1;
            this.labelOTC.Text = "One Time Code";
            this.labelOTC.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textOtc
            // 
            this.textOtc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textOtc.Location = new System.Drawing.Point(122, 53);
            this.textOtc.Name = "textOtc";
            this.textOtc.Size = new System.Drawing.Size(233, 20);
            this.textOtc.TabIndex = 2;
            // 
            // btnRemarkableApply
            // 
            this.btnRemarkableApply.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRemarkableApply.Location = new System.Drawing.Point(373, 49);
            this.btnRemarkableApply.Name = "btnRemarkableApply";
            this.btnRemarkableApply.Size = new System.Drawing.Size(90, 28);
            this.btnRemarkableApply.TabIndex = 3;
            this.btnRemarkableApply.Text = "Apply";
            this.btnRemarkableApply.UseVisualStyleBackColor = true;
            this.btnRemarkableApply.Click += new System.EventHandler(this.btnRemarkableApply_Click);
            // 
            // tableLayoutMyscript
            // 
            this.tableLayoutMyscript.ColumnCount = 3;
            this.tableLayoutMyscript.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutMyscript.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutMyscript.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutMyscript.Controls.Add(this.labelAppKey, 0, 1);
            this.tableLayoutMyscript.Controls.Add(this.labelHmacKey, 0, 2);
            this.tableLayoutMyscript.Controls.Add(this.textAppKey, 1, 1);
            this.tableLayoutMyscript.Controls.Add(this.textHmacKey, 1, 2);
            this.tableLayoutMyscript.Controls.Add(this.btnMyScriptApply, 2, 2);
            this.tableLayoutMyscript.Controls.Add(this.labelMyscript, 0, 0);
            this.tableLayoutMyscript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMyscript.Location = new System.Drawing.Point(3, 99);
            this.tableLayoutMyscript.Name = "tableLayoutMyscript";
            this.tableLayoutMyscript.RowCount = 3;
            this.tableLayoutMyscript.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutMyscript.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutMyscript.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutMyscript.Size = new System.Drawing.Size(478, 138);
            this.tableLayoutMyscript.TabIndex = 1;
            // 
            // labelAppKey
            // 
            this.labelAppKey.AutoSize = true;
            this.labelAppKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAppKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAppKey.Location = new System.Drawing.Point(3, 55);
            this.labelAppKey.Name = "labelAppKey";
            this.labelAppKey.Size = new System.Drawing.Size(113, 41);
            this.labelAppKey.TabIndex = 1;
            this.labelAppKey.Text = "App Key";
            this.labelAppKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelHmacKey
            // 
            this.labelHmacKey.AutoSize = true;
            this.labelHmacKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHmacKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHmacKey.Location = new System.Drawing.Point(3, 96);
            this.labelHmacKey.Name = "labelHmacKey";
            this.labelHmacKey.Size = new System.Drawing.Size(113, 42);
            this.labelHmacKey.TabIndex = 2;
            this.labelHmacKey.Text = "HMAC Key";
            this.labelHmacKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textAppKey
            // 
            this.textAppKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textAppKey.Location = new System.Drawing.Point(122, 65);
            this.textAppKey.Name = "textAppKey";
            this.textAppKey.Size = new System.Drawing.Size(233, 20);
            this.textAppKey.TabIndex = 3;
            // 
            // textHmacKey
            // 
            this.textHmacKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textHmacKey.Location = new System.Drawing.Point(122, 107);
            this.textHmacKey.Name = "textHmacKey";
            this.textHmacKey.Size = new System.Drawing.Size(233, 20);
            this.textHmacKey.TabIndex = 4;
            // 
            // btnMyScriptApply
            // 
            this.btnMyScriptApply.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnMyScriptApply.Location = new System.Drawing.Point(373, 103);
            this.btnMyScriptApply.Name = "btnMyScriptApply";
            this.btnMyScriptApply.Size = new System.Drawing.Size(90, 28);
            this.btnMyScriptApply.TabIndex = 5;
            this.btnMyScriptApply.Text = "Apply";
            this.btnMyScriptApply.UseVisualStyleBackColor = true;
            this.btnMyScriptApply.Click += new System.EventHandler(this.btnMyScriptApply_Click);
            // 
            // labelMyscript
            // 
            this.labelMyscript.AutoSize = true;
            this.tableLayoutMyscript.SetColumnSpan(this.labelMyscript, 3);
            this.labelMyscript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelMyscript.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMyscript.Location = new System.Drawing.Point(3, 0);
            this.labelMyscript.Name = "labelMyscript";
            this.labelMyscript.Size = new System.Drawing.Size(472, 55);
            this.labelMyscript.TabIndex = 0;
            this.labelMyscript.Text = "MyScript Setup";
            this.labelMyscript.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 261);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutRemarkable.ResumeLayout(false);
            this.tableLayoutRemarkable.PerformLayout();
            this.tableLayoutMyscript.ResumeLayout(false);
            this.tableLayoutMyscript.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRemarkable;
        private System.Windows.Forms.Label labelRemarkable;
        private System.Windows.Forms.TableLayoutPanel tableLayoutMyscript;
        private System.Windows.Forms.Label labelOTC;
        private System.Windows.Forms.Label labelMyscript;
        private System.Windows.Forms.Label labelAppKey;
        private System.Windows.Forms.Label labelHmacKey;
        private System.Windows.Forms.TextBox textOtc;
        private System.Windows.Forms.TextBox textAppKey;
        private System.Windows.Forms.TextBox textHmacKey;
        private System.Windows.Forms.Button btnRemarkableApply;
        private System.Windows.Forms.Button btnMyScriptApply;
    }
}