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

using System.Text;

namespace Microsoft.Identity.Core.Helpers
{
    internal static class StringBuilderExtensions
    {
        /// <summary>
        /// Create an array of bytes representing the UTF-8 encoding of the current string value of
        /// the given <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="stringBuilder"><see cref="StringBuilder"/> to get the UTF-8 bytes for</param>
        /// <returns>Array of UTF-8 character bytes</returns>
        public static byte[] ToByteArray(this StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                return null;
            }

            UTF8Encoding encoding = new UTF8Encoding();
            var messageChars = new char[stringBuilder.Length];

            try
            {
                stringBuilder.CopyTo(0, messageChars, 0, stringBuilder.Length);
                return encoding.GetBytes(messageChars);
            }
            finally
            {
                messageChars.SecureClear();
            }
        }

        public static void SecureClear(this StringBuilder stringBuilder)
        {
            if (stringBuilder != null)
            {
                for (int i = 0; i < stringBuilder.Length; i++)
                {
                    stringBuilder[i] = '\0';
                }

                stringBuilder.Length = 0;
            }
        }
    }
}
