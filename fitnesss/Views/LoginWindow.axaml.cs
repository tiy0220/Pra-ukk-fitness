using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using fitnesss.Data;
using fitnesss.Models;

namespace fitnesss.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseService _dbService = new DatabaseService();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text?.Trim() ?? string.Empty;
            string password = TxtPassword.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Username dan Password tidak boleh kosong!");
                return;
            }

            UserModel? user = null;
            try
            {
                user = _dbService.Login(username, password);
            }
            catch (Exception ex)
            {
                ShowError($"Gagal koneksi MySQL! Pastikan XAMPP MySQL aktif.\n({ex.Message})");
                return;
            }

            if (user != null)
            {
                HideError();

                // Catat log aktivitas login
                _dbService.LogActivity(user.Id, $"Pengguna '{user.Username}' ({user.Role}) berhasil login.");

                // Navigasi berdasarkan Role
                Window? nextWindow = user.Role.ToLowerInvariant() switch
                {
                    "kasir" => new KasirWindow(user),
                    "admin" => new AdminWindow(user),
                    "owner" => new OwnerWindow(user),
                    _ => null
                };

                if (nextWindow != null)
                {
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.MainWindow = nextWindow;
                    }
                    nextWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError($"Role '{user.Role}' tidak dikenali oleh sistem.");
                }
            }
            else
            {
                ShowError("Username atau Password salah! Periksa kembali akun Anda.");
            }
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            BorderError.IsVisible = true;
        }

        private void HideError()
        {
            BorderError.IsVisible = false;
        }
    }
}
