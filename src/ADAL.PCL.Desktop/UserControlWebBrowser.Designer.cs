namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{

    /// <summary>
    /// Extended web browser user control which has an address bar above its browser control
    /// </summary>
    partial class UserControlWebBrowser
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TextBoxAddressBar = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxAddressBar
            // 
            this.TextBoxAddressBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBoxAddressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.TextBoxAddressBar.Location = new System.Drawing.Point(0, 0);
            this.TextBoxAddressBar.Name = "textBoxAddressBar";
            this.TextBoxAddressBar.ReadOnly = true;
            this.TextBoxAddressBar.Size = new System.Drawing.Size(150, 20);
            this.TextBoxAddressBar.TabIndex = 0;
            // 
            // UserControlWebBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TextBoxAddressBar);
            this.Name = "UserControlWebBrowser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>
        /// Address bar text box
        /// </summary>
        public System.Windows.Forms.TextBox TextBoxAddressBar;
    }
}
