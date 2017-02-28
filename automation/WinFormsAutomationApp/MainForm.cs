﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace WinFormsAutomationApp
{
    public partial class MainForm : Form
    {
        private delegate Task<string> Command(Dictionary<string, string> input);

        private Command _commandToRun = null;

        public MainForm()
        {
            InitializeComponent();
            TokenCache.DefaultShared.AfterAccess += TokenCacheDelegates.AfterAccessNotification;
            TokenCache.DefaultShared.BeforeAccess += TokenCacheDelegates.BeforeAccessNotification;
        }

        private void acquireToken_Click(object sender, EventArgs e)
        {
            _commandToRun = AuthenticationHelper.AcquireToken;
            pageControl1.SelectedTab = dataInputPage;
            
        }

        private async void requestGo_Click(object sender, EventArgs e)
        {
            string output = await _commandToRun((AuthenticationHelper.CreateDictionaryFromJson(requestInfo.Text)));
            pageControl1.SelectedTab = resultPage;
            resultInfo.Text = output;
            adalLogs.Text = AdalLoggerCallBack.Message;
        }

        private void resultDone_Click(object sender, EventArgs e)
        {
            resultInfo.Text = string.Empty;
            requestInfo.Text = string.Empty;
            pageControl1.SelectedTab = mainPage;
        }

        private void acquireTokenSilent_Click(object sender, EventArgs e)  
        {
            _commandToRun = AuthenticationHelper.AcquireTokenSilent;
            pageControl1.SelectedTab = dataInputPage;
        }

        private void expireAccessToken_Click(object sender, EventArgs e)
        {
            _commandToRun = AuthenticationHelper.ExpireAccessToken;
            pageControl1.SelectedTab = dataInputPage;
        }

        private void invalidateRefreshToken_Click(object sender, EventArgs e)
        {
            _commandToRun = AuthenticationHelper.InvalidateRefreshTokens;
            pageControl1.SelectedTab = dataInputPage;
        }

        private void readCache_Click(object sender, EventArgs e)
        {
            _commandToRun = AuthenticationHelper.ReadCache;
            pageControl1.SelectedTab = dataInputPage;
        }

        private async void clearCache_Click(object sender, EventArgs e)
        {
            string output = await AuthenticationHelper.ClearCache();
            pageControl1.SelectedTab = resultPage;
            resultInfo.Text = output;
            adalLogs.Text = AdalLoggerCallBack.Message;
        }

        private void acquireTokenDeviceProfile_Click(object sender, EventArgs e)
        {

        }
    }
}
