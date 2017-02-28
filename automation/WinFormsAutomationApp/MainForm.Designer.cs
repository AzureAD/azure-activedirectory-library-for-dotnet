﻿namespace WinFormsAutomationApp
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
            this.adalLogs = new System.Windows.Forms.TextBox();
            this.pageControl1 = new WinFormsAutomationApp.PageControl();
            this.mainPage = new System.Windows.Forms.TabPage();
            this.acquireTokenDeviceProfile = new System.Windows.Forms.Button();
            this.clearCache = new System.Windows.Forms.Button();
            this.readCache = new System.Windows.Forms.Button();
            this.invalidateRefreshToken = new System.Windows.Forms.Button();
            this.expireAccessToken = new System.Windows.Forms.Button();
            this.acquireTokenSilent = new System.Windows.Forms.Button();
            this.acquireToken = new System.Windows.Forms.Button();
            this.dataInputPage = new System.Windows.Forms.TabPage();
            this.requestGo = new System.Windows.Forms.Button();
            this.requestInfo = new System.Windows.Forms.TextBox();
            this.resultPage = new System.Windows.Forms.TabPage();
            this.resultDone = new System.Windows.Forms.Button();
            this.resultInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pageControl1.SuspendLayout();
            this.mainPage.SuspendLayout();
            this.dataInputPage.SuspendLayout();
            this.resultPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // adalLogs
            // 
            this.adalLogs.AccessibleName = "adalLogs";
            this.adalLogs.Location = new System.Drawing.Point(-2, 582);
            this.adalLogs.Multiline = true;
            this.adalLogs.Name = "adalLogs";
            this.adalLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.adalLogs.Size = new System.Drawing.Size(575, 168);
            this.adalLogs.TabIndex = 9;
            // 
            // pageControl1
            // 
            this.pageControl1.Controls.Add(this.mainPage);
            this.pageControl1.Controls.Add(this.dataInputPage);
            this.pageControl1.Controls.Add(this.resultPage);
            this.pageControl1.Location = new System.Drawing.Point(-2, 0);
            this.pageControl1.Name = "pageControl1";
            this.pageControl1.SelectedIndex = 0;
            this.pageControl1.Size = new System.Drawing.Size(571, 537);
            this.pageControl1.TabIndex = 0;
            // 
            // mainPage
            // 
            this.mainPage.Controls.Add(this.acquireTokenDeviceProfile);
            this.mainPage.Controls.Add(this.clearCache);
            this.mainPage.Controls.Add(this.readCache);
            this.mainPage.Controls.Add(this.invalidateRefreshToken);
            this.mainPage.Controls.Add(this.expireAccessToken);
            this.mainPage.Controls.Add(this.acquireTokenSilent);
            this.mainPage.Controls.Add(this.acquireToken);
            this.mainPage.Location = new System.Drawing.Point(4, 22);
            this.mainPage.Name = "mainPage";
            this.mainPage.Padding = new System.Windows.Forms.Padding(3);
            this.mainPage.Size = new System.Drawing.Size(563, 511);
            this.mainPage.TabIndex = 0;
            this.mainPage.Text = "mainPage";
            this.mainPage.UseVisualStyleBackColor = true;
            // 
            // acquireTokenDeviceProfile
            // 
            this.acquireTokenDeviceProfile.AccessibleName = "acquireTokenDeviceProfile";
            this.acquireTokenDeviceProfile.Location = new System.Drawing.Point(175, 140);
            this.acquireTokenDeviceProfile.Name = "acquireTokenDeviceProfile";
            this.acquireTokenDeviceProfile.Size = new System.Drawing.Size(233, 43);
            this.acquireTokenDeviceProfile.TabIndex = 7;
            this.acquireTokenDeviceProfile.Text = "Acquire Token Using Device Profile Flow";
            this.acquireTokenDeviceProfile.UseVisualStyleBackColor = true;
            this.acquireTokenDeviceProfile.Click += new System.EventHandler(this.acquireTokenDeviceProfile_Click);
            // 
            // clearCache
            // 
            this.clearCache.AccessibleName = "clearCache";
            this.clearCache.Location = new System.Drawing.Point(175, 385);
            this.clearCache.Name = "clearCache";
            this.clearCache.Size = new System.Drawing.Size(233, 43);
            this.clearCache.TabIndex = 6;
            this.clearCache.Text = "Clear Cache";
            this.clearCache.UseVisualStyleBackColor = true;
            this.clearCache.Click += new System.EventHandler(this.clearCache_Click);
            // 
            // readCache
            // 
            this.readCache.AccessibleName = "readCache";
            this.readCache.Location = new System.Drawing.Point(175, 325);
            this.readCache.Name = "readCache";
            this.readCache.Size = new System.Drawing.Size(233, 43);
            this.readCache.TabIndex = 5;
            this.readCache.Text = "Read Cache";
            this.readCache.UseVisualStyleBackColor = true;
            this.readCache.Click += new System.EventHandler(this.readCache_Click);
            // 
            // invalidateRefreshToken
            // 
            this.invalidateRefreshToken.AccessibleName = "invalidateRefreshToken";
            this.invalidateRefreshToken.Location = new System.Drawing.Point(175, 261);
            this.invalidateRefreshToken.Name = "invalidateRefreshToken";
            this.invalidateRefreshToken.Size = new System.Drawing.Size(233, 43);
            this.invalidateRefreshToken.TabIndex = 4;
            this.invalidateRefreshToken.Text = "Invalidate Refresh Token";
            this.invalidateRefreshToken.UseVisualStyleBackColor = true;
            this.invalidateRefreshToken.Click += new System.EventHandler(this.invalidateRefreshToken_Click);
            // 
            // expireAccessToken
            // 
            this.expireAccessToken.AccessibleName = "expireAccessToken";
            this.expireAccessToken.Location = new System.Drawing.Point(175, 201);
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
            this.dataInputPage.Controls.Add(this.requestGo);
            this.dataInputPage.Controls.Add(this.requestInfo);
            this.dataInputPage.Location = new System.Drawing.Point(4, 22);
            this.dataInputPage.Name = "dataInputPage";
            this.dataInputPage.Padding = new System.Windows.Forms.Padding(3);
            this.dataInputPage.Size = new System.Drawing.Size(563, 511);
            this.dataInputPage.TabIndex = 1;
            this.dataInputPage.Text = "dataInputPage";
            this.dataInputPage.UseVisualStyleBackColor = true;
            // 
            // requestGo
            // 
            this.requestGo.AccessibleName = "requestGo";
            this.requestGo.Location = new System.Drawing.Point(36, 439);
            this.requestGo.Name = "requestGo";
            this.requestGo.Size = new System.Drawing.Size(515, 41);
            this.requestGo.TabIndex = 1;
            this.requestGo.Text = "GO!";
            this.requestGo.UseVisualStyleBackColor = true;
            this.requestGo.Click += new System.EventHandler(this.requestGo_Click);
            // 
            // requestInfo
            // 
            this.requestInfo.AccessibleName = "requestInfo";
            this.requestInfo.Location = new System.Drawing.Point(36, 23);
            this.requestInfo.Multiline = true;
            this.requestInfo.Name = "requestInfo";
            this.requestInfo.Size = new System.Drawing.Size(515, 410);
            this.requestInfo.TabIndex = 0;
            // 
            // resultPage
            // 
            this.resultPage.Controls.Add(this.resultDone);
            this.resultPage.Controls.Add(this.resultInfo);
            this.resultPage.Location = new System.Drawing.Point(4, 22);
            this.resultPage.Name = "resultPage";
            this.resultPage.Padding = new System.Windows.Forms.Padding(3);
            this.resultPage.Size = new System.Drawing.Size(563, 511);
            this.resultPage.TabIndex = 2;
            this.resultPage.Text = "resultPage";
            this.resultPage.UseVisualStyleBackColor = true;
            // 
            // resultDone
            // 
            this.resultDone.AccessibleName = "resultDone";
            this.resultDone.Location = new System.Drawing.Point(10, 468);
            this.resultDone.Name = "resultDone";
            this.resultDone.Size = new System.Drawing.Size(536, 32);
            this.resultDone.TabIndex = 1;
            this.resultDone.Text = "Done";
            this.resultDone.UseVisualStyleBackColor = true;
            this.resultDone.Click += new System.EventHandler(this.resultDone_Click);
            // 
            // resultInfo
            // 
            this.resultInfo.AccessibleName = "resultInfo";
            this.resultInfo.Enabled = false;
            this.resultInfo.Location = new System.Drawing.Point(10, 16);
            this.resultInfo.Multiline = true;
            this.resultInfo.Name = "resultInfo";
            this.resultInfo.Size = new System.Drawing.Size(536, 437);
            this.resultInfo.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 563);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "ADAL Logs:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 774);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.adalLogs);
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
            this.PerformLayout();

        }

        #endregion

        private PageControl pageControl1;
        private System.Windows.Forms.TabPage mainPage;
        private System.Windows.Forms.TabPage dataInputPage;
        private System.Windows.Forms.TabPage resultPage;
        private System.Windows.Forms.Button acquireToken;
        private System.Windows.Forms.Button requestGo;
        private System.Windows.Forms.TextBox requestInfo;
        private System.Windows.Forms.Button resultDone;
        private System.Windows.Forms.TextBox resultInfo;
        private System.Windows.Forms.Button acquireTokenSilent;
        private System.Windows.Forms.Button invalidateRefreshToken;
        private System.Windows.Forms.Button expireAccessToken;
        private System.Windows.Forms.Button clearCache;
        private System.Windows.Forms.Button readCache;
        private System.Windows.Forms.Button acquireTokenDeviceProfile;
        private System.Windows.Forms.TextBox adalLogs;
        private System.Windows.Forms.Label label1;
    }
}

