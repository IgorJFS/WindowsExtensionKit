using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xabe.FFmpeg;

namespace WPF_utils.Views
{
    public partial class ImageConverterControl : UserControl
    {
        private string[]? arquivosImagensSelecionados;

        public ImageConverterControl()
        {
            InitializeComponent();
        }

        private void ProcurarImagens_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select images",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.webp;*.bmp|All Files|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                arquivosImagensSelecionados = dialog.FileNames;
                txtImagesPath.Text = string.Join(" | ", dialog.FileNames.Select(f => System.IO.Path.GetFileName(f)));
                txtImageStatus.Text = string.Empty;
            }
        }

        private async void ConverterImagem_Click(object sender, RoutedEventArgs e)
        {
            if (arquivosImagensSelecionados == null || arquivosImagensSelecionados.Length == 0)
            {
                txtImageStatus.Text = "Please select at least one valid image.";
                txtImageStatus.Foreground = Brushes.Red;
                return;
            }

            string formatoSelecionado = cmbFormatoImagem.Text.ToLower();

            txtImageStatus.Text = "Converting images... Please wait.";
            txtImageStatus.Foreground = Brushes.Orange;

            int sucesso = 0;
            int falha = 0;

            foreach (var imagePath in arquivosImagensSelecionados)
            {
                try
                {
                    if (!File.Exists(imagePath)) continue;

                    string extension = $".{formatoSelecionado}";
                    string outputPath = System.IO.Path.ChangeExtension(imagePath, extension);

                    if (imagePath.Equals(outputPath, StringComparison.OrdinalIgnoreCase)) continue;

                    if (File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }

                    var conversion = FFmpeg.Conversions.New()
                        .AddParameter($"-i \"{imagePath}\"");

                    if (formatoSelecionado == "jpeg")
                    {
                       conversion.AddParameter("-q:v 2"); 
                    }

                    conversion.SetOutput(outputPath);

                    await conversion.Start();
                    sucesso++;
                }
                catch (Exception)
                {
                    falha++;
                }
            }

            if (falha == 0)
            {
                txtImageStatus.Text = $"Conversion completed! {sucesso} image(s) processed.";
                txtImageStatus.Foreground = Brushes.Green;

            }
            else if (sucesso == 0)
            {
                txtImageStatus.Text = $"Conversion failed for all images.";
                txtImageStatus.Foreground = Brushes.Red;
            }
            else
            {
                txtImageStatus.Text = $"Completed with errors. {sucesso} success(es), {falha} failure(s).";
                txtImageStatus.Foreground = Brushes.Orange;
            }
            txtImagesPath.Text = string.Empty;
            arquivosImagensSelecionados = null;
        }
    }
}