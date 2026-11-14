using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using fitnesss.Data;
using fitnesss.Models;

namespace fitnesss.Views
{
    public partial class KasirWindow : Window
    {
        private readonly DatabaseService _dbService = new DatabaseService();
        private readonly UserModel _currentUser;

        private List<ProductModel> _allProducts = new();
        private List<ProductModel> _filteredProducts = new();
        private readonly List<CartItemModel> _cartItems = new();

        private bool _isMemberActive = false;
        private int _dailyVisitsToday = 0;
        private int _totalTagihan = 0;

        public KasirWindow()
        {
            InitializeComponent();
            _currentUser = new UserModel { Id = 2, Nama = "Kasir Utama", Role = "kasir" };
            TxtKasirName.Text = _currentUser.Nama;
            LoadProducts();
        }

        public KasirWindow(UserModel user)
        {
            InitializeComponent();
            _currentUser = user;
            TxtKasirName.Text = _currentUser.Nama;
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                _allProducts = _dbService.GetProducts();
                _filteredProducts = new List<ProductModel>(_allProducts);
                IcProductCards.ItemsSource = _filteredProducts;
            }
            catch (Exception ex)
            {
                TxtErrorKasir.Text = $"Gagal memuat katalog produk: {ex.Message}";
            }
        }

        // --- PENCARIAN PRODUK ---
        private void TxtSearchProduct_TextChanged(object sender, TextChangedEventArgs e)
        {
            string q = TxtSearchProduct.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrEmpty(q))
            {
                _filteredProducts = new List<ProductModel>(_allProducts);
            }
            else
            {
                _filteredProducts = _allProducts.Where(p => p.NamaProduk.ToLowerInvariant().Contains(q)).ToList();
            }
            IcProductCards.ItemsSource = _filteredProducts;
        }

        // --- TAMBAH PRODUK KE KERANJANG (PENCET LANGSUNG ADD) ---
        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ProductModel p)
            {
                var existing = _cartItems.FirstOrDefault(c => c.ProductId == p.Id);
                if (existing != null)
                {
                    existing.Qty++;
                }
                else
                {
                    _cartItems.Add(new CartItemModel
                    {
                        ProductId = p.Id,
                        ProductName = p.NamaProduk,
                        UnitPrice = p.HargaProduk,
                        Qty = 1
                    });
                }
                RecalculateCart();
            }
        }

        // --- QTY & ITEM CONTROLS DI KERANJANG ---
        private void BtnQtyPlus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItemModel item)
            {
                item.Qty++;
                RecalculateCart();
            }
        }

        private void BtnQtyMinus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItemModel item)
            {
                if (item.Qty > 1)
                {
                    item.Qty--;
                }
                else
                {
                    _cartItems.Remove(item);
                }
                RecalculateCart();
            }
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CartItemModel item)
            {
                _cartItems.Remove(item);
                RecalculateCart();
            }
        }

        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            _cartItems.Clear();
            RecalculateCart();
        }

        // --- CEK MEMBERSHIP & KUOTA HARIAN ---
        private void TxtNomorUnik_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckMembershipStatus();
        }

        private void BtnCekMember_Click(object sender, RoutedEventArgs e)
        {
            CheckMembershipStatus();
        }

        private void CheckMembershipStatus()
        {
            string nomorUnik = TxtNomorUnik.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(nomorUnik))
            {
                TxtStatusBadge.Text = "Status: Masukkan nomor unik / ID member untuk cek membership";
                TxtStatusBadge.Foreground = new SolidColorBrush(Color.Parse("#475569"));
                TxtKuotaHarian.Text = "* Gym Harian Gratis 1x/hari khusus Member terdaftar";
                TxtKuotaHarian.Foreground = new SolidColorBrush(Color.Parse("#64748B"));
                BorderMemberStatus.Background = new SolidColorBrush(Color.Parse("#F1F5F9"));
                BorderMemberStatus.BorderBrush = new SolidColorBrush(Color.Parse("#CBD5E1"));
                _isMemberActive = false;
                _dailyVisitsToday = 0;
                RecalculateCart();
                return;
            }

            try
            {
                _isMemberActive = _dbService.IsActiveMember(nomorUnik, out string? memberName, out DateTime? validUntil);
                _dailyVisitsToday = _dbService.GetDailyGymVisitsToday(nomorUnik);

                // Auto isi nama jika member atau pernah bertransaksi
                if (!string.IsNullOrEmpty(memberName) && string.IsNullOrEmpty(TxtNamaPelanggan.Text?.Trim()))
                {
                    TxtNamaPelanggan.Text = memberName;
                }
                else if (string.IsNullOrEmpty(TxtNamaPelanggan.Text?.Trim()))
                {
                    string? prevName = _dbService.GetLatestCustomerName(nomorUnik);
                    if (!string.IsNullOrEmpty(prevName))
                    {
                        TxtNamaPelanggan.Text = prevName;
                    }
                }

                if (_isMemberActive)
                {
                    string sExp = validUntil.HasValue ? validUntil.Value.ToString("dd MMM yyyy") : "-";
                    TxtStatusBadge.Text = $"✅ MEMBER AKTIF a.n {memberName ?? "Member"} (s/d {sExp})";
                    TxtStatusBadge.Foreground = new SolidColorBrush(Color.Parse("#047857"));
                    BorderMemberStatus.Background = new SolidColorBrush(Color.Parse("#ECFDF5"));
                    BorderMemberStatus.BorderBrush = new SolidColorBrush(Color.Parse("#6EE7B7"));

                    if (_dailyVisitsToday == 0)
                    {
                        TxtKuotaHarian.Text = "🎉 Kuota Harian Hari Ini: GRATIS (Tersedia 1x hari ini)";
                        TxtKuotaHarian.Foreground = new SolidColorBrush(Color.Parse("#059669"));
                    }
                    else
                    {
                        TxtKuotaHarian.Text = $"⚠️ Kuota gratis 1x hari ini sudah dipakai ({_dailyVisitsToday}x kunjungan). Input berikutnya berbayar normal.";
                        TxtKuotaHarian.Foreground = new SolidColorBrush(Color.Parse("#D97706"));
                    }
                }
                else
                {
                    TxtStatusBadge.Text = "ℹ️ Pelanggan Reguler / Non-Member";
                    TxtStatusBadge.Foreground = new SolidColorBrush(Color.Parse("#334155"));
                    TxtKuotaHarian.Text = "* Member baru dapat mendaftar dengan memilih produk 'Member Bulanan'";
                    TxtKuotaHarian.Foreground = new SolidColorBrush(Color.Parse("#64748B"));
                    BorderMemberStatus.Background = new SolidColorBrush(Color.Parse("#F8FAFC"));
                    BorderMemberStatus.BorderBrush = new SolidColorBrush(Color.Parse("#E2E8F0"));
                }

                RecalculateCart();
            }
            catch (Exception ex)
            {
                TxtErrorKasir.Text = $"Gagal cek status member: {ex.Message}";
            }
        }

        // --- KALKULASI HARGA KERANJANG & ATURAN MEMBER GRATIS 1X ---
        private void RecalculateCart()
        {
            TxtErrorKasir.Text = string.Empty;

            foreach (var item in _cartItems)
            {
                bool isDailyGym = item.ProductName.IndexOf("Harian", StringComparison.OrdinalIgnoreCase) >= 0;

                if (isDailyGym && _isMemberActive)
                {
                    if (_dailyVisitsToday == 0)
                    {
                        // 1x gratis hari ini
                        if (item.Qty == 1)
                        {
                            item.Subtotal = 0;
                            item.IsFreeMemberBenefit = true;
                            item.Note = "* Kuota Harian Member: GRATIS (1x)";
                        }
                        else
                        {
                            // Lebih dari 1x input: 1 gratis, sisanya bayar
                            item.Subtotal = (item.Qty - 1) * item.UnitPrice;
                            item.IsFreeMemberBenefit = true;
                            item.Note = $"* 1x Gratis Kuota Member, {item.Qty - 1}x Bayar Normal";
                        }
                    }
                    else
                    {
                        // Jatah gratis 1x hari ini sudah dipakai
                        item.Subtotal = item.Qty * item.UnitPrice;
                        item.IsFreeMemberBenefit = false;
                        item.Note = "* Kuota gratis hari ini sudah habis";
                    }
                }
                else
                {
                    item.Subtotal = item.Qty * item.UnitPrice;
                    item.IsFreeMemberBenefit = false;
                    item.Note = string.Empty;
                }
            }

            _totalTagihan = _cartItems.Sum(i => i.Subtotal);
            TxtTotalTagihan.Text = $"Rp {_totalTagihan:N0}";

            // Jika total tagihan 0 (misal hanya 1x gym harian gratis member), auto isi 0
            if (_totalTagihan == 0 && _cartItems.Count > 0)
            {
                TxtUangBayar.Text = "0";
            }

            HitungKembalian();

            // Refresh UI ItemsControl
            IcCartItems.ItemsSource = null;
            IcCartItems.ItemsSource = _cartItems;
        }

        private void TxtUangBayar_TextChanged(object sender, TextChangedEventArgs e)
        {
            HitungKembalian();
        }

        private void HitungKembalian()
        {
            if (int.TryParse(TxtUangBayar.Text?.Replace(".", "").Replace(",", "").Trim(), out int uangBayar))
            {
                int kembalian = uangBayar - _totalTagihan;
                if (kembalian >= 0)
                {
                    LblUangKembali.Text = $"Rp {kembalian:N0}";
                    LblUangKembali.Foreground = new SolidColorBrush(Color.Parse("#059669"));
                }
                else
                {
                    LblUangKembali.Text = $"Kurang Rp {Math.Abs(kembalian):N0}!";
                    LblUangKembali.Foreground = new SolidColorBrush(Color.Parse("#DC2626"));
                }
            }
            else
            {
                LblUangKembali.Text = "Rp 0";
                LblUangKembali.Foreground = new SolidColorBrush(Color.Parse("#059669"));
            }
        }

        // --- CHECKOUT / SIMPAN TRANSAKSI & CETAK STRUK MULTI-ITEM ---
        private void BtnSimpanTrans_Click(object sender, RoutedEventArgs e)
        {
            TxtErrorKasir.Text = string.Empty;

            if (_cartItems.Count == 0)
            {
                TxtErrorKasir.Text = "Keranjang belanja masih kosong! Silakan klik '+ Tambah' pada produk terlebih dahulu.";
                return;
            }

            string namaPelanggan = TxtNamaPelanggan.Text?.Trim() ?? string.Empty;
            string nomorUnik = TxtNomorUnik.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(nomorUnik))
            {
                TxtErrorKasir.Text = "Nomor Unik / ID Member wajib diisi!";
                return;
            }

            if (string.IsNullOrEmpty(namaPelanggan))
            {
                TxtErrorKasir.Text = "Nama pelanggan wajib diisi!";
                return;
            }

            int uangBayar = 0;
            if (!string.IsNullOrEmpty(TxtUangBayar.Text))
            {
                int.TryParse(TxtUangBayar.Text.Replace(".", "").Replace(",", "").Trim(), out uangBayar);
            }

            int uangKembali = uangBayar - _totalTagihan;
            if (uangKembali < 0)
            {
                TxtErrorKasir.Text = "Uang pembayaran tunai kurang dari total tagihan!";
                return;
            }

            try
            {
                int primaryTrxId = 0;

                // Simpan setiap item di keranjang ke tabel transactions
                for (int i = 0; i < _cartItems.Count; i++)
                {
                    var item = _cartItems[i];
                    bool isLastItem = (i == _cartItems.Count - 1);

                    var trx = new TransactionModel
                    {
                        IdProduk = item.ProductId,
                        NamaPelanggan = namaPelanggan,
                        NomorUnik = nomorUnik,
                        UangBayar = isLastItem ? (uangBayar - (_totalTagihan - item.Subtotal)) : item.Subtotal,
                        UangKembali = isLastItem ? uangKembali : 0
                    };

                    int savedId = _dbService.AddTransaction(trx, _currentUser.Id);
                    if (primaryTrxId == 0) primaryTrxId = savedId;
                }

                // Tampilkan Struk Bukti Transaksi Multi-Item
                var struk = new StrukWindow(
                    primaryTrxId,
                    _currentUser.Nama,
                    namaPelanggan,
                    nomorUnik,
                    new List<CartItemModel>(_cartItems),
                    _totalTagihan,
                    uangBayar,
                    uangKembali
                );
                struk.ShowDialog(this);

                // Reset Keranjang & Form
                _cartItems.Clear();
                TxtNomorUnik.Text = string.Empty;
                TxtNamaPelanggan.Text = string.Empty;
                TxtUangBayar.Text = string.Empty;
                LblUangKembali.Text = "Rp 0";
                CheckMembershipStatus();
            }
            catch (Exception ex)
            {
                TxtErrorKasir.Text = $"Gagal memproses transaksi: {ex.Message}";
            }
        }

        private void BtnBukaRiwayat_Click(object sender, RoutedEventArgs e)
        {
            var win = new RiwayatTransaksiWindow(_currentUser.Nama);
            win.ShowDialog(this);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            _dbService.LogActivity(_currentUser.Id, $"Kasir '{_currentUser.Username}' logout.");
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
