using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nge7_Helper
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnUpdateAndroidWebFiles_Click(object sender, EventArgs e)
        {
            string src = txtSource.Text.Trim();
            string trg = txtTarget.Text.Trim();
            if (Directory.Exists(trg)) Directory.Delete(trg, true);
            Utilities.CopyFilesRecursively(src, trg);
        }

        private void btnRenameImages_Click(object sender, EventArgs e)
        {
            string imagesFolder = txtSource.Text + @"\data\media\images\english";
            string[] folders = Directory.GetDirectories(imagesFolder);
            foreach (string folder in folders)
            {
                Directory.Move(folder, folder + "_Renaming");
                Directory.CreateDirectory(folder);
                string[] files = Directory.GetFiles(folder + "_Renaming", "*.jpg");
                for (int i = 0; i < files.Length; i++)
                {
                    File.Move(files[i], $"{folder}\\{i}.jpg");
                }
            }
        }

        private void btnFoldersNames_Click(object sender, EventArgs e)
        {
            string imagesFolder = txtSource.Text + @"\data\media\sound\english";
            string[] folders = Directory.GetDirectories(imagesFolder);
            foreach (string folder in folders)
            {
                string cleanPath = Path.GetDirectoryName(folder) + "\\" + CleanName(Path.GetFileName(folder));
                if (cleanPath == folder) continue;

                if (Directory.Exists(cleanPath))
                {
                    Directory.Delete(folder, true);
                    continue;
                }

                Directory.Move(folder, cleanPath);
            }
        }

        private string CleanName(string word)
        {
            string cleanWord = Regex.Replace(word, "\\W+", "-");
            cleanWord = cleanWord.Trim('-');

            return cleanWord.ToLower();
        }
    }
}