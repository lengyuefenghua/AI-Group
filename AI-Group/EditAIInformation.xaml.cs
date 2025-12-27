using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AI_Group
{
    /// <summary>
    /// EditAIInformation.xaml 的交互逻辑
    /// </summary>
    public partial class EditAIInformation : Window
    {
        public event Action<string[]> OnAIAdded;
        public EditAIInformation()
        {
            InitializeComponent();
        }
        private void AddAIButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(AIName.Text) &&
                !string.IsNullOrEmpty(AIUrl.Text))
            {
                string[] data = new string[]
                {
                    AIName.Text,
                    AIUrl.Text,
                    AILogoUrl.Text
                };
                OnAIAdded?.Invoke(data);
                this.Close();
                Console.WriteLine("AI added successfully");
            }
            else
            {
                MessageBox.Show("AI added Failed");
            }

        }

        private async void AIUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            // 正则表达式模式 (关键！)
            var res = await DownloadWebsiteLogoToFile(AIUrl.Text, ".\\Icon", AppDomain.CurrentDomain.BaseDirectory + "\\logo.ico");
            AILogoPreview.Source = res.First().Value;
            AILogoUrl.Text = res.First().Key;
        }

        private void LoadLocalIcon_Click(object sender, RoutedEventArgs e)
        {
            string websiteUrl = AIUrl.Text;
            if (String.IsNullOrWhiteSpace(websiteUrl))
            {
                MessageBox.Show("AI网址不能为空");
                return;
            }
            websiteUrl = GetDomainName(websiteUrl);
            if (websiteUrl == null)
            {
                MessageBox.Show("AI网址无效");
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择图标文件",
                Filter = "图像文件(*.png,*.ico,*.jpg)|*.png;*.ico;*.jpg", // 文件类型过滤
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory, // 初始目录
                Multiselect = false // 是否允许多选
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileNames[0];
                AILogoPreview.Source = new BitmapImage(new Uri(path, UriKind.Absolute));

                byte[] bytes = File.ReadAllBytes(path);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string fileExtension = Path.GetExtension(path);
                string savePath = Path.Combine(baseDir, "Icon\\" + websiteUrl + fileExtension);
                //删除旧图标文件
                string[] backupFiles = Directory.GetFiles(Path.Combine(baseDir, "Icon"), websiteUrl + ".*");
                foreach (string file in backupFiles)
                {
                    File.Delete(file);
                }
                File.WriteAllBytes(savePath, bytes);
                string relativePath = GetRelativePath(baseDir, savePath);
                AILogoUrl.Text = relativePath;
            }
        }


        public async Task<Dictionary<string, BitmapImage>> DownloadWebsiteLogoToFile(string websiteUrl, string saveDir, string defaultImagePath)
        {
            Dictionary<string, BitmapImage> res = new Dictionary<string, BitmapImage>();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string saveFileDir = saveDir;
            string fileExtension = ".png";
            //网址检查
            if (String.IsNullOrWhiteSpace(websiteUrl))
                throw new ArgumentException("网址不能为空");

            string domain = GetDomainName(websiteUrl);
            if (domain == null)
            {
                throw new ArgumentException("网址无效");
            }
            //保存干净的域名信息作为文件名
            websiteUrl = domain;

            //文件名及保存目录检查
            if (String.IsNullOrWhiteSpace(saveDir))
                throw new ArgumentNullException($"保存目录无效");

            //相对路径则变绝对路径
            if (!Path.IsPathRooted(saveDir))
            {
                saveFileDir = Path.GetFullPath(Path.Combine(baseDir, saveDir));
            }

            //保存目录检查,不存在则创建
            if (!Directory.Exists(saveFileDir))
                Directory.CreateDirectory(saveFileDir);

            BitmapImage image = null;
            //获取网络图标类型，仅支持ico,jpeg，jpg,png类型
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(3);
                client.DefaultRequestHeaders.Add(
                    "User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Mobile Safari/537.36 Edg/142.0.0.0");

                var response = await client.GetAsync("https://favicon.im/" + websiteUrl);
                if (response != null && response.IsSuccessStatusCode)
                {
                    string[] imageType = new string[] { "image/x-icon", "image/jpeg", "image/png" };
                    string contentType = response.Content.Headers.ContentType.ToString();
                    bool isMatch = imageType.Any(str => contentType.Contains(str));
                    if (isMatch)
                    {
                        byte[] bytes = await response.Content.ReadAsByteArrayAsync();

                        image = new BitmapImage();
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            ms.Position = 0;
                            image.BeginInit();
                            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            image.CacheOption = BitmapCacheOption.OnLoad; // 关键！避免流被关闭后无法加载
                            image.UriSource = null;
                            image.StreamSource = ms;
                            image.EndInit();
                            image.Freeze(); // 可选：使图像可跨线程使用，并减少内存占用
                        }

                        if (contentType.Contains(imageType[0]))
                        {
                            fileExtension = ".ico";
                        }
                        else if (contentType.Contains(imageType[1]))
                        {
                            fileExtension = ".jpg";
                        }
                        else if (contentType.Contains(imageType[2]))
                        {
                            fileExtension = ".png";
                        }
                        string savePath = Path.Combine(saveFileDir, websiteUrl + fileExtension);
                        string relativePath = GetRelativePath(baseDir, savePath);
                        res[relativePath] = image;
                        using (FileStream fs = File.OpenWrite(savePath))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }
                    }
                }

            }
            //image为空表示网络获取失败，直接使用默认图标
            if (image == null)
            {
                if (!File.Exists(defaultImagePath))
                    throw new ArgumentException("默认图片路径无效");
                fileExtension = Path.GetExtension(defaultImagePath);
                string savePath = Path.Combine(saveFileDir, websiteUrl + fileExtension);
                byte[] bytes = File.ReadAllBytes(defaultImagePath);
                string relativePath = GetRelativePath(baseDir, savePath);
                File.WriteAllBytes(savePath, bytes);
                res[relativePath] = new BitmapImage(new Uri(savePath, UriKind.Absolute));
            }
            return res;
        }

        /// <summary>
        /// 提取输入网址的域名
        /// </summary>
        /// <param name="url">输入网址</param>
        /// <returns>域名（失败返回null）</returns>
        public string GetDomainName(string url)
        {
            string pattern = @"(?:https?://)?([^/#?]+)";
            url = url.Trim();
            Match match = Regex.Match(url, pattern);
            if (!match.Success)
            {
                return null;
            }
            return match.Groups[1].Value;
        }


        /// <summary>
        /// 获取 targetPath 相对于 basePath 的相对路径
        /// </summary>
        public static string GetRelativePath(string basePath, string targetPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentException("Base path cannot be null or empty.", nameof(basePath));
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentException("Target path cannot be null or empty.", nameof(targetPath));

            // 确保路径以目录分隔符结尾（作为目录处理）
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                basePath += Path.DirectorySeparatorChar;

            Uri baseUri = new Uri(basePath);
            Uri targetUri = new Uri(targetPath);

            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
            return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

    }
}
