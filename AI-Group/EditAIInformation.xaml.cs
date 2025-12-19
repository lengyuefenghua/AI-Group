using System;
using System.Text.RegularExpressions;
using System.Windows;

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
