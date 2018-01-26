﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XForms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogPage : ContentPage
    {
        private static readonly StringBuilder Sb = new StringBuilder();
        private static readonly StringBuilder SbPii = new StringBuilder();
        private static readonly object BufferLock = new object();
        private static readonly object BufferLockPii = new object();


        public LogPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            ShowLog();
        }

        private void ShowLog()
        {
            lock (BufferLock)
            {
                log.Text = Sb.ToString();
            }
            lock (BufferLockPii)
            {
                logPii.Text = SbPii.ToString();
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            lock (BufferLock)
            {
                Sb.Clear();
            }
            lock (BufferLockPii)
            {
                SbPii.Clear();
            }
            ShowLog();
        }

        public static void AddToLog(string str, bool containsPii)
        {
            if (containsPii)
                lock (BufferLockPii)
                {
                    SbPii.AppendLine(str);
                }
            else
                lock (BufferLock)
                {
                    Sb.AppendLine(str);
                }
        }
    }
}
