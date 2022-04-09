using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using UIAutomation.Utils_Selenium;

namespace UIAutomation.Tests.UFC.Tests.Groups
{
    public class EnglishCrawler
    {
        public void CrawlImages_English(string imagesDir = @"C:\Projects\nage7\web\wwwroot\content\images\english",
            string wordsFile = @"C:\Projects\nage7\data\media\prime3\english\all-words.txt")
        {
            WebClient client = new WebClient();
            Random random = new Random();
            CustomDriver driver = null;
            foreach (string word in File.ReadLines(wordsFile))
            {
                try
                {
                    string cleanWord = CleanWord(word);
                    string wordDir = Path.Combine(imagesDir, cleanWord);
                    if (Directory.Exists(wordDir) && Directory.GetFiles(wordDir).Length >= 10) continue;

                    if (string.IsNullOrEmpty(cleanWord)) continue;
                    var chromeDriverService = ChromeDriverService.CreateDefaultService();
                    chromeDriverService.HideCommandPromptWindow = true;
                    ChromeOptions chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("test-type");
                    chromeOptions.AddArgument("ignore-certificate-errors");
                    chromeOptions.AddArgument("disable-infobars");
                    driver = new CustomDriver(new ChromeDriver(chromeDriverService, chromeOptions, TimeSpan.FromMinutes(2)));
                    driver.Url = $"https://www.google.com/search?q=clipart+{word.Replace(" ", "+")}&tbm=isch";
                    driver.Navigate();

                    Thread.Sleep(2000);
                    for (int i = 0; i < 20; i++) driver.FindElement(By.TagName("body")).SendKeys(Keys.ArrowDown);
                    Thread.Sleep(2000);
                    for (int i = 0; i < 20; i++) driver.FindElement(By.TagName("body")).SendKeys(Keys.ArrowDown);

                    Thread.Sleep(5000);

                    List<WebElement> images = driver.FindWebElements(By.TagName("img")).FindAll(x => x.GetAttribute("width") != null && x.GetAttribute("height") != null && int.Parse(x.GetAttribute("width")) > 100 && int.Parse(x.GetAttribute("height")) > 100);
                    if (images.Count == 0) continue;
                    Directory.CreateDirectory(wordDir);
                    for (int i = 0; i < images.Count; i++)
                    {
                        byte[] binaryData = null;
                        string url = images[i].GetAttribute("src");
                        if (string.IsNullOrWhiteSpace(url)) continue;
                        if (url.StartsWith("data:image"))
                        {
                            string base64Data = url.Substring(url.IndexOf("base64,") + 7);
                            binaryData = Convert.FromBase64String(base64Data);
                        }
                        else
                        {
                            Stream reader = client.OpenRead(url);
                            List<byte> bytes = new List<byte>();
                            int readByte = reader.ReadByte();
                            while (readByte != -1)
                            {
                                bytes.Add(Convert.ToByte(readByte));
                                readByte = reader.ReadByte();
                            }
                            binaryData = bytes.ToArray(); ;
                        }

                        string ext = GetInlineImageExtension(url);

                        if (binaryData.Length == 0) continue;
                        if (string.IsNullOrEmpty(ext))
                        {
                            ext = DetectExtension(binaryData);
                        }
                        if (string.IsNullOrEmpty(ext)) continue;

                        string targetPath = Path.Combine(wordDir, i + ext);

                        File.WriteAllBytes(targetPath, binaryData);
                        if (ext != ".jpg")
                        {
                            string tmpImg = Path.GetTempFileName() + ext;
                            File.Copy(targetPath, tmpImg);
                            Process.Start("c:\\tools\\ffmpeg.exe", "-i " + tmpImg + " " + Path.Combine(imagesDir, cleanWord, i + ".jpg"));
                            Thread.Sleep(1000);
                            File.Delete(targetPath);
                        }
                    }
                    driver.Close();
                    Thread.Sleep(new Random().Next(1000, 3000));
                }
                catch (Exception ex)
                {
                    try
                    {
                        driver.Close();
                    }
                    catch (Exception)
                    {
                    }
                    Thread.Sleep(new Random().Next(120000, 200000));
                }
            }
        }

        private string GetInlineImageExtension(string url)
        {
            if (url.StartsWith("data:image/png")) return ".png";
            if (url.StartsWith("data:image/jpg")) return ".jpg";
            if (url.StartsWith("data:image/jpeg")) return ".jpeg";
            if (url.StartsWith("data:image/bmp")) return ".bmp";
            if (url.StartsWith("data:image/gif")) return ".gif";
            if (url.StartsWith("http")) return Path.GetExtension(url);

            return "";
        }

        private string DetectExtension(byte[] binaryData)
        {
            Image img = Image.FromStream(new MemoryStream(binaryData));
            if (img == null) return "";

            if (ImageFormat.Jpeg.Equals(img.RawFormat)) return ".jpeg";
            if (ImageFormat.Bmp.Equals(img.RawFormat)) return ".bmp";
            if (ImageFormat.Png.Equals(img.RawFormat)) return ".png";
            if (ImageFormat.Gif.Equals(img.RawFormat)) return ".gif";
            if (ImageFormat.Tiff.Equals(img.RawFormat)) return ".tiff";
            if (ImageFormat.Icon.Equals(img.RawFormat)) return ".ico";

            return "";
        }

        public void TextToSpeech_English(string soundsDir = @"C:\Projects\nage7\web\wwwroot\content\sound\english", string wordsFile = @"C:\Projects\nage7\data\media\prime3\english\all-words.txt")
        {
            WebClient client = new WebClient();
            Random random = new Random();
            List<KeyValuePair<string, string>> sounds = new List<KeyValuePair<string, string>>();
            sounds.Add(new KeyValuePair<string, string>("en-US-Standard-B", "0"));
            //sounds.Add(new KeyValuePair<string, string>("en-US-Standard-C", "0"));
            //sounds.Add(new KeyValuePair<string, string>("en-US-Standard-D", "0"));
            //sounds.Add(new KeyValuePair<string, string>("en-US-Standard-E", "0"));
            //sounds.Add(new KeyValuePair<string, string>("en-US-Standard-E", "0"));
            //sounds.Add(new KeyValuePair<string, string>("Salli_Female", "1"));
            ////sounds.Add(new KeyValuePair<string, string>("Joanna_Male", "1"));
            //sounds.Add(new KeyValuePair<string, string>("Matthew_Male", "0"));
            ////sounds.Add(new KeyValuePair<string, string>("Ivy_Female", "1"));
            ////sounds.Add(new KeyValuePair<string, string>("Justin_Male", "1"));
            //sounds.Add(new KeyValuePair<string, string>("Kendra_Female", "1"));
            //sounds.Add(new KeyValuePair<string, string>("Kimberly_Female", "1"));
            //sounds.Add(new KeyValuePair<string, string>("Joey_Male", "1"));

            if (!Directory.Exists(soundsDir)) Directory.CreateDirectory(soundsDir);
            foreach (string word in File.ReadLines(wordsFile))
            {
                string cleanWord, wordDir;
                try
                {
                    cleanWord = CleanWord(word);
                    wordDir = Path.Combine(soundsDir, cleanWord);
                    if (Directory.Exists(wordDir) && Directory.GetFiles(wordDir).Length >= sounds.Count) continue;
                    else Directory.CreateDirectory(wordDir);

                    if (string.IsNullOrEmpty(cleanWord)) continue;
                    for (int i = 0; i < sounds.Count; i++)
                    {
                        string targetPath = Path.Combine(wordDir, i + ".mp3");
                        if (File.Exists(targetPath)) continue;

                        string response = client.DownloadString($"https://freetts.com/Home/PlayAudio?Language=en-US&Voice={sounds[i].Key}&TextMessage={word}&type={sounds[i].Value}");
                        JObject json = JObject.Parse(response);
                        string id = json["id"].Value<string>();
                        string url = $"https://freetts.com/audio/{id}";

                        Stream reader = client.OpenRead(url);
                        List<byte> bytes = new List<byte>();
                        int readByte = reader.ReadByte();
                        while (readByte != -1)
                        {
                            bytes.Add(Convert.ToByte(readByte));
                            readByte = reader.ReadByte();
                        }
                        byte[] binaryData = bytes.ToArray(); ;

                        if (binaryData.Length == 0) continue;

                        Thread.Sleep(new Random().Next(5000, 7000));
                        File.WriteAllBytes(targetPath, binaryData);
                    }
                    Thread.Sleep(new Random().Next(60000, 70000));
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    Thread.Sleep(new Random().Next(500000, 600000));
                }
            }
        }

        private string CleanWord(string word)
        {
            string cleanWord = Regex.Replace(word, "\\W+", "-");
            cleanWord = cleanWord.Trim('-');

            return cleanWord.ToLower();
        }
    }
}