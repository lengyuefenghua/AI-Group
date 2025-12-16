using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AI_Group
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, WebView2> _webViewDictUnload = new Dictionary<string, WebView2>();
        private Dictionary<string, WebView2> _webViewDictLoaded = new Dictionary<string, WebView2>();
        private Dictionary<string, string> _aiUrlDict = new Dictionary<string, string>();
        private string _aiDataFilePath = Path.Combine(Environment.CurrentDirectory, "AIs.xml");
        private CoreWebView2Environment _env;
        public MainWindow()
        {
            InitializeComponent();

            if (!File.Exists(_aiDataFilePath))
            {
                var aiElement = new XElement("AI",
                    new XElement("Name", "Doubao"),
                    new XElement("Url", "https://www.doubao.com/chat"),
                    new XElement("LogoUrl", "https://icon.bqb.cool/?url=" + "https://www.doubao.com/chat"),
                    new XElement("Description", "Doubao AI")
                );
                var doc = new XDocument(new XElement("AIs", aiElement));
                doc.Save(_aiDataFilePath);
            }
            GetWebView2Environment();
            AddAIToUI();
        }

        private async void GetWebView2Environment()
        {
            string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }
            _env = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,  // 自动查找Edge运行时
                userDataFolder: cacheDir        // 自定义缓存目录
            );
        }

        private async void AIButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonTag = (string)((Button)sender).Tag;

            //若已经包含则先移出原有控件
            if (MainDockPanel.Children.Count == 1)
            {
                MainDockPanel.Children.RemoveAt(0);
            }
            //判断是否已加载过该webview
            if (_webViewDictUnload.ContainsKey(buttonTag))//表示该webview未加载
            {

                //初始化webview
                var temp = _webViewDictUnload[buttonTag];
                if (_env == null)
                {
                    try
                    {
                        string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache");
                        if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);
                        _env = await CoreWebView2Environment.CreateAsync(browserExecutableFolder: null, userDataFolder: cacheDir);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("CreateAsync 异常: " + ex);
                        MessageBox.Show("创建 WebView2 环境失败：" + ex.Message);
                        return;
                    }
                }
                // 使用超时与异常捕获来防止 await 卡住或异常导致后续行未执行
                bool initialized = false;
                try
                {
                    initialized = await EnsureCoreWebView2WithTimeoutAsync(temp, _env, TimeSpan.FromSeconds(15));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Ensure 调用抛出异常: " + ex);
                    MessageBox.Show("初始化 WebView2 时出现异常：" + ex.Message);
                    return;
                }

                if (!initialized)
                {
                    System.Diagnostics.Debug.WriteLine("EnsureCoreWebView2Async 未完成（超时或失败）。");
                    MessageBox.Show("WebView2 初始化超时或失败，查看输出窗口以获取详细日志。");
                    return;
                }
                // 初始化WebView2时传入环境
                //await temp.EnsureCoreWebView2Async(_env);
                //await temp.EnsureCoreWebView2Async(null);
                //把初始化后的webview移到已加载字典
                _webViewDictLoaded.Add(buttonTag, temp);
                _webViewDictUnload.Remove(buttonTag);
            }
            //获取对应webview
            var wv = _webViewDictLoaded[buttonTag];
            //导航到对应URL
            wv.Source = new Uri(_aiUrlDict[buttonTag]);
            //添加到主面板
            MainDockPanel.Children.Add(wv);

        }
        private async System.Threading.Tasks.Task<bool> EnsureCoreWebView2WithTimeoutAsync(WebView2 wv, CoreWebView2Environment env, TimeSpan timeout)
        {
            try
            {
                var initTask = wv.EnsureCoreWebView2Async(env);
                var completed = await System.Threading.Tasks.Task.WhenAny(initTask, System.Threading.Tasks.Task.Delay(timeout));
                if (completed != initTask)
                {
                    System.Diagnostics.Debug.WriteLine("EnsureCoreWebView2Async 超时");
                    return false;
                }
                // 等待以抛出可能的异常
                await initTask;
                System.Diagnostics.Debug.WriteLine("EnsureCoreWebView2Async 完成");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EnsureCoreWebView2Async 异常: " + ex);
                return false;
            }
        }
        private void AddAIToUI()
        {
            try
            {
                var doc = XDocument.Load(_aiDataFilePath);
                var aiElements = doc.Root.Elements("AI");
                foreach (var aiElement in aiElements)
                {
                    string name = aiElement.Element("Name")?.Value;
                    string url = aiElement.Element("Url")?.Value;
                    string logoUrl = aiElement.Element("LogoUrl")?.Value;
                    string description = aiElement.Element("Description")?.Value;
                    if (!_aiUrlDict.ContainsKey(name))
                    {
                        var webView = new WebView2();
                        _webViewDictUnload[name] = webView;
                        _aiUrlDict[name] = url;
                        Image logoImage = new Image();
                        var success = SetIcon(logoImage, logoUrl);

                        Button aiButton = new Button
                        {
                            Content = logoImage,
                            ToolTip = description,
                            Margin = new Thickness(5),
                            Tag = name,
                            Height = 45,
                            Width = 45,
                            Style = this.TryFindResource("Normal") as Style
                        };
                        // 设置默认图标
                        if (!success)
                        {
                            aiButton.Content = name;
                        }
                        spAIButtons.Children.Insert(spAIButtons.Children.Count - 1, aiButton); // 新按钮插入到最前面
                        aiButton.Click += AIButton_Click;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load AI data: " + ex.Message);
            }
        }

        private void AddAIButton_Click(object sender, RoutedEventArgs e)
        {
            var addNewWebAI = new EditAIInformation();
            addNewWebAI.Owner = this;
            Action<string[]> _aiAddedHandler = (data) =>
            {
                Console.WriteLine($"Name: {data[0]} URL: {data[1]} Logo URL: {data[2]} Description: {data[3]}");
                var state = AddAIToXML(_aiDataFilePath, data[0], data[1], data[2], data[3]);
            };
            addNewWebAI.OnAIAdded += _aiAddedHandler;
            addNewWebAI.ShowDialog();
            addNewWebAI.OnAIAdded -= _aiAddedHandler;
            AddAIToUI();

        }
        /// <summary>
        /// 将 AI 条目追加到用户 AppData 下的 AIs.xml 文件。
        /// 返回 true 表示成功；失败时返回 false
        /// </summary>
        private bool AddAIToXML(string filePath, string name, string url, string logoUrl, string description)
        {
            try
            {
                //拼接新的 AI 元素
                var aiElement = new XElement("AI",
                    new XElement("Name", name),
                    new XElement("Url", url),
                    new XElement("LogoUrl", logoUrl),
                    new XElement("Description", description)
                    );
                var doc = XDocument.Load(filePath);
                var root = doc.Element("AIs");
                var finded = false;
                // 检查是否已存在同名的 AI 条目,并更新
                foreach (var existingAI in root.Elements("AI"))
                {
                    if (existingAI.Element("Name")?.Value == name)
                    {
                        finded = true;
                        // 更新已有的 AI 条目
                        existingAI.Element("Url")?.SetValue(url);
                        existingAI.Element("LogoUrl")?.SetValue("https://icon.bqb.cool/?url=" + logoUrl);
                        existingAI.Element("Description")?.SetValue(description);
                        doc.Save(filePath);
                        break;
                    }
                }
                if (!finded)
                {
                    root.Add(aiElement);
                    doc.Save(filePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving AI data: " + ex.Message);
                return false;
            }
        }

        private bool RemoveAIFromXML(string filePath, string name)
        {
            try
            {
                var doc = XDocument.Load(filePath);
                var root = doc.Element("AIs");
                foreach (var existingAI in root.Elements("AI"))
                {
                    if (existingAI.Element("Name")?.Value == name)
                    {
                        existingAI.Remove();
                        doc.Save(filePath);
                        break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting AI entry: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 设置Image控件的图片源，URL不可用时返回false
        /// </summary>
        /// <param name="image">目标Image控件</param>
        /// <param name="imageUrl">图片URL（绝对路径/网络地址）</param>
        /// <returns>加载成功返回true，失败返回false</returns>
        private bool SetIcon(Image image, string imageUrl)
        {
            // 入参校验：Image控件不能为空，URL不能为空/空白
            if (image == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                image.Source = null; // 清空原有图片
                return false;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();

                // 核心设置：确保同步加载并缓存，避免异步加载导致的异常捕获失效
                bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // 忽略缓存，实时检测URL有效性

                // 可选：设置超时（WPF无直接超时，需结合异步，见扩展说明）
                bitmap.EndInit();

                // 验证图片是否真的加载成功（部分场景EndInit不抛异常，但图片为空）
                if (bitmap.PixelWidth == 0 || bitmap.PixelHeight == 0)
                {
                    image.Source = null;
                    return false;
                }

                // 加载成功，设置图片源
                image.Source = bitmap;
                return true;
            }
            catch (UriFormatException)
            {
                // URL格式错误（如非法字符、未加协议头）
                image.Source = null;
                return false;
            }
            catch (IOException)
            {
                // 网络不可达、URL不存在、文件无法读取等IO异常（URL不可用的核心场景）
                image.Source = null;
                return false;
            }
            catch (NotSupportedException)
            {
                // 不支持的URL协议、图片格式等
                image.Source = null;
                return false;
            }
            catch (Exception)
            {
                // 兜底捕获其他未知异常（如权限问题）
                image.Source = null;
                return false;
            }
        }


    }
}
