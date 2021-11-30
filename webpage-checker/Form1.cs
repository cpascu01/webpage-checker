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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> urlList = readSitemap();


            ScreenshotMaker screenmaker = new ScreenshotMaker();
            screenmaker.RemoveScrollBarsWhileShooting();
            VerticalCombineDecorator vcd = new VerticalCombineDecorator(screenmaker);

            progressBar1.Maximum = urlList.Count;
            foreach (string url in urlList)
            {
                _driver.Navigate().GoToUrl(url);
                
                var screen = _driver.TakeScreenshot(vcd);
                string filename = string.Format("{0}{1}", url.Replace("https://", "").Replace("/", "--"), ".png");
                File.WriteAllBytes(filename, screen);

                progressBar1.Value++;
            }
            _driver.Quit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string img1_ref, img2_ref;
            int count1 = 0, count2 = 0;
            bool flag = true;
            var img1 = new Bitmap("fullimage.png");
            var img2 = new Bitmap("fullimage2.png");
            progressBar1.Maximum = img1.Width;
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
                            count2 = +1;
                            flag = false;
                            break;
                        }
                        count1 = +1;
                    }
                    progressBar1.Value++;
                }
                if (flag == false)
                    MessageBox.Show("Sorry, Images are not same , " + count2 + " wrong pixels found");
                else
                    MessageBox.Show(" Images are same , " + count1 + " same pixels found and " + count2 + " wrong pixels found");
            }
            else
                MessageBox.Show("can not compare this images");
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

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
