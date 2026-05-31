using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualBasic.FileIO;

namespace WPF_utils.Views
{
    public partial class FileOrganizerControl : UserControl
    {
        public FileOrganizerControl()
        {
            InitializeComponent();
        }

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

                if (!Directory.Exists(destino))
                {
                    Directory.CreateDirectory(destino);
                }

                string[] arquivos = Directory.GetFiles(origem, "*" + extensao);
                int contador = 0;
                int contadorDuplicatas = 0;

                foreach (var arquivo in arquivos)
                {
                    string nomeArquivo = System.IO.Path.GetFileName(arquivo);
                    string caminhoDestino = System.IO.Path.Combine(destino, nomeArquivo);

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
    }
}