using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WPF_utils.Views
{
    public class AliasItem
    {
        public string AliasName { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
    }

    public partial class AliasManagerControl : UserControl
    {
        private string aliasManagerDir;
        private string aliasDir;
        private string jsonPath;
        private ObservableCollection<AliasItem> aliasesList = new ObservableCollection<AliasItem>();
        private bool isEditing = false;
        private AliasItem? currentEditingItem = null;

        public AliasManagerControl()
        {
            InitializeComponent();

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            aliasManagerDir = Path.Combine(localAppData, "AliasManager");
            aliasDir = Path.Combine(aliasManagerDir, "aliases");
            jsonPath = Path.Combine(aliasManagerDir, "aliases.json");

            EnsureDirectories();
            LoadAliases();
            lvAliases.ItemsSource = aliasesList;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CheckPathEnvironment();
            PrepareAddMode();
        }

        private void EnsureDirectories()
        {
            if (!Directory.Exists(aliasManagerDir)) Directory.CreateDirectory(aliasManagerDir);
            if (!Directory.Exists(aliasDir)) Directory.CreateDirectory(aliasDir);
        }

        private void CheckPathEnvironment()
        {
            string? pathVar = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (pathVar == null || !pathVar.Contains(aliasDir, StringComparison.OrdinalIgnoreCase))
            {
                txtWarningMessage.Text = $"Alias folder is not in your PATH. You can add it automatically or manually to your user PATH variable:\n{aliasDir}";
                BannerPathWarning.Visibility = Visibility.Visible;
            }
            else
            {
                BannerPathWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void AddToPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? pathVar = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                string newPath = string.IsNullOrWhiteSpace(pathVar) ? aliasDir : $"{pathVar};{aliasDir}";

                if (!newPath.EndsWith(";"))
                    newPath += ";";

                Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);

                MessageBox.Show("Successfully added to your user PATH environment variable! You may need to restart your terminal for changes to take effect.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                BannerPathWarning.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to modify PATH: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DismissWarning_Click(object sender, RoutedEventArgs e)
        {
            BannerPathWarning.Visibility = Visibility.Collapsed;
        }

        private void LoadAliases()
        {
            aliasesList.Clear();
            if (File.Exists(jsonPath))
            {
                try
                {
                    string json = File.ReadAllText(jsonPath);
                    var list = JsonSerializer.Deserialize<List<AliasItem>>(json);
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            aliasesList.Add(item);
                            EnsureAliasFiles(item);
                        }
                    }
                }
                catch { }
            }
        }

        private void SaveAliasesToFile()
        {
            try
            {
                var list = aliasesList.ToList();
                string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(jsonPath, json);
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Error saving aliases: " + ex.Message;
            }
        }

        private void PrepareAddMode()
        {
            isEditing = false;
            currentEditingItem = null;
            txtFormTitle.Text = "Create Alias";
            txtAliasName.Text = "";
            txtCommand.Text = "";
            txtAliasName.IsReadOnly = false;
            btnDelete.IsEnabled = false;
            txtStatus.Text = "";
            lvAliases.SelectedItem = null;
        }

        private void PrepareEditMode(AliasItem item)
        {
            isEditing = true;
            currentEditingItem = item;
            txtFormTitle.Text = "Edit Alias";
            txtAliasName.Text = item.AliasName;
            txtCommand.Text = item.Command;
            txtAliasName.IsReadOnly = true; 
            btnDelete.IsEnabled = true;
            txtStatus.Text = "";
        }

        private void AddAlias_Click(object sender, RoutedEventArgs e)
        {
            PrepareAddMode();
        }

        private void lvAliases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvAliases.SelectedItem is AliasItem item)
            {
                PrepareEditMode(item);
            }
        }

        private void SaveAlias_Click(object sender, RoutedEventArgs e)
        {
            string name = txtAliasName.Text.Trim();
            string command = txtCommand.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(command))
            {
                txtStatus.Text = "Alias name and command cannot be empty.";
                return;
            }

            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\-]+$"))
            {
                txtStatus.Text = "Alias name can only contain alphanumeric characters and hyphens.";
                return;
            }

            if (!isEditing && aliasesList.Any(a => a.AliasName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                txtStatus.Text = "An alias with this name already exists.";
                return;
            }

            if (isEditing && currentEditingItem != null)
            {
                currentEditingItem.Command = command;
                EnsureAliasFiles(currentEditingItem);

                var view = System.Windows.Data.CollectionViewSource.GetDefaultView(aliasesList);
                view.Refresh();

                lvAliases.SelectedItem = currentEditingItem;
            }
            else
            {
                var newItem = new AliasItem { AliasName = name, Command = command };
                aliasesList.Add(newItem);
                EnsureAliasFiles(newItem);
                lvAliases.SelectedItem = newItem;
            }

            SaveAliasesToFile();
            txtStatus.Text = ""; 
        }

        private void DeleteAlias_Click(object sender, RoutedEventArgs e)
        {
            if (!isEditing || currentEditingItem == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete the alias '{currentEditingItem.AliasName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                RemoveAliasFiles(currentEditingItem.AliasName);
                aliasesList.Remove(currentEditingItem);
                SaveAliasesToFile();
                PrepareAddMode();
            }
        }

        private void EnsureAliasFiles(AliasItem item)
        {
            try
            {
                string batPath = Path.Combine(aliasDir, $"{item.AliasName}.bat");
                string batContent = $"@echo off\n{item.Command} %*";
                File.WriteAllText(batPath, batContent);

                string shPath = Path.Combine(aliasDir, item.AliasName);
                string shContent = $"#!/bin/sh\n{item.Command} \"$@\"";
                File.WriteAllText(shPath, shContent);
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Failed to write wrapper files for {item.AliasName}: {ex.Message}";
            }
        }

        private void RemoveAliasFiles(string aliasName)
        {
            try
            {
                string batPath = Path.Combine(aliasDir, $"{aliasName}.bat");
                if (File.Exists(batPath)) File.Delete(batPath);

                string shPath = Path.Combine(aliasDir, aliasName);
                if (File.Exists(shPath)) File.Delete(shPath);
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Failed to delete wrapper files for {aliasName}: {ex.Message}";
            }
        }
    }
}