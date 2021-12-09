using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using WDSE;
using WDSE.Decorators;
using WDSE.ScreenshotMaker;

namespace webpage_checker
{
    public partial class Form1 : Form
    {
        IWebDriver _driver;
        public Form1()
        {
            InitializeComponent();
            _driver = new ChromeDriver();
            _driver.Manage().Window.Size = new Size(1300, 1080);

            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> urlList = readSitemap();

            ScreenshotMaker screenmaker = new ScreenshotMaker();
            screenmaker.RemoveScrollBarsWhileShooting();
            VerticalCombineDecorator vcd = new VerticalCombineDecorator(screenmaker);

            progressBar1.Maximum = urlList.Count;
            progressBar1.Value = 0;
            foreach (string url in urlList)
            {
                _driver.Navigate().GoToUrl(url);

                var screen = _driver.TakeScreenshot(vcd);
                if (!Directory.Exists(comboBox1.SelectedItem.ToString().ToLower()))
                {
                    Directory.CreateDirectory(comboBox1.SelectedItem.ToString().ToLower());
                }
                string filename = string.Format("{0}\\{1}{2}", comboBox1.SelectedItem.ToString().ToLower(), url.Replace("https://", "").Replace("/", "--"), ".png");
                File.WriteAllBytes(filename, screen);

                progressBar1.Value++;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string[] preDeploymentFile = Directory.GetFiles(comboBox1.Items[0].ToString().ToLower());

            int total = 0;
            List<string> warnings = new List<string>();
            List<string> errors = new List<string>();
            foreach (string file in preDeploymentFile)
            {
                string postDeploymentFile = file.Replace(comboBox1.Items[0].ToString().ToLower(), comboBox1.Items[1].ToString().ToLower());

                string img1_ref, img2_ref;
                float count1 = 0, count2 = 0;
                bool flag = true;
                var img1 = new Bitmap(file);
                var img2 = new Bitmap(postDeploymentFile);
                progressBar1.Maximum = img1.Width;
                progressBar1.Value = 0;


                if (img1.Width == img2.Width && img1.Height == img2.Height)
                {
                    for (int i = 0; i < img1.Width; i++)
                    {
                        for (int j = 0; j < img1.Height; j++)
                        {
                            img1_ref = img1.GetPixel(i, j).ToString();
                            img2_ref = img2.GetPixel(i, j).ToString();
                            if (img1_ref != img2_ref)
                            {
                                count2++; ;
                                flag = false;
                                //break;
                            }
                            count1++;
                        }
                        progressBar1.Value++;
                    }
                    var percent = count2 / count1 * 100;
                    float threshold = 2;
                    if (flag == false && percent > threshold)
                    {
                        if (checkBox1.Checked)
                        {
                            textBox1.AppendText(string.Format("\r\nMismatch image between {0} and {1}: {2}%.", file, postDeploymentFile, percent));
                        }
                        errors.Add(file);
                    }
                    else if (percent > 0 && percent <= threshold)
                    {
                        if (checkBox1.Checked)
                        {
                            textBox1.AppendText(string.Format("\r\n{0}% variance between {1} and {2}.", percent, file, postDeploymentFile));
                        }
                        warnings.Add(file);
                    }

                }
                else
                    MessageBox.Show("can not compare this images");
            }
            textBox1.AppendText(string.Format("{0} pages analyzed. {1} errors. {2} warnings.", total, errors.Count, warnings.Count));

            textBox1.VisibleChanged += (sender, e) =>
            {
                if (textBox1.Visible)
                {
                    textBox1.SelectionStart = textBox1.TextLength;
                    textBox1.ScrollToCaret();
                }
            };
        }

        private List<string> readSitemap()
        {
            XmlDocument xmldoc = new XmlDocument();

            FileStream fs = new FileStream("sitemap.xml", FileMode.Open, FileAccess.Read);
            xmldoc.Load(fs);

            XmlNodeList urlNodes = xmldoc.GetElementsByTagName("url");
            List<string> urls = new List<string>();
            foreach (XmlNode url in urlNodes)
            {
                string location = url["loc"].InnerText;
                urls.Add(location);
            }

            fs.Close();
            fs.Dispose();

            return urls;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _driver.Quit();
        }

    }
}
