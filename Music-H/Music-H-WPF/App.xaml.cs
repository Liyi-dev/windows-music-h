using System.Configuration;
using System.Data;
using System.Windows;

namespace Music_H_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 捕获 UI 线程的异常
            this.DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"程序发生错误：\n{args.Exception.Message}\n\n详细信息：\n{args.Exception.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true; // 阻止程序直接崩溃
            };

            // 捕获非 UI 线程的异常
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"严重错误：\n{ex.Message}", "致命错误", MessageBoxButton.OK, MessageBoxImage.Stop);
            };

            base.OnStartup(e);
        }
    }
}
