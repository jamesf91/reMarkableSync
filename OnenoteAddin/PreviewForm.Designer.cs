namespace RemarkableSync.OnenoteAddin
{
    partial class PreviewForm
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
            this.components = new System.ComponentModel.Container();
            this.ilPreviews = new System.Windows.Forms.ImageList(this.components);
            this.lvPreviews = new System.Windows.Forms.ListView();
            this.btnOk = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ilPreviews
            // 
            this.ilPreviews.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.ilPreviews.ImageSize = new System.Drawing.Size(50, 50);
            this.ilPreviews.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lvPreviews
            // 
            this.lvPreviews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPreviews.HideSelection = false;
            this.lvPreviews.Location = new System.Drawing.Point(0, 0);
            this.lvPreviews.Name = "lvPreviews";
            this.lvPreviews.Size = new System.Drawing.Size(998, 726);
            this.lvPreviews.TabIndex = 0;
            this.lvPreviews.UseCompatibleStateImageBehavior = false;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.Location = new System.Drawing.Point(911, 691);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // PreviewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 726);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lvPreviews);
            this.Name = "PreviewForm";
            this.Text = "PreviewForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList ilPreviews;
        private System.Windows.Forms.ListView lvPreviews;
        private System.Windows.Forms.Button btnOk;
    }
}