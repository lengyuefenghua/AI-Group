using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace AI_Group
{
    public partial class App : System.Windows.Application
    {
        private MainWindow _mainWindow = null;
        private NotifyIcon _notifyIcon = null;
        private const int HOTKEY_ID = 2025;
        private IntPtr _mainWindowHandle = IntPtr.Zero;
        private HwndSource _hwndSource = null;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 创建但不立即显示主窗口
            _mainWindow = new MainWindow();
            if (Debugger.IsAttached)
            {
                // _mainWindow.Show();
            }
            // 订阅窗口的SourceInitialized事件，确保句柄已创建
            _mainWindow.SourceInitialized += MainWindow_SourceInitialized;
            var helper = new WindowInteropHelper(_mainWindow);
            helper.EnsureHandle();
            // 隐藏窗口（而不是关闭）
            _mainWindow.Closing += (s, args) =>
            {
                if (!Debugger.IsAttached)
                {
                    _mainWindow.Hide();
                    args.Cancel = true;
                }
            };

            // 延迟初始化托盘图标和热键
            InitializeTrayIcon();
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            // 确保在主UI线程上执行
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // 获取窗口句柄
                    _mainWindowHandle = new WindowInteropHelper(_mainWindow).Handle;
                    uint keyModifiers = (uint)HotKey.KeyModifiers.Alt;
                    if (Debugger.IsAttached)
                        keyModifiers = (uint)(HotKey.KeyModifiers.Alt | HotKey.KeyModifiers.Shift);
                    // 注册热键 ALT+~（反引号）
                    if (!HotKey.RegisterHotKey(_mainWindowHandle, HOTKEY_ID,
                        keyModifiers, (uint)Keys.Oemtilde))
                    {
                        System.Diagnostics.Debug.WriteLine("热键注册失败！");
                    }

                    // 添加消息处理钩子
                    _hwndSource = HwndSource.FromHwnd(_mainWindowHandle);
                    if (_hwndSource != null)
                    {
                        _hwndSource.AddHook(WndProc);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"热键注册错误: {ex.Message}");
                }
            });
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon();

            try
            {
                var uri = new Uri("pack://application:,,,/logo.ico");
                var resource = System.Windows.Application.GetResourceStream(uri);

                // 尝试加载图标
                if (resource?.Stream != null)
                {
                    // Icon 构造函数接受 Stream
                    _notifyIcon.Icon = new System.Drawing.Icon(resource.Stream);
                }
                else
                {
                    // 使用默认图标
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon.Text = "AI-Group";
            if (Debugger.IsAttached)
                _notifyIcon.Text = "AI-Group-Debug";
            _notifyIcon.Visible = true;

            // 创建上下文菜单
            var contextMenu = new ContextMenuStrip();

            // 开机启动菜单项
            var startupItem = new ToolStripMenuItem("开机启动");
            startupItem.Checked = IsStartupEnabled();
            startupItem.Click += (s, e) =>
            {
                startupItem.Checked = !startupItem.Checked;
                SetStartup(startupItem.Checked);
            };
            contextMenu.Items.Add(startupItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // 显示/隐藏窗口菜单项
            contextMenu.Items.Add("显示主窗口", null, (s, ea) => ShowMainWindow());
            contextMenu.Items.Add("退出", null, (s, ea) => ShutdownApplication());

            // 双击托盘图标显示窗口
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void ShowMainWindow()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;

            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                Dispatcher.Invoke(() =>
                {
                    if (_mainWindow.IsVisible)
                    {
                        _mainWindow.Hide();
                    }
                    else
                    {
                        ShowMainWindow();
                    }
                });
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void ShutdownApplication()
        {
            // 取消热键注册
            if (_mainWindowHandle != IntPtr.Zero)
            {
                HotKey.UnregisterHotKey(_mainWindowHandle, HOTKEY_ID);
            }

            // 清理托盘图标
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            // 清理消息钩子
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WndProc);
                _hwndSource = null;
            }

            // 关闭应用程序
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ShutdownApplication();
            base.OnExit(e);
        }

        //// 开机启动功能
        //private bool IsStartupEnabled()
        //{
        //    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
        //        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        //    {
        //        return key?.GetValue("AI-Group") != null;
        //    }
        //}

        //private void SetStartup(bool enable)
        //{

        //    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
        //        "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
        //    {
        //        if (enable)
        //        {
        //            key?.SetValue("AI-Group", System.Reflection.Assembly.GetExecutingAssembly().Location);
        //        }
        //        else
        //        {
        //            key?.DeleteValue("AI-Group", false);
        //        }
        //    }
        //}
        private string GetStartupShortcutPath()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            return Path.Combine(startupFolder, "AI-Group.lnk");
        }

        private bool IsStartupEnabled()
        {
            return System.IO.File.Exists(GetStartupShortcutPath());
        }

        private void SetStartup(bool enable)
        {
            string shortcutPath = GetStartupShortcutPath();
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            if (enable)
            {
                // 创建快捷方式
                var wshShell = new WshShell();
                IWshShortcut shortcut = wshShell.CreateShortcut(shortcutPath) as IWshShortcut;
                if (shortcut != null)
                {
                    shortcut.TargetPath = exePath;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                    shortcut.Description = "AI-Group Application";
                    shortcut.Save();
                }
            }
            else
            {
                // 删除快捷方式（如果存在）
                if (System.IO.File.Exists(shortcutPath))
                {
                    System.IO.File.Delete(shortcutPath);
                }
            }
        }

        public class HotKey
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            [Flags]
            public enum KeyModifiers
            {
                None = 0,
                Alt = 1,
                Ctrl = 2,
                Shift = 4,
                WindowsKey = 8
            }
        }
    }
}