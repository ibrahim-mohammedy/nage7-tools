using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UIAutomation.Tests.UFC.Tests.Groups;

namespace AutomationUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnDownloadImages_Click(object sender, EventArgs e)
        {
            EnglishCrawler crawler = new EnglishCrawler();
            crawler.CrawlImages_English(Path.Combine(txtOutputDir.Text, "images", "english"), txtWordsFilePath.Text);
            MessageBox.Show("Images finished.");
        }

        private void btnDownloadSounds_Click(object sender, EventArgs e)
        {
            EnglishCrawler crawler = new EnglishCrawler();
            crawler.TextToSpeech_English(Path.Combine(txtOutputDir.Text, "sound", "english"), txtWordsFilePath.Text);
            MessageBox.Show("Sound downloaded successfully.");
        }

        private void btnBrowseWordsFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK) txtWordsFilePath.Text = fd.FileName;
        }

        private void btnBrowseOutputDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK) txtWordsFilePath.Text = fd.SelectedPath;
        }
    }
}