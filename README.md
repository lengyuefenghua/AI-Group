# AI-Group

## 项目简介

AI-Group 是基于 WPF 开发的桌面应用程序，旨在整合主流 AI 聊天工具（如 DeepSeek、Gemini、豆包、千问等），通过统一界面提供便捷的访问入口。用户可通过左侧按钮快速切换不同 AI 工具，支持对集成的 AI 工具进行添加、删除、关闭等管理操作，简化多 AI 工具的使用流程。

## 安装方法

### 运行要求

- 系统要求：Windows 1903+
- 环境要求：安装.NET Framework 4.6.2+（默认自带）（[下载地址](https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net462)）

#### 方式1：直接运行

1. 前往 [Releases页面]((https://github.com/lengyuefenghua/AI-Group/releases))
2. 下载最新版本的`AI-Group.zip`
3. 解压到任意文件夹（比如桌面）
4. 双击`AI-Group.exe`，无需安装，直接启动

#### 方式2：源码编译

1. 克隆仓库：`https://github.com/lengyuefenghua/AI-Group.git`
2. 用Visual Studio 2019+打开`AI-Group.csproj`
3. 右键「还原NuGet包」→ 「生成解决方案」
4. 在`bin/Debug`目录找到`AI-Group.exe`运行

## 使用教程

1. 启动后隐藏在系统托盘，通过快捷键<kbd>Alt</kbd>+<kbd>`</kbd>或双击托盘图标<span style=" color: #007fff; padding: 2px 5px;">AI</span>打开主窗口右键托盘图标<span style=" color: #007fff; padding: 2px 5px;">AI</span>可以选择设置开机启动，打开主窗口或退出
2. 主页面点击图标即可打开对应AI网页端，若登陆账号，只需登陆一次，后续会记住登陆信息（浏览器组件自带功能，和浏览器登陆相同）
3. 每次启动第一次打开某个AI需要加载webview2，加载事件和网络及电脑性能有关，后续切换不需要等待
4. 右键按钮可选择「删除」「关闭」[打开]等管理操作
5. 点击最下方加号按钮科添加自定义AI，输入名称及网址后会自动获取图标，也可以选择本地图标上传

## 界面展示

### 主页面

![主页面](https://github.com/lengyuefenghua/Storage/blob/main/Images/AI-Group1.png)

## 许可证

MIT