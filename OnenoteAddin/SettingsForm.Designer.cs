
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
            this.tableLayoutOverall = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutRmCloud = new System.Windows.Forms.TableLayoutPanel();
            this.labelRemarkableCloud = new System.Windows.Forms.Label();
            this.labelOTC = new System.Windows.Forms.Label();
            this.textOtc = new System.Windows.Forms.TextBox();
            this.btnRemarkableCloudApply = new System.Windows.Forms.Button();
            this.tableLayoutMyscript = new System.Windows.Forms.TableLayoutPanel();
            this.labelAppKey = new System.Windows.Forms.Label();
            this.labelHmacKey = new System.Windows.Forms.Label();
            this.textAppKey = new System.Windows.Forms.TextBox();
            this.textHmacKey = new System.Windows.Forms.TextBox();
            this.btnMyScriptApply = new System.Windows.Forms.Button();
            this.labelMyscript = new System.Windows.Forms.Label();
            this.tableLayoutRmSelection = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutRmSsh = new System.Windows.Forms.TableLayoutPanel();
            this.labelRemarkableSsh = new System.Windows.Forms.Label();
            this.labelSshPassword = new System.Windows.Forms.Label();
            this.textSshPassword = new System.Windows.Forms.TextBox();
            this.btnRemarkableSshApply = new System.Windows.Forms.Button();
            this.labelRemarkableConnection = new System.Windows.Forms.Label();
            this.radioButtonRmCloud = new System.Windows.Forms.RadioButton();
            this.radioButtonRmSsh = new System.Windows.Forms.RadioButton();
            this.tableLayoutOverall.SuspendLayout();
            this.tableLayoutRmCloud.SuspendLayout();
            this.tableLayoutMyscript.SuspendLayout();
            this.tableLayoutRmSelection.SuspendLayout();
            this.tableLayoutRmSsh.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutOverall
            // 
            this.tableLayoutOverall.ColumnCount = 1;
            this.tableLayoutOverall.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutOverall.Controls.Add(this.tableLayoutRmCloud, 0, 1);
            this.tableLayoutOverall.Controls.Add(this.tableLayoutMyscript, 0, 3);
            this.tableLayoutOverall.Controls.Add(this.tableLayoutRmSelection, 0, 0);
            this.tableLayoutOverall.Controls.Add(this.tableLayoutRmSsh, 0, 2);
            this.tableLayoutOverall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutOverall.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutOverall.Margin = new System.Windows.Forms.Padding(10);
            this.tableLayoutOverall.Name = "tableLayoutOverall";
            this.tableLayoutOverall.RowCount = 5;
            this.tableLayoutOverall.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutOverall.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutOverall.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutOverall.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutOverall.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutOverall.Size = new System.Drawing.Size(484, 461);
            this.tableLayoutOverall.TabIndex = 0;
            // 
            // tableLayoutRmCloud
            // 
            this.tableLayoutRmCloud.ColumnCount = 3;
            this.tableLayoutRmCloud.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutRmCloud.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmCloud.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutRmCloud.Controls.Add(this.labelRemarkableCloud, 0, 0);
            this.tableLayoutRmCloud.Controls.Add(this.labelOTC, 0, 1);
            this.tableLayoutRmCloud.Controls.Add(this.textOtc, 1, 1);
            this.tableLayoutRmCloud.Controls.Add(this.btnRemarkableCloudApply, 2, 1);
            this.tableLayoutRmCloud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRmCloud.Location = new System.Drawing.Point(3, 85);
            this.tableLayoutRmCloud.Name = "tableLayoutRmCloud";
            this.tableLayoutRmCloud.RowCount = 2;
            this.tableLayoutRmCloud.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutRmCloud.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutRmCloud.Size = new System.Drawing.Size(478, 104);
            this.tableLayoutRmCloud.TabIndex = 0;
            // 
            // labelRemarkableCloud
            // 
            this.labelRemarkableCloud.AutoSize = true;
            this.tableLayoutRmCloud.SetColumnSpan(this.labelRemarkableCloud, 3);
            this.labelRemarkableCloud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRemarkableCloud.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRemarkableCloud.Location = new System.Drawing.Point(3, 0);
            this.labelRemarkableCloud.Name = "labelRemarkableCloud";
            this.labelRemarkableCloud.Size = new System.Drawing.Size(472, 41);
            this.labelRemarkableCloud.TabIndex = 0;
            this.labelRemarkableCloud.Text = "reMarkable Cloud Setup";
            this.labelRemarkableCloud.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOTC
            // 
            this.labelOTC.AutoSize = true;
            this.labelOTC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelOTC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOTC.Location = new System.Drawing.Point(3, 41);
            this.labelOTC.Name = "labelOTC";
            this.labelOTC.Size = new System.Drawing.Size(113, 63);
            this.labelOTC.TabIndex = 1;
            this.labelOTC.Text = "One Time Code";
            this.labelOTC.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textOtc
            // 
            this.textOtc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textOtc.Location = new System.Drawing.Point(122, 62);
            this.textOtc.Name = "textOtc";
            this.textOtc.Size = new System.Drawing.Size(233, 20);
            this.textOtc.TabIndex = 2;
            // 
            // btnRemarkableCloudApply
            // 
            this.btnRemarkableCloudApply.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRemarkableCloudApply.Location = new System.Drawing.Point(373, 57);
            this.btnRemarkableCloudApply.Name = "btnRemarkableCloudApply";
            this.btnRemarkableCloudApply.Size = new System.Drawing.Size(90, 30);
            this.btnRemarkableCloudApply.TabIndex = 3;
            this.btnRemarkableCloudApply.Text = "Apply";
            this.btnRemarkableCloudApply.UseVisualStyleBackColor = true;
            this.btnRemarkableCloudApply.Click += new System.EventHandler(this.btnRemarkableApply_Click);
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
            this.tableLayoutMyscript.Location = new System.Drawing.Point(3, 305);
            this.tableLayoutMyscript.Name = "tableLayoutMyscript";
            this.tableLayoutMyscript.RowCount = 3;
            this.tableLayoutMyscript.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutMyscript.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutMyscript.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutMyscript.Size = new System.Drawing.Size(478, 131);
            this.tableLayoutMyscript.TabIndex = 1;
            // 
            // labelAppKey
            // 
            this.labelAppKey.AutoSize = true;
            this.labelAppKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelAppKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAppKey.Location = new System.Drawing.Point(3, 52);
            this.labelAppKey.Name = "labelAppKey";
            this.labelAppKey.Size = new System.Drawing.Size(113, 39);
            this.labelAppKey.TabIndex = 1;
            this.labelAppKey.Text = "App Key";
            this.labelAppKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelHmacKey
            // 
            this.labelHmacKey.AutoSize = true;
            this.labelHmacKey.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHmacKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHmacKey.Location = new System.Drawing.Point(3, 91);
            this.labelHmacKey.Name = "labelHmacKey";
            this.labelHmacKey.Size = new System.Drawing.Size(113, 40);
            this.labelHmacKey.TabIndex = 2;
            this.labelHmacKey.Text = "HMAC Key";
            this.labelHmacKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textAppKey
            // 
            this.textAppKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textAppKey.Location = new System.Drawing.Point(122, 61);
            this.textAppKey.Name = "textAppKey";
            this.textAppKey.Size = new System.Drawing.Size(233, 20);
            this.textAppKey.TabIndex = 3;
            // 
            // textHmacKey
            // 
            this.textHmacKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textHmacKey.Location = new System.Drawing.Point(122, 101);
            this.textHmacKey.Name = "textHmacKey";
            this.textHmacKey.Size = new System.Drawing.Size(233, 20);
            this.textHmacKey.TabIndex = 4;
            // 
            // btnMyScriptApply
            // 
            this.btnMyScriptApply.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnMyScriptApply.Location = new System.Drawing.Point(373, 96);
            this.btnMyScriptApply.Name = "btnMyScriptApply";
            this.btnMyScriptApply.Size = new System.Drawing.Size(90, 30);
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
            this.labelMyscript.Size = new System.Drawing.Size(472, 52);
            this.labelMyscript.TabIndex = 0;
            this.labelMyscript.Text = "MyScript Setup";
            this.labelMyscript.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutRmSelection
            // 
            this.tableLayoutRmSelection.ColumnCount = 2;
            this.tableLayoutRmSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmSelection.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmSelection.Controls.Add(this.labelRemarkableConnection, 0, 0);
            this.tableLayoutRmSelection.Controls.Add(this.radioButtonRmCloud, 0, 1);
            this.tableLayoutRmSelection.Controls.Add(this.radioButtonRmSsh, 1, 1);
            this.tableLayoutRmSelection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRmSelection.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutRmSelection.Name = "tableLayoutRmSelection";
            this.tableLayoutRmSelection.RowCount = 2;
            this.tableLayoutRmSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmSelection.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmSelection.Size = new System.Drawing.Size(478, 76);
            this.tableLayoutRmSelection.TabIndex = 2;
            // 
            // tableLayoutRmSsh
            // 
            this.tableLayoutRmSsh.ColumnCount = 3;
            this.tableLayoutRmSsh.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutRmSsh.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmSsh.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutRmSsh.Controls.Add(this.labelRemarkableSsh, 0, 0);
            this.tableLayoutRmSsh.Controls.Add(this.labelSshPassword, 0, 1);
            this.tableLayoutRmSsh.Controls.Add(this.textSshPassword, 1, 1);
            this.tableLayoutRmSsh.Controls.Add(this.btnRemarkableSshApply, 2, 1);
            this.tableLayoutRmSsh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRmSsh.Location = new System.Drawing.Point(3, 195);
            this.tableLayoutRmSsh.Name = "tableLayoutRmSsh";
            this.tableLayoutRmSsh.RowCount = 2;
            this.tableLayoutRmSsh.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmSsh.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutRmSsh.Size = new System.Drawing.Size(478, 104);
            this.tableLayoutRmSsh.TabIndex = 3;
            // 
            // labelRemarkableSsh
            // 
            this.labelRemarkableSsh.AutoSize = true;
            this.tableLayoutRmSsh.SetColumnSpan(this.labelRemarkableSsh, 3);
            this.labelRemarkableSsh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRemarkableSsh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRemarkableSsh.Location = new System.Drawing.Point(3, 0);
            this.labelRemarkableSsh.Name = "labelRemarkableSsh";
            this.labelRemarkableSsh.Size = new System.Drawing.Size(472, 52);
            this.labelRemarkableSsh.TabIndex = 0;
            this.labelRemarkableSsh.Text = "reMarkable SSH Setup";
            this.labelRemarkableSsh.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelSshPassword
            // 
            this.labelSshPassword.AutoSize = true;
            this.labelSshPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelSshPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.labelSshPassword.Location = new System.Drawing.Point(3, 52);
            this.labelSshPassword.Name = "labelSshPassword";
            this.labelSshPassword.Size = new System.Drawing.Size(113, 52);
            this.labelSshPassword.TabIndex = 1;
            this.labelSshPassword.Text = "Password";
            this.labelSshPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textSshPassword
            // 
            this.textSshPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textSshPassword.Location = new System.Drawing.Point(122, 68);
            this.textSshPassword.MaxLength = 20;
            this.textSshPassword.Name = "textSshPassword";
            this.textSshPassword.Size = new System.Drawing.Size(233, 20);
            this.textSshPassword.TabIndex = 2;
            this.textSshPassword.UseSystemPasswordChar = true;
            // 
            // btnRemarkableSshApply
            // 
            this.btnRemarkableSshApply.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRemarkableSshApply.Location = new System.Drawing.Point(373, 63);
            this.btnRemarkableSshApply.Name = "btnRemarkableSshApply";
            this.btnRemarkableSshApply.Size = new System.Drawing.Size(90, 30);
            this.btnRemarkableSshApply.TabIndex = 3;
            this.btnRemarkableSshApply.Text = "Apply";
            this.btnRemarkableSshApply.UseVisualStyleBackColor = true;
            this.btnRemarkableSshApply.Click += new System.EventHandler(this.btnRemarkableSshApply_Click);
            // 
            // labelRemarkableConnection
            // 
            this.labelRemarkableConnection.AutoSize = true;
            this.tableLayoutRmSelection.SetColumnSpan(this.labelRemarkableConnection, 2);
            this.labelRemarkableConnection.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRemarkableConnection.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRemarkableConnection.Location = new System.Drawing.Point(3, 0);
            this.labelRemarkableConnection.Name = "labelRemarkableConnection";
            this.labelRemarkableConnection.Size = new System.Drawing.Size(472, 38);
            this.labelRemarkableConnection.TabIndex = 0;
            this.labelRemarkableConnection.Text = "reMarkable Connection";
            this.labelRemarkableConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // radioButtonRmCloud
            // 
            this.radioButtonRmCloud.AutoSize = true;
            this.radioButtonRmCloud.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonRmCloud.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonRmCloud.Location = new System.Drawing.Point(30, 41);
            this.radioButtonRmCloud.Margin = new System.Windows.Forms.Padding(30, 3, 30, 3);
            this.radioButtonRmCloud.Name = "radioButtonRmCloud";
            this.radioButtonRmCloud.Size = new System.Drawing.Size(179, 32);
            this.radioButtonRmCloud.TabIndex = 1;
            this.radioButtonRmCloud.TabStop = true;
            this.radioButtonRmCloud.Text = "Use reMarkable Cloud";
            this.radioButtonRmCloud.UseVisualStyleBackColor = true;
            this.radioButtonRmCloud.CheckedChanged += new System.EventHandler(this.radioButtonRmCloud_CheckedChanged);
            // 
            // radioButtonRmSsh
            // 
            this.radioButtonRmSsh.AutoSize = true;
            this.radioButtonRmSsh.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radioButtonRmSsh.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonRmSsh.Location = new System.Drawing.Point(269, 41);
            this.radioButtonRmSsh.Margin = new System.Windows.Forms.Padding(30, 3, 30, 3);
            this.radioButtonRmSsh.Name = "radioButtonRmSsh";
            this.radioButtonRmSsh.Size = new System.Drawing.Size(179, 32);
            this.radioButtonRmSsh.TabIndex = 2;
            this.radioButtonRmSsh.TabStop = true;
            this.radioButtonRmSsh.Text = "Use USB SSH";
            this.radioButtonRmSsh.UseVisualStyleBackColor = true;
            this.radioButtonRmSsh.CheckedChanged += new System.EventHandler(this.radioButtonRmSsh_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.tableLayoutOverall);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.tableLayoutOverall.ResumeLayout(false);
            this.tableLayoutRmCloud.ResumeLayout(false);
            this.tableLayoutRmCloud.PerformLayout();
            this.tableLayoutMyscript.ResumeLayout(false);
            this.tableLayoutMyscript.PerformLayout();
            this.tableLayoutRmSelection.ResumeLayout(false);
            this.tableLayoutRmSelection.PerformLayout();
            this.tableLayoutRmSsh.ResumeLayout(false);
            this.tableLayoutRmSsh.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutOverall;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRmCloud;
        private System.Windows.Forms.Label labelRemarkableCloud;
        private System.Windows.Forms.TableLayoutPanel tableLayoutMyscript;
        private System.Windows.Forms.Label labelOTC;
        private System.Windows.Forms.Label labelMyscript;
        private System.Windows.Forms.Label labelAppKey;
        private System.Windows.Forms.Label labelHmacKey;
        private System.Windows.Forms.TextBox textOtc;
        private System.Windows.Forms.TextBox textAppKey;
        private System.Windows.Forms.TextBox textHmacKey;
        private System.Windows.Forms.Button btnRemarkableCloudApply;
        private System.Windows.Forms.Button btnMyScriptApply;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRmSelection;
        private System.Windows.Forms.TableLayoutPanel tableLayoutRmSsh;
        private System.Windows.Forms.Label labelRemarkableSsh;
        private System.Windows.Forms.Label labelSshPassword;
        private System.Windows.Forms.TextBox textSshPassword;
        private System.Windows.Forms.Button btnRemarkableSshApply;
        private System.Windows.Forms.Label labelRemarkableConnection;
        private System.Windows.Forms.RadioButton radioButtonRmCloud;
        private System.Windows.Forms.RadioButton radioButtonRmSsh;
    }
}