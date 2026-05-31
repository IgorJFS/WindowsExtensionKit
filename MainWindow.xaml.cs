using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualBasic.FileIO;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using System.Threading.Tasks;
using System.Linq;

namespace WPF_utils;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeFFmpegAsync();
    }

    private void AppMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MainTabControl != null && AppMenu != null)
        {
            MainTabControl.SelectedIndex = AppMenu.SelectedIndex;
        }
    }

    private async void InitializeFFmpegAsync()
    {
        // Baixa os binários do FFmpeg caso não existam no diretório
        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
        FFmpeg.SetExecutablesPath(".");
    }
}