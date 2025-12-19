using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private string _aiDataFilePath = "AI-Group.xml";
        public MainWindow()
        {
            InitializeComponent();
            InitializaConfigFile();
            AddAIToUI();
        }

        private void InitializaConfigFile()
        {
            if (!File.Exists(_aiDataFilePath))
            {
                var aiElement = new XElement("AI",
                    new XElement("Name", "Doubao"),
                    new XElement("Url", "https://www.doubao.com/chat"),
                    new XElement("LogoUrl", "https://icon.bqb.cool/?url=" + "https://www.doubao.com/chat")
                );
                var doc = new XDocument(new XElement("Settings", new XElement("AIs", aiElement)));
                doc.Save(_aiDataFilePath);
            }
        }

        private async void AIButton_Click(object sender, RoutedEventArgs e)
        {
            string buttonTag = (string)((Button)sender).Tag;
            WebView2 wv = null;
            //若已经包含则先移出原有控件
            var chs = MainDockPanel.Children.OfType<UIElement>().ToList();
            foreach (UIElement ch in chs)
            {
                MainDockPanel.Children.Remove(ch);
                Console.WriteLine(ch);
            }
            //判断是否已加载过该webview
            if (_webViewDictUnload.ContainsKey(buttonTag))//表示该webview未加载
            {
                //初始化webview
                var temp = _webViewDictUnload[buttonTag];
                //把初始化后的webview移到已加载字典
                _webViewDictLoaded.Add(buttonTag, temp);
                _webViewDictUnload.Remove(buttonTag);
                //获取对应webview
                wv = temp;
                //导航到对应URL
                //先隐藏webview，等待加载完成后再显示
                wv.Visibility = Visibility.Collapsed;
                MainDockPanel.Children.Add(wv);
                MainDockPanel.Children.Add(lblWelcome);
                lblWelcome.Content = "正在加载 " + _aiUrlDict[buttonTag];
                await wv.EnsureCoreWebView2Async(null);
                wv.CoreWebView2.Navigate(_aiUrlDict[buttonTag]);


                //等待导航完成事件
                wv.CoreWebView2.NavigationCompleted += (s1, ev1) =>
                {
                    // 可以在这里处理导航完成后的逻辑
                    //添加到主面板
                    MainDockPanel.Children.Remove(lblWelcome);
                    wv.Visibility = Visibility.Visible;
                };
            }
            else//表示该webview已加载
            {
                wv = _webViewDictLoaded[buttonTag];
                //添加到主面板
                MainDockPanel.Children.Add(wv);
            }
        }

        private void AddAIToUI()
        {
            try
            {
                var doc = XDocument.Load(_aiDataFilePath);
                var aiElements = doc.Root.Element("AIs")?.Elements("AI");
                foreach (var aiElement in aiElements)
                {
                    string name = aiElement.Element("Name")?.Value;
                    string url = aiElement.Element("Url")?.Value;
                    string logoUrl = aiElement.Element("LogoUrl")?.Value;
                    if (!_aiUrlDict.ContainsKey(name))
                    {
                        var webView = new WebView2();
                        _webViewDictUnload[name] = webView;
                        _aiUrlDict[name] = url;
                        Image logoImage = new Image();
                        logoImage.Height = 30;
                        logoImage.Width = 30;
                        logoImage.Margin = new Thickness(3);
                        var success = SetIcon(logoImage, logoUrl);

                        Button aiButton = new Button
                        {
                            Content = logoImage,
                            ToolTip = name,
                            Margin = new Thickness(5),
                            Tag = name,
                            Height = 40,
                            Width = 40,
                            Style = this.TryFindResource("Normal") as Style
                        };
                        // 设置默认图标
                        if (!success)
                        {
                            aiButton.Content = name;
                        }

                        aiButton.ContextMenu = new ContextMenu();
                        MenuItem openMenuItem = new MenuItem { Header = "打开" };
                        MenuItem deleteMenuItem = new MenuItem { Header = "删除" };
                        MenuItem closeMenuItem = new MenuItem { Header = "关闭" };
                        openMenuItem.Click += (s, e) =>
                        {
                            AIButton_Click(aiButton, null);
                        };

                        deleteMenuItem.Click += (s, e) =>
                        {
                            // 删除按钮的点击事件
                            spAIButtons.Children.Remove(aiButton);
                            _aiUrlDict.Remove(aiButton.Tag.ToString());
                            if (_webViewDictUnload.ContainsKey(aiButton.Tag.ToString()))
                                _webViewDictUnload.Remove(aiButton.Tag.ToString());
                            if (_webViewDictLoaded.ContainsKey(aiButton.Tag.ToString()))
                                _webViewDictLoaded.Remove(aiButton.Tag.ToString());
                            RemoveAIFromXML(_aiDataFilePath, aiButton.Tag.ToString());
                        };
                        closeMenuItem.Click += (s, e) =>
                        {
                            // 关闭按钮的点击事件
                            if (_webViewDictLoaded.ContainsKey(aiButton.Tag.ToString()))
                            {
                                var wv = _webViewDictLoaded[aiButton.Tag.ToString()];
                                if (MainDockPanel.Children.Contains(wv))
                                {
                                    MainDockPanel.Children.Remove(wv);
                                    lblWelcome.Content = "欢迎使用 AI-Group！请选择左侧的 AI 聊天工具开始使用。";
                                    MainDockPanel.Children.Add(lblWelcome);
                                }
                                //把关闭的webview移到未加载字典
                                _webViewDictUnload.Add(aiButton.Tag.ToString(), wv);
                                _webViewDictLoaded.Remove(aiButton.Tag.ToString());
                            }
                        };

                        aiButton.ContextMenu.Items.Add(openMenuItem);
                        aiButton.ContextMenu.Items.Add(deleteMenuItem);
                        aiButton.ContextMenu.Items.Add(closeMenuItem);

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
                Console.WriteLine($"Name: {data[0]} URL: {data[1]} Logo URL: {data[2]}");
                var state = AddAIToXML(_aiDataFilePath, data[0], data[1], data[2]);
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
        private bool AddAIToXML(string filePath, string name, string url, string logoUrl)
        {
            try
            {
                var aiElement = new XElement("AI",
                    new XElement("Name", name),
                    new XElement("Url", url),
                    new XElement("LogoUrl", logoUrl)
                );

                var doc = XDocument.Load(filePath);

                // 确保 <AIs> 节点存在
                var aisContainer = doc.Root.Element("AIs");
                if (aisContainer == null)
                {
                    aisContainer = new XElement("AIs");
                    doc.Root.Add(aisContainer);
                }

                bool found = false;
                var existingAIs = aisContainer.Elements("AI");

                foreach (var existingAI in existingAIs)
                {
                    if (existingAI.Element("Name")?.Value == name)
                    {
                        // 更新现有条目
                        existingAI.Element("Url")?.SetValue(url);
                        existingAI.Element("LogoUrl")?.SetValue(logoUrl);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    aisContainer.Add(aiElement);
                }

                doc.Save(filePath);
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
                var allAI = doc.Descendants("AI");
                var aiToRemove = allAI.FirstOrDefault(ai => ai.Element("Name")?.Value == name);
                if (aiToRemove != null)
                {
                    aiToRemove.Remove();
                    doc.Save(filePath);
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
