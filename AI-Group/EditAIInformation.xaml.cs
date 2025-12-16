using System;
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
                !string.IsNullOrEmpty(AIUrl.Text) &&
                !string.IsNullOrEmpty(AIDescription.Text))
            {
                string[] data = new string[]
                {
                    AIName.Text,
                    AIUrl.Text,
                    AILogoUrl.Text == "" ? "https://icon.bqb.cool/?url=" + AIUrl.Text : AILogoUrl.Text,
                AIDescription.Text
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
    }
}
