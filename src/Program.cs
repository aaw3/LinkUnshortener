using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinForm = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace LinkUnshortener
{
    class Program
    {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [STAThread]
        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);

            if (!Clipboard.ContainsText())
                return;

            ParseClipboard(Clipboard.GetText());
        }

        public static void ParseClipboard(string uriData)
        {
            Uri uri;

            if (Uri.IsWellFormedUriString(uriData, UriKind.Absolute))
            {
                uri = new Uri(uriData);
                if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
                {
                    var returnURL = GetResponseURL(uri.AbsoluteUri);
                    WinForm.MessageBox.Show($"Redirected To: {returnURL}", "Link Unshortener", WinForm.MessageBoxButtons.OK, WinForm.MessageBoxIcon.Information);
                }
            }
            else if (Regex.IsMatch(uriData, @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$", RegexOptions.IgnoreCase))
            {
                var dResultHTTP = WinForm.MessageBox.Show($"URI Scheme Missing!\nRetry using HTTP Scheme?\n\"{uriData}\"", "Link Unshortener", WinForm.MessageBoxButtons.YesNo, WinForm.MessageBoxIcon.Question);

                if (dResultHTTP == WinForm.DialogResult.No)
                    return;

                   string returnHTTP = GetResponseURL("http://" + uriData);

                if (returnHTTP == null)
                {
                    var dResultHTTPS = WinForm.MessageBox.Show($"HTTP Response was null!\nRetry using HTTPS Scheme?\n\"{uriData}\"", "Link Unshortener", WinForm.MessageBoxButtons.YesNo, WinForm.MessageBoxIcon.Question);

                    if (dResultHTTPS == WinForm.DialogResult.No)
                        return;
                    else
                        WinForm.MessageBox.Show($"Redirected To: {returnHTTP}", "Link Unshortener", WinForm.MessageBoxButtons.OK, WinForm.MessageBoxIcon.Information);

                    string returnHTTPS = GetResponseURL("https://" + uriData);

                    if (returnHTTPS == null)
                        WinForm.MessageBox.Show($"HTTPS Response was null!\nApplication Exiting...", "Link Unshortener", WinForm.MessageBoxButtons.YesNo, WinForm.MessageBoxIcon.Information);
                    else
                        WinForm.MessageBox.Show($"Redirected To: {returnHTTPS}", "Link Unshortener", WinForm.MessageBoxButtons.OK, WinForm.MessageBoxIcon.Information);
                }


            } else
                WinForm.MessageBox.Show($"We couldn't unshorten this data: \"{uriData}\"", "Link Unshortener", WinForm.MessageBoxButtons.OK, WinForm.MessageBoxIcon.Information);
        }

        public static string GetResponseURL(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.AllowAutoRedirect = false;

            webRequest.Timeout = 10000;
            webRequest.Method = "HEAD";

            HttpWebResponse webResponse;
            using (webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                string uriString = webResponse.Headers["Location"];
                webResponse.Close();
                return uriString;
            }

        }
    }
}
