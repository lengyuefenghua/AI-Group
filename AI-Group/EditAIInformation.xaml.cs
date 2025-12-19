using System;
using System.Text.RegularExpressions;
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

        public EditAIInformation(string[] aiData) : this()
        {
            // 1. 安全检查：防止数组为空或长度不足
            if (aiData == null || aiData.Length < 3)
            {
                // 可以选择记录日志或设置默认值
                return;
            }

            AIName.Text = aiData[0];
            AIUrl.Text = aiData[1];
            AILogoUrl.Text = aiData[2];

            string imgUrl = aiData[2];

            // 2. 图片加载安全处理
            if (!string.IsNullOrWhiteSpace(imgUrl))
            {
                try
                {
                    // 使用 UriKind.RelativeOrAbsolute 兼容本地路径和网络路径
                    if (Uri.TryCreate(imgUrl, UriKind.RelativeOrAbsolute, out Uri resultUri))
                    {
                        var bitmap = new BitmapImage();

                        bitmap.BeginInit();
                        bitmap.UriSource = resultUri;
                        // 3. 异步加载：防止大图或网络图卡死 UI
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        // 如果是网络图片，建议加上这个，让它在后台下载
                        bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                        bitmap.EndInit();

                        AILogoPreview.Source = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    // 图片加载失败时，可以设置一个默认的“加载失败”图片，或者什么都不做
                    AILogoPreview.Source = new BitmapImage(new Uri("pack://application:,,,/logo.ico"));
                }
            }
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
                    AILogoUrl.Text == "" ? "https://icon.bqb.cool/?url=" + AIUrl.Text : AILogoUrl.Text,
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

        private void AIUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            // 正则表达式模式 (关键！)
            string pattern = @"(?:https?://)?([^/#?]+)";

            var tb = sender as System.Windows.Controls.TextBox;
            var inputUrl = tb.Text.Trim();

            Match match = Regex.Match(inputUrl, pattern);
            if (match.Success)
            {
                Console.WriteLine($"URL: {inputUrl} → 域名: {match.Groups[1].Value}");
                AILogoUrl.Text = "https://favicon.im/" + match.Groups[1].Value;
                AILogoPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(AILogoUrl.Text));
            }
            else
            {
                Console.WriteLine($"URL: {inputUrl} → 匹配失败！");
            }
        }
    }
}
