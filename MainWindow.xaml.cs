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

    // GERENCIAMENTO DE ARQUIVOS

    private void ProcurarOrigem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select source folder"
        };

        if (dialog.ShowDialog() == true)
        {
            txtOrigem.Text = dialog.FolderName;
        }
    }

    private void ProcurarDestino_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select destination folder"
        };

        if (dialog.ShowDialog() == true)
        {
            txtDestino.Text = dialog.FolderName;
        }
    }

    private void MoverArquivos_Click(object sender, RoutedEventArgs e)
    {
        string origem = txtOrigem.Text;
        string extensao = txtExtensao.Text;
        string destino = txtDestino.Text;

        txtStatus.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(origem) || string.IsNullOrWhiteSpace(extensao) || string.IsNullOrWhiteSpace(destino))
        {
            txtStatus.Text = "Please fill in all fields.";
            txtStatus.Foreground = Brushes.Red;
            return;
        }

        // Normaliza a extensão para conter o ".". Ex: "txt" vira ".txt"
        if (!extensao.StartsWith("."))
        {
            extensao = "." + extensao;
        }

        try
        {
            if (!Directory.Exists(origem))
            {
                txtStatus.Text = "Source folder does not exist or is invalid.";
                txtStatus.Foreground = Brushes.Red;
                return;
            }

            // Busca todos os arquivos com a extensão fornecida
            string[] arquivos = Directory.GetFiles(origem, "*" + extensao);
            int contador = 0;
            int contadorDuplicatas = 0;

            foreach (var arquivo in arquivos)
            {
                string nomeArquivo = System.IO.Path.GetFileName(arquivo);
                string caminhoDestino = System.IO.Path.Combine(destino, nomeArquivo);

                // Mover o arquivo para o destino
                // Caso já exista no destino, você pode optar por criar uma exceção, ignorar, ou sobrescrever
                if (!File.Exists(caminhoDestino))
                {
                    File.Move(arquivo, caminhoDestino);
                    contador++;
                }
                else
                {
                    FileSystem.DeleteFile(arquivo, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    contadorDuplicatas++;
                }
            }
            if (contador <= 0 && contadorDuplicatas <= 0)
            {
                txtStatus.Text = "No files found with the specified extension.";
                txtStatus.Foreground = Brushes.Orange;
                return;
            }
            txtStatus.Text = $"{contador} file(s) moved and {contadorDuplicatas} duplicated file(s) moved to recycle bin.";
            txtStatus.Foreground = Brushes.Green;
        }
        catch (Exception ex)
        {
            txtStatus.Text = $"Error trying to move files: {ex.Message}";
            txtStatus.Foreground = Brushes.Red;
        }
    }

    // CONVERSOR DE TEMPERATURA

    private bool isUpdatingTemp = false;

    private void txtCelsius_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isUpdatingTemp) return;
        if (txtCelsius.IsFocused)
        {
            if (double.TryParse(txtCelsius.Text, out double c))
            {
                isUpdatingTemp = true;
                txtFahrenheit.Text = (c * 9.0 / 5.0 + 32).ToString("0.##");
                isUpdatingTemp = false;
            }
            else if (string.IsNullOrWhiteSpace(txtCelsius.Text))
            {
                LimparTemperaturas();
            }
        }
    }

    private void txtFahrenheit_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isUpdatingTemp) return;
        if (txtFahrenheit.IsFocused)
        {
            if (double.TryParse(txtFahrenheit.Text, out double f))
            {
                isUpdatingTemp = true;
                double c = (f - 32) * 5.0 / 9.0;
                txtCelsius.Text = c.ToString("0.##");
                isUpdatingTemp = false;
            }
            else if (string.IsNullOrWhiteSpace(txtFahrenheit.Text))
            {
                LimparTemperaturas();
            }
        }
    }

    private void LimparTemperaturas()
    {
        isUpdatingTemp = true;
        txtCelsius.Text = "";
        txtFahrenheit.Text = "";
        isUpdatingTemp = false;
    }

    // VÍDEO PARA GIF

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

            // Se o arquivo GIF já existir, exclui-o antes de tentar converter (ou pede para sobrescrever dependendo dos parâmetros na conversão)
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

            // Define os argumentos personalizados para a conversão de GIF com geração de paleta no mesmo comando usando o complex filter
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

    // CONVERSOR DE IMAGEM

    private string[]? arquivosImagensSelecionados;

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
                
                // Impede substituição acidental de um arquivo idêntico antes de terminar
                if (imagePath.Equals(outputPath, StringComparison.OrdinalIgnoreCase)) continue;

                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                var conversion = FFmpeg.Conversions.New()
                    .AddParameter($"-i \"{imagePath}\"");
                    
                if (formatoSelecionado == "jpeg")
                {
                   conversion.AddParameter("-q:v 2"); // Define qualidade de imagem para JPEG equivalente a -qscale:v
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