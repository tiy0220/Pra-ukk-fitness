using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using fitnesss.Data;
using fitnesss.Models;

namespace fitnesss.Views
{
    public partial class AdminWindow : Window
    {
        private readonly DatabaseService _dbService = new DatabaseService();
        private readonly UserModel _currentUser;

        public AdminWindow()
        {
            InitializeComponent();
            _currentUser = new UserModel { Id = 1, Nama = "Administrator", Role = "admin" };
            TxtAdminName.Text = _currentUser.Nama;
            LoadData();
        }

        public AdminWindow(UserModel user)
        {
            InitializeComponent();
            _currentUser = user;
            TxtAdminName.Text = _currentUser.Nama;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DgAdminProducts.ItemsSource = _dbService.GetProducts();
                DgAdminUsers.ItemsSource = _dbService.GetUsers();
            }
            catch (Exception ex)
            {
                TxtStatusProduk.Text = $"Gagal memuat data: {ex.Message}";
                TxtStatusProduk.Foreground = new SolidColorBrush(Color.Parse("#DC2626"));
            }
        }

        // --- CRUD PRODUK ---
        private void BtnSimpanProduk_Click(object sender, RoutedEventArgs e)
        {
            string nama = TxtNamaProduk.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(nama))
            {
                ShowStatusProduk("Nama produk tidak boleh kosong!", true);
                return;
            }

            if (!int.TryParse(TxtHargaProduk.Text?.Replace(".", "").Replace(",", "").Trim(), out int harga) || harga < 0)
            {
                ShowStatusProduk("Harga produk harus berupa angka valid!", true);
                return;
            }

            try
            {
                _dbService.AddProduct(nama, harga, _currentUser.Id);
                ShowStatusProduk($"Produk '{nama}' berhasil ditambahkan!", false);
                BtnResetProduk_Click(sender, e);
                LoadData();
            }
            catch (Exception ex)
            {
                ShowStatusProduk($"Gagal menyimpan: {ex.Message}", true);
            }
        }

        private void BtnUpdateProduk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtIdProduk.Text, out int id) || id <= 0)
            {
                ShowStatusProduk("Pilih produk dari tabel terlebih dahulu!", true);
                return;
            }

            string nama = TxtNamaProduk.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(nama))
            {
                ShowStatusProduk("Nama produk tidak boleh kosong!", true);
                return;
            }

            if (!int.TryParse(TxtHargaProduk.Text?.Replace(".", "").Replace(",", "").Trim(), out int harga) || harga < 0)
            {
                ShowStatusProduk("Harga produk harus berupa angka valid!", true);
                return;
            }

            try
            {
                _dbService.UpdateProduct(id, nama, harga, _currentUser.Id);
                ShowStatusProduk($"Produk ID {id} berhasil diupdate!", false);
                BtnResetProduk_Click(sender, e);
                LoadData();
            }
            catch (Exception ex)
            {
                ShowStatusProduk($"Gagal update: {ex.Message}", true);
            }
        }

        private void BtnHapusProduk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtIdProduk.Text, out int id) || id <= 0)
            {
                ShowStatusProduk("Pilih produk dari tabel yang ingin dihapus!", true);
                return;
            }

            string nama = TxtNamaProduk.Text ?? "Produk";

            try
            {
                _dbService.DeleteProduct(id, nama, _currentUser.Id);
                ShowStatusProduk($"Produk '{nama}' berhasil dihapus!", false);
                BtnResetProduk_Click(sender, e);
                LoadData();
            }
            catch (Exception ex)
            {
                ShowStatusProduk($"Gagal menghapus: {ex.Message}", true);
            }
        }

        private void BtnResetProduk_Click(object sender, RoutedEventArgs e)
        {
            TxtIdProduk.Text = string.Empty;
            TxtNamaProduk.Text = string.Empty;
            TxtHargaProduk.Text = string.Empty;
            DgAdminProducts.SelectedItem = null;
        }

        private void DgAdminProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgAdminProducts.SelectedItem is ProductModel p)
            {
                TxtIdProduk.Text = p.Id.ToString();
                TxtNamaProduk.Text = p.NamaProduk;
                TxtHargaProduk.Text = p.HargaProduk.ToString();
                TxtStatusProduk.Text = string.Empty;
            }
        }

        private void ShowStatusProduk(string msg, bool isError)
        {
            TxtStatusProduk.Text = msg;
            TxtStatusProduk.Foreground = isError ? new SolidColorBrush(Color.Parse("#DC2626")) : new SolidColorBrush(Color.Parse("#16A34A"));
        }

        // --- CRUD USER ---
        private void BtnSimpanUser_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsernameUser.Text?.Trim() ?? string.Empty;
            string password = TxtPasswordUser.Text?.Trim() ?? string.Empty;
            string nama = TxtNamaUser.Text?.Trim() ?? string.Empty;
            string role = (CbRoleUser.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "kasir";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nama))
            {
                ShowStatusUser("Semua kolom user wajib diisi!", true);
                return;
            }

            try
            {
                var newUser = new UserModel
                {
                    Username = username,
                    Password = password,
                    Nama = nama,
                    Role = role
                };
                _dbService.AddUser(newUser, _currentUser.Id);
                ShowStatusUser($"User '{username}' berhasil disimpan!", false);
                BtnResetUser_Click(sender, e);
                LoadData();
            }
            catch (Exception ex)
            {
                ShowStatusUser($"Gagal menyimpan user: {ex.Message}", true);
            }
        }

        private void BtnUpdateUser_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtIdUser.Text, out int id) || id <= 0)
            {
                ShowStatusUser("Pilih user dari tabel terlebih dahulu!", true);
                return;
            }

            string username = TxtUsernameUser.Text?.Trim() ?? string.Empty;
            string password = TxtPasswordUser.Text?.Trim() ?? string.Empty;
            string nama = TxtNamaUser.Text?.Trim() ?? string.Empty;
            string role = (CbRoleUser.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "kasir";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(nama))
            {
                ShowStatusUser("Username dan Nama Lengkap tidak boleh kosong!", true);
                return;
            }

            try
            {
                var user = new UserModel
                {
                    Id = id,
                    Username = username,
                    Password = password,
                    Nama = nama,
                    Role = role
                };
                _dbService.UpdateUser(user, _currentUser.Id);
                ShowStatusUser($"User '{username}' berhasil diupdate!", false);
                BtnResetUser_Click(sender, e);
                LoadData();
            }
            catch (Exception ex)
            {
                ShowStatusUser($"Gagal update user: {ex.Message}", true);
            }
        }

        private void BtnHapusUser_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtIdUser.Text, out int id) || id <= 0)
            {
                ShowStatusUser("Pilih user dari tabel yang ingin dinonaktifkan!", true);
                return;
            }

            if (id == _currentUser.Id)
            {
                ShowStatusUser("Anda tidak dapat menghapus akun Anda sendiri yang sedang aktif!", true);
                return;
            }

            string username = TxtUsernameUser.Text ?? "User";

            try
            {
                _dbService.DeleteUser(id, username, _currentUser.Id);
                ShowStatusUser($"User '{username}' berhasil dinonaktifkan/dihapus!", false);
                BtnResetUser_Click(sender, e);
                LoadData();
            }
            catch (Exception ex)
            {
                ShowStatusUser($"Gagal menghapus user: {ex.Message}", true);
            }
        }

        private void BtnResetUser_Click(object sender, RoutedEventArgs e)
        {
            TxtIdUser.Text = string.Empty;
            TxtUsernameUser.Text = string.Empty;
            TxtPasswordUser.Text = string.Empty;
            TxtNamaUser.Text = string.Empty;
            CbRoleUser.SelectedIndex = 1; // Default kasir
            DgAdminUsers.SelectedItem = null;
        }

        private void DgAdminUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgAdminUsers.SelectedItem is UserModel u)
            {
                TxtIdUser.Text = u.Id.ToString();
                TxtUsernameUser.Text = u.Username;
                TxtPasswordUser.Text = u.Password;
                TxtNamaUser.Text = u.Nama;

                for (int i = 0; i < CbRoleUser.Items.Count; i++)
                {
                    if ((CbRoleUser.Items[i] as ComboBoxItem)?.Content?.ToString()?.Equals(u.Role, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        CbRoleUser.SelectedIndex = i;
                        break;
                    }
                }
                TxtStatusUser.Text = string.Empty;
            }
        }

        private void ShowStatusUser(string msg, bool isError)
        {
            TxtStatusUser.Text = msg;
            TxtStatusUser.Foreground = isError ? new SolidColorBrush(Color.Parse("#DC2626")) : new SolidColorBrush(Color.Parse("#16A34A"));
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            _dbService.LogActivity(_currentUser.Id, $"Administrator '{_currentUser.Username}' logout.");
            var login = new LoginWindow();
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = login;
            }
            login.Show();
            this.Close();
        }
    }
}
