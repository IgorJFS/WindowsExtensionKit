using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xabe.FFmpeg;

namespace WPF_utils.Views
{
    public partial class VideoToGifControl : UserControl
    {
        public VideoToGifControl()
        {
            InitializeComponent();
        }

        private void ProcurarVideo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select a video file",
                Filter = "Video Files|*.mp4;*.avi;*.mkv;*.mov;*.wmv|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                txtVideoPath.Text = dialog.FileName;
            }
        }

        private async void ConverterParaGif_Click(object sender, RoutedEventArgs e)
        {
            string videoPath = txtVideoPath.Text;
            txtGifStatus.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(videoPath) || !File.Exists(videoPath))
            {
                txtGifStatus.Text = "Please select a valid video file.";
                txtGifStatus.Foreground = Brushes.Red;
                return;
            }

            if (!int.TryParse(txtFps.Text, out int fps) || fps <= 0)
            {
                txtGifStatus.Text = "Please enter a valid FPS value.";
                txtGifStatus.Foreground = Brushes.Red;
                return;
            }

            if (!int.TryParse(txtScale.Text, out int scale) || scale <= 0)
            {
                txtGifStatus.Text = "Please enter a valid scale value (width).";
                txtGifStatus.Foreground = Brushes.Red;
                return;
            }

            string outputPath = System.IO.Path.ChangeExtension(videoPath, ".gif");

            try
            {
                txtGifStatus.Text = "Converting... Please wait.";
                txtGifStatus.Foreground = Brushes.Orange;

                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

                string customArgs = $"-vf \"fps={fps},scale={scale}:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\"";

                var conversion = FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{videoPath}\"")
                    .AddParameter(customArgs)
                    .SetOutput(outputPath);

                await conversion.Start();

                txtGifStatus.Text = $"Conversion successfully completed!\nSaved at: {outputPath}";
                txtGifStatus.Foreground = Brushes.Green;
            }
            catch (Exception ex)
            {
                txtGifStatus.Text = $"Error during conversion: {ex.Message}";
                txtGifStatus.Foreground = Brushes.Red;
            }
        }
    }
}