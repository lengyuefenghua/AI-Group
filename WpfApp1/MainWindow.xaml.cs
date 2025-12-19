using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private CoreWebView2Controller webView;
        public MainWindow()
        {
            InitializeComponent();
            Image img = Image.FromFile("C:/Users/liaoweijia/Downloads/geminisvg.svg");
            string base64 = ConvertImageToBase64(img);
            Console.WriteLine(base64);

        }
        public string ConvertImageToBase64(Image file)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.Save(memoryStream, file.RawFormat);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
        }
        public Image ConvertBase64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                ms.Write(imageBytes, 0, imageBytes.Length);
                return Image.FromStream(ms, true);
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private CoreWebView2 webView2 => webView?.CoreWebView2;
    }
}
