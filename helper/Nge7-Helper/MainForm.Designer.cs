
namespace Nge7_Helper
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
            this.btnRenameImages = new System.Windows.Forms.Button();
            this.txtTarget = new System.Windows.Forms.TextBox();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnUpdateAndroidWebFiles = new System.Windows.Forms.Button();
            this.btnFoldersNames = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnRenameImages
            // 
            this.btnRenameImages.Location = new System.Drawing.Point(330, 125);
            this.btnRenameImages.Name = "btnRenameImages";
            this.btnRenameImages.Size = new System.Drawing.Size(123, 23);
            this.btnRenameImages.TabIndex = 1;
            this.btnRenameImages.Text = "Rename images files";
            this.btnRenameImages.UseVisualStyleBackColor = true;
            this.btnRenameImages.Click += new System.EventHandler(this.btnRenameImages_Click);
            // 
            // txtTarget
            // 
            this.txtTarget.Location = new System.Drawing.Point(74, 44);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.Size = new System.Drawing.Size(365, 20);
            this.txtTarget.TabIndex = 9;
            this.txtTarget.Text = "C:\\Projects\\nage7\\mobile\\android\\nage7\\www";
            // 
            // txtSource
            // 
            this.txtSource.Location = new System.Drawing.Point(74, 12);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(365, 20);
            this.txtSource.TabIndex = 8;
            this.txtSource.Text = "C:\\Projects\\nage7\\web";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Target";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Source";
            // 
            // btnUpdateAndroidWebFiles
            // 
            this.btnUpdateAndroidWebFiles.Location = new System.Drawing.Point(30, 125);
            this.btnUpdateAndroidWebFiles.Name = "btnUpdateAndroidWebFiles";
            this.btnUpdateAndroidWebFiles.Size = new System.Drawing.Size(116, 23);
            this.btnUpdateAndroidWebFiles.TabIndex = 5;
            this.btnUpdateAndroidWebFiles.Text = "Update Android";
            this.btnUpdateAndroidWebFiles.UseVisualStyleBackColor = true;
            this.btnUpdateAndroidWebFiles.Click += new System.EventHandler(this.btnUpdateAndroidWebFiles_Click);
            // 
            // btnFoldersNames
            // 
            this.btnFoldersNames.Location = new System.Drawing.Point(309, 182);
            this.btnFoldersNames.Name = "btnFoldersNames";
            this.btnFoldersNames.Size = new System.Drawing.Size(144, 23);
            this.btnFoldersNames.TabIndex = 10;
            this.btnFoldersNames.Text = "Rename Folders Names";
            this.btnFoldersNames.UseVisualStyleBackColor = true;
            this.btnFoldersNames.Click += new System.EventHandler(this.btnFoldersNames_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 392);
            this.Controls.Add(this.btnFoldersNames);
            this.Controls.Add(this.btnRenameImages);
            this.Controls.Add(this.txtTarget);
            this.Controls.Add(this.txtSource);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUpdateAndroidWebFiles);
            this.Name = "MainForm";
            this.Text = "Nage7";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRenameImages;
        private System.Windows.Forms.TextBox txtTarget;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUpdateAndroidWebFiles;
        private System.Windows.Forms.Button btnFoldersNames;
    }
}

