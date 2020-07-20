using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ICExHotmailCaptchaGenerator
{
    public partial class startProcess : Form
    {
        Uri mainURL = new Uri("https://eus.client.hip.live.com/GetHIP/GetHIPAMFE/HIPAMFE?dc=EUS&mkt=tr-TR&id=15041&fid=d5b9f8d61eea4c1ca206631bf391aa3c&type=visual&c=14&rnd=0.007418881013300238");
        string folderPath;
        string escapedURL;
        string unescapedURL;
        bool success = false;
        bool nextones=true;
        int counter;
        public startProcess()
        {
            InitializeComponent();
        }
        public void Downloader(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    escapedURL = Between(reader.ReadToEnd(), "\"hipChallengeUrl\":\"", "\"");
                    unescapedURL = Regex.Unescape(escapedURL);
                    success = true;
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                        wc.DownloadFileAsync(new Uri(unescapedURL), folderPath + "/" + "ICEx_" + DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss") + ".png");
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void Get(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    escapedURL = Between(reader.ReadToEnd(), "\"hipChallengeUrl\":\"", "\"");
                    unescapedURL= Regex.Unescape(escapedURL);
                    Invoke(new Action(() =>
                    {
                        richTextBox1.Text = unescapedURL;
                    }));
                    pictureBox1.LoadAsync(unescapedURL);
                    success = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            
        }
        public string Between(string str, string firstString, string lastString)
        {
            int pos1 = str.IndexOf(firstString) + firstString.Length;
            int pos2 = str.Substring(pos1).IndexOf(lastString);
            return str.Substring(pos1, pos2);
        }
        private void getCaptcha_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => { Get(mainURL.ToString()); });
        }
        private void folder_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog Folder = new FolderBrowserDialog();
            Folder.ShowDialog();
            folderPath = Folder.SelectedPath;
            folderPathShowBox.Visible = true;
            folderPathShowBox.Text = folderPath;
        }
        //private void downloadFile()
        //{
        //    for (int i = 0; i < getCounter.Value; i++)
        //    {
        //        using (WebClient wc = new WebClient())
        //        {
        //            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
        //            wc.DownloadFileAsync(new Uri(unescapedURL), folderPath + "/" + "ICEx_" + DateTime.Now.ToString("yyyy-dd-M-HH-mm-ss")+".png");
        //        }
        //    }
        //}
        //private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        //{
        //    progressBar1.Value = e.ProgressPercentage;
        //}
        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            nextones = true;
            counter++;
            Invoke(new Action(() =>
            {
                progressBar1.Value++;
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Maximum =Convert.ToInt32(getCounter.Value)+1;
            if (success)
            {
                if (folderPath != null)
                {
                    Task t =Task.Factory.StartNew(() =>
                    {
                        do
                        {
                            if (nextones)
                            {
                                nextones = false;
                                Downloader(mainURL.ToString());
                            }

                        }
                        while (counter!=getCounter.Value);
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show("Completed!");
                            counter = 0;
                            progressBar1.Value = 0;
                        }));
                    });
                }
            }
            else
                MessageBox.Show("please press the \"Get One CAPTCHA \" button for the first test.");
        }
    }
}
