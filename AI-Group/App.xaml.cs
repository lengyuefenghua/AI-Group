using System.Windows;
using wf = System.Windows.Forms;
namespace AI_Group
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private MainWindow mainWindow = null;
        private wf.NotifyIcon notifyIcon = null;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mainWindow = mw;
            //mw.Show();   // 先创建并显示窗口（确保窗口句柄生成）
            mainWindow.Show();   // 立即隐藏，避免闪烁
            notifyIcon = new wf.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon("logo.ico");
            notifyIcon.Text = "AI-Group";
            notifyIcon.Visible = true;
            var contextMenu = new wf.ContextMenuStrip();
            var toggleItem = new wf.ToolStripMenuItem("开机启动");
            toggleItem.Checked = false; // 添加打勾标记
            contextMenu.Items.Add("开机启动", null, (s, ea) => Application.Current.Shutdown());
            contextMenu.Items.Add("显示主窗口", null, (s, ea) => mainWindow.Show());
            contextMenu.Items.Add("退出", null, (s, ea) => Application.Current.Shutdown());
            notifyIcon.ContextMenuStrip = contextMenu;
            mainWindow.Closing += (s, ea) => { mainWindow.Hide(); ea.Cancel = true; };
        }

    }
}
