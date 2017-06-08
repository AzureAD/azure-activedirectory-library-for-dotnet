using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;

    public  partial class UserControlWebBrowser : UserControl
    {
        internal  CustomWebBrowser customWebBrowser=new CustomWebBrowser();

        /// <summary>
        /// UserControlWebBrowser Constructor 
        /// </summary>
        public UserControlWebBrowser(bool displayAddressBar)
        {
            InitializeComponent();
            customWebBrowser.Dock =DockStyle.Fill;
            this.Controls.Add(this.customWebBrowser);
            if (displayAddressBar)
            {
                this.TextBoxAddressBar.Height = 30;
                this.TextBoxAddressBar.BringToFront();
                this.TextBoxAddressBar.Visible =true;
            }
            else
            {
                this.TextBoxAddressBar.Visible = false;
            }
        }
    }
}
