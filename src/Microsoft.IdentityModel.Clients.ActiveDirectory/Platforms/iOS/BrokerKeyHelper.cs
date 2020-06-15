//------------------------------------------------------------------------------
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
using System.IO;
using System.Security.Cryptography;
using Foundation;
using Security;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Helpers;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform
{
    static class BrokerKeyHelper
    {
        private const string LocalSettingsContainerName = "ActiveDirectoryAuthenticationLibrary";
        private const string KeyChainServiceBrokerKey = "Broker Key Service";
        private const string KeyChainAccountBrokerKey = "Broker Key Account";

        internal static byte[] GetOrCreateBrokerKey(ICoreLogger logger)
        {
            if (TryGetBrokerKey(out byte[] brokerKey))
            {
                logger.Info("Found existing broker key");
                return brokerKey;
            }

            brokerKey = CreateAndStoreBrokerKey(logger);

            return brokerKey;
        }

        private static byte[] CreateAndStoreBrokerKey(ICoreLogger logger)
        {
            logger.Info("Creating new broker key");

            byte[] brokerKey;
            byte[] rawBytes;
            using (AesManaged algo = CreateSymmetricAlgorith(null))
            {
                algo.GenerateKey();
                rawBytes = algo.Key;
            }

            NSData byteData = NSData.FromArray(rawBytes);

            var recordToAdd = new SecRecord(SecKind.GenericPassword)
            {
                Generic = NSData.FromString(LocalSettingsContainerName),
                Service = "Broker Key Service",
                Account = "Broker Key Account",
                Label = "Broker Key Label",
                Comment = "Broker Key Comment",
                Description = "Storage for broker key",
                ValueData = byteData
            };

            var result = SecKeyChain.Add(recordToAdd);
            
            if (result == SecStatusCode.DuplicateItem)
            {
                logger.Info("Could not add the broker key, a key already exists. Trying to delete it first.");
                var recordToRemove = new SecRecord(SecKind.GenericPassword)
                {
                    Service = "Broker Key Service",
                    Account = "Broker Key Account",
                };

                var removeResult = SecKeyChain.Remove(recordToRemove);
                logger.Info("Broker Key removal result: " + removeResult);

                result = SecKeyChain.Add(recordToAdd);
                logger.Info("Broker Key re-adding result: " + result);

            }

            if (result != SecStatusCode.Success)
            {
                throw new AdalException(
                    AdalError.BrokerKeySaveFailed,
                    AdalErrorMessage.BrokerKeySaveFailed(result.ToString()));
            }

            brokerKey = byteData.ToArray();
            return brokerKey;
        }

        private static bool TryGetBrokerKey(out byte[] brokerKey)
        {
            SecRecord record = new SecRecord(SecKind.GenericPassword)
            {
                Generic = NSData.FromString(LocalSettingsContainerName),
                Service = KeyChainServiceBrokerKey,
                Account = KeyChainAccountBrokerKey,
            };

            NSData key = SecKeyChain.QueryAsData(record);
            if (key != null)
            {
                brokerKey = key.ToArray();
                return true;
            }

            brokerKey = null;
            return false;
        }

        internal static string DecryptBrokerResponse(string encryptedBrokerResponse)
        {
            byte[] outputBytes = Base64UrlHelpers.DecodeToBytes(encryptedBrokerResponse);
            string plaintext = string.Empty;

            if (TryGetBrokerKey(out byte[] key))
            {
                AesManaged algo = null;
                CryptoStream cryptoStream = null;
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(outputBytes);
                    algo = CreateSymmetricAlgorith(key);
                    cryptoStream = new CryptoStream(
                        memoryStream,
                        algo.CreateDecryptor(),
                        CryptoStreamMode.Read);
                    using (StreamReader srDecrypt = new StreamReader(cryptoStream))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                        return plaintext;
                    }
                }
                finally
                {
                    memoryStream?.Dispose();
                    cryptoStream?.Dispose();
                    algo?.Dispose();
                }
            }

            throw new AdalException(
                "broker_key_fetch_failed",
                "Could not fetch the broker key after it was created. " +
                $"Try to delete the KeyChain entry service {KeyChainServiceBrokerKey} account {KeyChainAccountBrokerKey}");

        }

        private static AesManaged CreateSymmetricAlgorith(byte[] key)
        {
            AesManaged algorithm = new AesManaged
            {
                //set the mode, padding and block size
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128
            };

            if (key != null)
            {
                algorithm.Key = key;
            }

            algorithm.IV = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            return algorithm;
        }
    }
}
