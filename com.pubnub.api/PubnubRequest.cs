using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace com.pubnub.api
{
    internal sealed class PubnubRequest
    {
        internal static string BuildUrl(bool enableSsl, params string[] url)
        {
            var sb = new StringBuilder();

            // Add Origin To The Request
            sb.Append((enableSsl ? "https://" : "http://") + OriginHostname);

            // Generate URL with UTF-8 Encoding
            foreach (string part in url)
            {
                sb.Append("/");
                sb.Append(_encodeURIcomponent(part));
            }

            // Fail if string too long
            if (sb.Length > RequestLengthLimit)
                throw new Exception("Constructed URL is too long, Request Length Limit has been exceeded.");

            return sb.ToString();
        }

        internal static string GetSignature(string publishKey, string subscribeKey, string secretKey, string channel, string json)
        {
            string signature = "0";
            if (secretKey.Length > 0)
            {
                StringBuilder string_to_sign = new StringBuilder();
                string_to_sign
                    .Append(publishKey)
                    .Append('/')
                    .Append(subscribeKey)
                    .Append('/')
                    .Append(secretKey)
                    .Append('/')
                    .Append(channel)
                    .Append('/')
                    .Append(json); // 1

                // Sign Message
                signature = md5(string_to_sign.ToString());
            }
            return signature;
        }

        private static string _encodeURIcomponent(string s)
        {
            StringBuilder o = new StringBuilder();
            foreach (char ch in s.ToCharArray())
            {
                if (isUnsafe(ch))
                {
                    o.Append('%');
                    o.Append(toHex(ch / 16));
                    o.Append(toHex(ch % 16));
                }
                else o.Append(ch);
            }
            return o.ToString();
        }

        private static char toHex(int ch)
        {
            return (char)(ch < 10 ? '0' + ch : 'A' + ch - 10);
        }

        private static bool isUnsafe(char ch)
        {
            return " ~`!@#$%^&*()+=[]\\{}|;':\",./<>?".IndexOf(ch) >= 0;
        }

        private static string md5(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.Unicode.GetBytes(text);
            byte[] hash = md5.ComputeHash(data);
            string hexaHash = "";
            foreach (byte b in hash) hexaHash += String.Format("{0:x2}", b);
            return hexaHash;
        }

        private const string OriginHostname = "pubsub.pubnub.com";

        /// <summary>
        /// All requests to Pubnub are via the GET URI and hence need to be capped at
        /// this length.
        /// </summary>
        private const int RequestLengthLimit = 1800;

        /// <summary>
        /// Timeout the request after 4 minutes
        /// </summary>
        private const int RequestTimeout = 1000 * 60 * 4;

        /// <summary>
        /// If we are behind a proxy server this object is setup
        /// </summary>
        internal IWebProxy Proxy
        {
            get;
            set;
        }

        internal HttpStatusCode Execute(string url, out string json)
        {
            // Default empty Json
            json = "[]";
            string Data = json;
            HttpStatusCode Status = HttpStatusCode.Unused;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (Proxy != null)
                request.Proxy = Proxy;

            request.ServicePoint.ConnectionLimit = 100;
            request.Timeout = RequestTimeout;

            using (var handle = new ManualResetEvent(false))
            {
                request.BeginGetResponse(ar =>
                {
                    try
                    {
                        var response = (HttpWebResponse)request.EndGetResponse(ar);

                        using (var receiveStream = response.GetResponseStream())
                        using (var readStream = new StreamReader(receiveStream, Encoding.ASCII))
                        {
                            Data = readStream.ReadToEnd();
                        }

                        Status = response.StatusCode;
                    }
                    catch (Exception exp)
                    {
                        System.Diagnostics.Debug.WriteLine("PubnubRequest.Execute: " + exp.Message);
                        Console.WriteLine("PubnubRequest.Execute: " + exp.Message);
                    }
                    finally
                    {
                        if (handle != null)
                            handle.Set();
                    }

                }, new object() /* state */);

                // In case the first timeout doesn't work then move onto then
                // we'll let the wait request handle timeout as well
                handle.WaitOne(RequestTimeout + (RequestTimeout / 10));
            }
            json = Data;
            return Status;
        }
    }

}
