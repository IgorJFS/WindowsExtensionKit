using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualBasic.FileIO;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using System.Threading.Tasks;

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
            Title = "Selecione a pasta de origem"
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
            Title = "Selecione a pasta de destino"
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
            txtStatus.Text = "Por favor, preencha todos os campos.";
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
                txtStatus.Text = "A pasta de origem não existe ou é inválida.";
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
                txtStatus.Text = "Nenhum arquivo encontrado com a extensão especificada.";
                txtStatus.Foreground = Brushes.Orange;
                return;
            }
            txtStatus.Text = $"{contador} arquivo(s) movido(s) e {contadorDuplicatas} arquivo(s) duplicado(s) movidos para a lixeira.";
            txtStatus.Foreground = Brushes.Green;
        }
        catch (Exception ex)
        {
            txtStatus.Text = $"Erro ao tentar mover arquivos: {ex.Message}";
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
            Title = "Selecione o arquivo de vídeo",
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
            txtGifStatus.Text = "Por favor, selecione um arquivo de vídeo válido.";
            txtGifStatus.Foreground = Brushes.Red;
            return;
        }

        if (!int.TryParse(txtFps.Text, out int fps) || fps <= 0)
        {
            txtGifStatus.Text = "Por favor, insira um valor válido para FPS.";
            txtGifStatus.Foreground = Brushes.Red;
            return;
        }

        if (!int.TryParse(txtScale.Text, out int scale) || scale <= 0)
        {
            txtGifStatus.Text = "Por favor, insira um valor válido para o Scale (largura).";
            txtGifStatus.Foreground = Brushes.Red;
            return;
        }

        string outputPath = System.IO.Path.ChangeExtension(videoPath, ".gif");

        try
        {
            txtGifStatus.Text = "Convertendo... Aguarde.";
            txtGifStatus.Foreground = Brushes.Orange;

            // Se o arquivo GIF já existir, exclui-o antes de tentar converter (ou pede para sobrescrever dependendo dos parâmetros na conversão)
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

            // Define os argumentos personalizados para a conversão de GIF
            string customArgs = $"-vf \"fps={fps},scale={scale}:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\"";

            var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{videoPath}\"")
                .AddParameter(customArgs)
                .SetOutput(outputPath);

            await conversion.Start();

            txtGifStatus.Text = $"Conversão concluída com sucesso!\nSalvo em: {outputPath}";
            txtGifStatus.Foreground = Brushes.Green;
        }
        catch (Exception ex)
        {
            txtGifStatus.Text = $"Erro durante a conversão: {ex.Message}";
            txtGifStatus.Foreground = Brushes.Red;
        }
    }
}