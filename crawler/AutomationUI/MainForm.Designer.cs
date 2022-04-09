
namespace AutomationUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnDownloadSounds = new System.Windows.Forms.Button();
            this.btnDownloadImages = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtWordsFilePath = new System.Windows.Forms.TextBox();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnBrowseWordsFile = new System.Windows.Forms.Button();
            this.btnBrowseOutputDir = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDownloadSounds
            // 
            this.btnDownloadSounds.Location = new System.Drawing.Point(286, 203);
            this.btnDownloadSounds.Name = "btnDownloadSounds";
            this.btnDownloadSounds.Size = new System.Drawing.Size(120, 35);
            this.btnDownloadSounds.TabIndex = 0;
            this.btnDownloadSounds.Text = "Download Sound";
            this.btnDownloadSounds.UseVisualStyleBackColor = true;
            this.btnDownloadSounds.Click += new System.EventHandler(this.btnDownloadSounds_Click);
            // 
            // btnDownloadImages
            // 
            this.btnDownloadImages.Location = new System.Drawing.Point(143, 203);
            this.btnDownloadImages.Name = "btnDownloadImages";
            this.btnDownloadImages.Size = new System.Drawing.Size(120, 35);
            this.btnDownloadImages.TabIndex = 1;
            this.btnDownloadImages.Text = "Download Images";
            this.btnDownloadImages.UseVisualStyleBackColor = true;
            this.btnDownloadImages.Click += new System.EventHandler(this.btnDownloadImages_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(85, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Words file";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(85, 112);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Output Dir";
            // 
            // txtWordsFilePath
            // 
            this.txtWordsFilePath.Location = new System.Drawing.Point(172, 80);
            this.txtWordsFilePath.Name = "txtWordsFilePath";
            this.txtWordsFilePath.Size = new System.Drawing.Size(219, 20);
            this.txtWordsFilePath.TabIndex = 4;
            this.txtWordsFilePath.Text = "C:\\Projects\\nage7\\web\\data\\e-books\\G1\\english\\all-words.txt";
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Location = new System.Drawing.Point(172, 112);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(219, 20);
            this.txtOutputDir.TabIndex = 5;
            this.txtOutputDir.Text = "C:\\Projects\\nage7\\web\\data\\media";
            // 
            // btnBrowseWordsFile
            // 
            this.btnBrowseWordsFile.Location = new System.Drawing.Point(409, 79);
            this.btnBrowseWordsFile.Name = "btnBrowseWordsFile";
            this.btnBrowseWordsFile.Size = new System.Drawing.Size(69, 21);
            this.btnBrowseWordsFile.TabIndex = 6;
            this.btnBrowseWordsFile.Text = "Browse";
            this.btnBrowseWordsFile.UseVisualStyleBackColor = true;
            this.btnBrowseWordsFile.Click += new System.EventHandler(this.btnBrowseWordsFile_Click);
            // 
            // btnBrowseOutputDir
            // 
            this.btnBrowseOutputDir.Location = new System.Drawing.Point(409, 112);
            this.btnBrowseOutputDir.Name = "btnBrowseOutputDir";
            this.btnBrowseOutputDir.Size = new System.Drawing.Size(69, 21);
            this.btnBrowseOutputDir.TabIndex = 7;
            this.btnBrowseOutputDir.Text = "Browse";
            this.btnBrowseOutputDir.UseVisualStyleBackColor = true;
            this.btnBrowseOutputDir.Click += new System.EventHandler(this.btnBrowseOutputDir_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnBrowseOutputDir);
            this.Controls.Add(this.btnBrowseWordsFile);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.txtWordsFilePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnDownloadImages);
            this.Controls.Add(this.btnDownloadSounds);
            this.Name = "MainForm";
            this.Text = "Nage7 Automation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDownloadSounds;
        private System.Windows.Forms.Button btnDownloadImages;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWordsFilePath;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnBrowseWordsFile;
        private System.Windows.Forms.Button btnBrowseOutputDir;
    }
}

