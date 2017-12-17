using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace YoutubeSubtitlesScraper
{
    public partial class Form1 : Form
    {
        List<Subtitle> subs = new List<Subtitle>();
        string videoId;
        int id;

        public Form1()
        {
            InitializeComponent();
        }

        private void bunifuImageButton3_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com");
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            subs.Clear();
            Uri youtubeVideoUri = CreateValidUri();
            try
            {
                videoId = Regex.Match(youtubeVideoUri.AbsoluteUri, $"(?:watch\\?v=)([^\\s]+)").Groups[1].Value;
                string availableSubsDoc =
                    await GetAvailableSubsDocument("http://video.google.com/timedtext?type=list&v=" + videoId);
                XDocument availableSubsXDocument = XDocument.Parse(availableSubsDoc);
                if (!RootHasChilds(availableSubsXDocument))
                {
                    MessageBox.Show("Нет доступных субтитров для этого видео.", "Оповещение!");
                    throw new Exception("Пустой корень документа");
                }

                foreach (var subtitle in availableSubsXDocument.Root.Elements())
                {
                    subs.Add(new Subtitle()
                    {
                        Id = subtitle.Attribute("id").Value,
                        LangCode = subtitle.Attribute("lang_code").Value,
                        LangOriginal = subtitle.Attribute("lang_original").Value,
                        LangTranslated = subtitle.Attribute("lang_translated").Value
                    });
                }

                foreach (var sub in subs)
                {
                    comboBox1.Items.Add(sub);
                }

                id++;
            }
            catch (Exception exception)
            {

            }
        }

        private bool RootHasChilds(XDocument availableSubsXDocument)
        {
            return availableSubsXDocument.Root.HasElements;
        }

        private Uri CreateValidUri()
        {
            Uri youtubeVideoUri;
            bool isUri = Uri.TryCreate(textBox1.Text, UriKind.Absolute, out youtubeVideoUri)
                         && (youtubeVideoUri.Scheme == Uri.UriSchemeHttp ||
                             youtubeVideoUri.Scheme == Uri.UriSchemeHttps);

            if (!isUri)
            {
                MessageBox.Show("Не валидный адрес.\r\nВведите верный адрес.", "Ошибка!");

            }
            else if (youtubeVideoUri.Host != "www.youtube.com")
            {
                MessageBox.Show("Это не адрес youtube.\r\nВведите верный адрес.", "Ошибка!");
            }
            else if (youtubeVideoUri.AbsoluteUri.EndsWith("watch?v="))
            {
                MessageBox.Show("Не верный адрес видео youtube.\r\nВведите верный адрес.", "Ошибка!");
            }

            return youtubeVideoUri;
        }

        private async Task<string> GetAvailableSubsDocument(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(uri);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedItem == null)
                {
                    MessageBox.Show("Выберите субтитры.", "Предупреждение!");
                }
                Subtitle targetSubtitle = null;
                foreach (var sub in subs)
                {
                    if (comboBox1.SelectedItem == sub)
                    {
                        targetSubtitle = sub;
                    }
                }
                string subDocument = await GetTargetSubDocument($"http://video.google.com/timedtext?type=track&v={videoId}&id={targetSubtitle.Id}&lang={targetSubtitle.LangCode}");
                XDocument subXmlDocument = XDocument.Parse(subDocument);
                WriteDocumentInFile(subXmlDocument, targetSubtitle);
                MessageBox.Show("Готово!", "Поздравления!");
            }
            catch (Exception exception)
            {
            }
        }

        private async Task<string> GetTargetSubDocument(string uri)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(uri);
            }
        }

        private void WriteDocumentInFile(XDocument subXmlDocument, Subtitle subtitle)
        {
            using (FileStream fs = File.Create($"{id}_{subtitle.LangTranslated}_{DateTime.Now.ToString("HH-mm-ss--dd-M-yy")}.txt"))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (var subElement in subXmlDocument.Root.Elements())
                    {
                        string newLine = subElement.Value;
                        sw.WriteLine(newLine.Replace("&#39;","'"));                      
                    }
                }
            }
        }
    }
}    