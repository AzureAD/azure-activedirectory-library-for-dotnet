//----------------------------------------------------------------------
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

using Android.App;
using Android.OS;
using Android.Widget;
using Java.Lang;

namespace AdalAndroidTestApp
{
    /// <summary>
    /// Enables modifications to TextView from a non UI thread
    /// </summary>
    public class UITextView
    {
        private Activity activity;
        private TextView view;

        public UITextView(Activity activity, TextView view)
        {
            this.activity = activity;
            this.view = view;
        }

        /// <summary>
        /// Checks if set is made from a non UI thread and moves it to the UI thread if necessary
        /// </summary>
        public string Text
        {
            get
            {
                return view.Text;
            }
            set
            {
                if (Looper.MyLooper() != null && Looper.MyLooper().Thread == Looper.MainLooper.Thread)
                    view.Text = value;
                else
                    activity.RunOnUiThread(new Runnable(delegate () { view.Text = value; }));
            }
        }
    }
}