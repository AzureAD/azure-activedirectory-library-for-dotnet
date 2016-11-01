using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutomationApp;

namespace WinFormsAutomationApp
{
    public partial class MainForm : Form
    {
        private delegate Task<string> Command(Dictionary<string, string> input);

        private Command _commandToRun = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void acquireToken_Click(object sender, EventArgs e)
        {
            _commandToRun = AuthenticationHelper.AcquireToken;
            pageControl1.SelectedTab = dataInputPage;
        }

        private async void go_Click(object sender, EventArgs e)
        {
            string output = await _commandToRun((AuthenticationHelper.CreateDictionaryFromJson(dataInput.Text)));
            pageControl1.SelectedTab = resultPage;
            result.Text = output;
        }

        private void done_Click(object sender, EventArgs e)
        {
            result.Text = string.Empty;
            dataInput.Text = string.Empty;
            pageControl1.SelectedTab = mainPage;
        }

        private void acquireTokenSilent_Click(object sender, EventArgs e)
        {
            _commandToRun = AuthenticationHelper.AcquireTokenSilent;
            pageControl1.SelectedTab = dataInputPage;
        }
    }
}
