namespace WinFormsAutomationApp
{
    partial class MainForm
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
            this.pageControl1 = new WinFormsAutomationApp.PageControl();
            this.mainPage = new System.Windows.Forms.TabPage();
            this.clearCache = new System.Windows.Forms.Button();
            this.readCache = new System.Windows.Forms.Button();
            this.invalidateRefreshToken = new System.Windows.Forms.Button();
            this.expireAccessToken = new System.Windows.Forms.Button();
            this.acquireTokenSilent = new System.Windows.Forms.Button();
            this.acquireToken = new System.Windows.Forms.Button();
            this.dataInputPage = new System.Windows.Forms.TabPage();
            this.go = new System.Windows.Forms.Button();
            this.dataInput = new System.Windows.Forms.TextBox();
            this.resultPage = new System.Windows.Forms.TabPage();
            this.done = new System.Windows.Forms.Button();
            this.result = new System.Windows.Forms.TextBox();
            this.pageControl1.SuspendLayout();
            this.mainPage.SuspendLayout();
            this.dataInputPage.SuspendLayout();
            this.resultPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // pageControl1
            // 
            this.pageControl1.Controls.Add(this.mainPage);
            this.pageControl1.Controls.Add(this.dataInputPage);
            this.pageControl1.Controls.Add(this.resultPage);
            this.pageControl1.Location = new System.Drawing.Point(-2, 0);
            this.pageControl1.Name = "pageControl1";
            this.pageControl1.SelectedIndex = 0;
            this.pageControl1.Size = new System.Drawing.Size(599, 573);
            this.pageControl1.TabIndex = 0;
            // 
            // mainPage
            // 
            this.mainPage.Controls.Add(this.clearCache);
            this.mainPage.Controls.Add(this.readCache);
            this.mainPage.Controls.Add(this.invalidateRefreshToken);
            this.mainPage.Controls.Add(this.expireAccessToken);
            this.mainPage.Controls.Add(this.acquireTokenSilent);
            this.mainPage.Controls.Add(this.acquireToken);
            this.mainPage.Location = new System.Drawing.Point(4, 22);
            this.mainPage.Name = "mainPage";
            this.mainPage.Padding = new System.Windows.Forms.Padding(3);
            this.mainPage.Size = new System.Drawing.Size(591, 547);
            this.mainPage.TabIndex = 0;
            this.mainPage.Text = "mainPage";
            this.mainPage.UseVisualStyleBackColor = true;
            // 
            // clearCache
            // 
            this.clearCache.AccessibleName = "clearCache";
            this.clearCache.Location = new System.Drawing.Point(175, 323);
            this.clearCache.Name = "clearCache";
            this.clearCache.Size = new System.Drawing.Size(233, 43);
            this.clearCache.TabIndex = 6;
            this.clearCache.Text = "Clear Cache";
            this.clearCache.UseVisualStyleBackColor = true;
            // 
            // readCache
            // 
            this.readCache.AccessibleName = "readCache";
            this.readCache.Location = new System.Drawing.Point(175, 263);
            this.readCache.Name = "readCache";
            this.readCache.Size = new System.Drawing.Size(233, 43);
            this.readCache.TabIndex = 5;
            this.readCache.Text = "Read Cache";
            this.readCache.UseVisualStyleBackColor = true;
            // 
            // invalidateRefreshToken
            // 
            this.invalidateRefreshToken.AccessibleName = "invalidateRefreshToken";
            this.invalidateRefreshToken.Location = new System.Drawing.Point(175, 199);
            this.invalidateRefreshToken.Name = "invalidateRefreshToken";
            this.invalidateRefreshToken.Size = new System.Drawing.Size(233, 43);
            this.invalidateRefreshToken.TabIndex = 4;
            this.invalidateRefreshToken.Text = "Invalidate Refresh Token";
            this.invalidateRefreshToken.UseVisualStyleBackColor = true;
            // 
            // expireAccessToken
            // 
            this.expireAccessToken.AccessibleName = "expireAccessToken";
            this.expireAccessToken.Location = new System.Drawing.Point(175, 139);
            this.expireAccessToken.Name = "expireAccessToken";
            this.expireAccessToken.Size = new System.Drawing.Size(233, 43);
            this.expireAccessToken.TabIndex = 3;
            this.expireAccessToken.Text = "Expire Access Token";
            this.expireAccessToken.UseVisualStyleBackColor = true;
            this.expireAccessToken.Click += new System.EventHandler(this.expireAccessToken_Click);
            // 
            // acquireTokenSilent
            // 
            this.acquireTokenSilent.AccessibleName = "acquireTokenSilent";
            this.acquireTokenSilent.Location = new System.Drawing.Point(175, 79);
            this.acquireTokenSilent.Name = "acquireTokenSilent";
            this.acquireTokenSilent.Size = new System.Drawing.Size(233, 43);
            this.acquireTokenSilent.TabIndex = 2;
            this.acquireTokenSilent.Text = "Acquire Token Silent";
            this.acquireTokenSilent.UseVisualStyleBackColor = true;
            this.acquireTokenSilent.Click += new System.EventHandler(this.acquireTokenSilent_Click);
            // 
            // acquireToken
            // 
            this.acquireToken.AccessibleName = "acquireToken";
            this.acquireToken.Location = new System.Drawing.Point(175, 20);
            this.acquireToken.Name = "acquireToken";
            this.acquireToken.Size = new System.Drawing.Size(233, 43);
            this.acquireToken.TabIndex = 1;
            this.acquireToken.Text = "Acquire Token";
            this.acquireToken.UseVisualStyleBackColor = true;
            this.acquireToken.Click += new System.EventHandler(this.acquireToken_Click);
            // 
            // dataInputPage
            // 
            this.dataInputPage.Controls.Add(this.go);
            this.dataInputPage.Controls.Add(this.dataInput);
            this.dataInputPage.Location = new System.Drawing.Point(4, 22);
            this.dataInputPage.Name = "dataInputPage";
            this.dataInputPage.Padding = new System.Windows.Forms.Padding(3);
            this.dataInputPage.Size = new System.Drawing.Size(591, 547);
            this.dataInputPage.TabIndex = 1;
            this.dataInputPage.Text = "dataInputPage";
            this.dataInputPage.UseVisualStyleBackColor = true;
            // 
            // go
            // 
            this.go.AccessibleName = "go";
            this.go.Location = new System.Drawing.Point(36, 439);
            this.go.Name = "go";
            this.go.Size = new System.Drawing.Size(515, 41);
            this.go.TabIndex = 1;
            this.go.Text = "GO!";
            this.go.UseVisualStyleBackColor = true;
            this.go.Click += new System.EventHandler(this.go_Click);
            // 
            // dataInput
            // 
            this.dataInput.AccessibleName = "dataInput";
            this.dataInput.Location = new System.Drawing.Point(36, 23);
            this.dataInput.Multiline = true;
            this.dataInput.Name = "dataInput";
            this.dataInput.Size = new System.Drawing.Size(515, 410);
            this.dataInput.TabIndex = 0;
            // 
            // resultPage
            // 
            this.resultPage.Controls.Add(this.done);
            this.resultPage.Controls.Add(this.result);
            this.resultPage.Location = new System.Drawing.Point(4, 22);
            this.resultPage.Name = "resultPage";
            this.resultPage.Padding = new System.Windows.Forms.Padding(3);
            this.resultPage.Size = new System.Drawing.Size(591, 547);
            this.resultPage.TabIndex = 2;
            this.resultPage.Text = "resultPage";
            this.resultPage.UseVisualStyleBackColor = true;
            // 
            // done
            // 
            this.done.AccessibleName = "done";
            this.done.Location = new System.Drawing.Point(28, 494);
            this.done.Name = "done";
            this.done.Size = new System.Drawing.Size(536, 32);
            this.done.TabIndex = 1;
            this.done.Text = "Done";
            this.done.UseVisualStyleBackColor = true;
            this.done.Click += new System.EventHandler(this.done_Click);
            // 
            // result
            // 
            this.result.AccessibleName = "result";
            this.result.Enabled = false;
            this.result.Location = new System.Drawing.Point(28, 16);
            this.result.Multiline = true;
            this.result.Name = "result";
            this.result.Size = new System.Drawing.Size(536, 460);
            this.result.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 571);
            this.Controls.Add(this.pageControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.pageControl1.ResumeLayout(false);
            this.mainPage.ResumeLayout(false);
            this.dataInputPage.ResumeLayout(false);
            this.dataInputPage.PerformLayout();
            this.resultPage.ResumeLayout(false);
            this.resultPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private PageControl pageControl1;
        private System.Windows.Forms.TabPage mainPage;
        private System.Windows.Forms.TabPage dataInputPage;
        private System.Windows.Forms.TabPage resultPage;
        private System.Windows.Forms.Button acquireToken;
        private System.Windows.Forms.Button go;
        private System.Windows.Forms.TextBox dataInput;
        private System.Windows.Forms.Button done;
        private System.Windows.Forms.TextBox result;
        private System.Windows.Forms.Button acquireTokenSilent;
        private System.Windows.Forms.Button invalidateRefreshToken;
        private System.Windows.Forms.Button expireAccessToken;
        private System.Windows.Forms.Button clearCache;
        private System.Windows.Forms.Button readCache;
    }
}

