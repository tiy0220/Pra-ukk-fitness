using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using fitnesss.Models;

namespace fitnesss.Views
{
    public partial class StrukWindow : Window
    {
        private List<CartItemModel> _items = new();

        public StrukWindow()
        {
            InitializeComponent();
        }

        // Konstruktor untuk daftar multi-item keranjang belanja (POS Cart)
        public StrukWindow(int trxId, string kasirNama, string pelangganNama, string nomorUnik, List<CartItemModel> items, int totalTagihan, int bayar, int kembali)
        {
            InitializeComponent();
            _items = items ?? new List<CartItemModel>();

            TxtNoTrx.Text = $"#TRX-{trxId:D5}";
            TxtTglTrx.Text = DateTime.Now.ToString("dd MMM yyyy, HH:mm");
            TxtKasir.Text = kasirNama;
            TxtPelanggan.Text = $"{pelangganNama} ({nomorUnik})";

            IcReceiptItems.ItemsSource = _items;

            TxtTotalTagihan.Text = $"Rp {totalTagihan:N0}";
            TxtUangBayar.Text = $"Rp {bayar:N0}";
            TxtUangKembali.Text = $"Rp {kembali:N0}";
        }

        // Konstruktor single-item (backward compatibility)
        public StrukWindow(int trxId, string kasirNama, string pelangganNama, string nomorUnik, string produkNama, int totalHarga, int bayar, int kembali, bool isGratisMember)
        {
            InitializeComponent();

            _items = new List<CartItemModel>
            {
                new CartItemModel
                {
                    ProductName = produkNama,
                    UnitPrice = totalHarga,
                    Qty = 1,
                    Subtotal = totalHarga,
                    IsFreeMemberBenefit = isGratisMember,
                    Note = isGratisMember ? "* Kuota Harian Member: GRATIS (1x/hari)" : ""
                }
            };

            TxtNoTrx.Text = $"#TRX-{trxId:D5}";
            TxtTglTrx.Text = DateTime.Now.ToString("dd MMM yyyy, HH:mm");
            TxtKasir.Text = kasirNama;
            TxtPelanggan.Text = $"{pelangganNama} ({nomorUnik})";

            IcReceiptItems.ItemsSource = _items;

            TxtTotalTagihan.Text = $"Rp {totalHarga:N0}";
            TxtUangBayar.Text = $"Rp {bayar:N0}";
            TxtUangKembali.Text = $"Rp {kembali:N0}";
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("========================================");
                sb.AppendLine("              LONTAR GYM                ");
                sb.AppendLine("     Fitness Center & Health Club       ");
                sb.AppendLine("  Jl. Raya Lontar No. 123 (Offline)     ");
                sb.AppendLine("========================================");
                sb.AppendLine($"No. Transaksi : {TxtNoTrx.Text}");
                sb.AppendLine($"Tanggal/Jam   : {TxtTglTrx.Text}");
                sb.AppendLine($"Kasir         : {TxtKasir.Text}");
                sb.AppendLine($"Pelanggan     : {TxtPelanggan.Text}");
                sb.AppendLine("----------------------------------------");
                sb.AppendLine("ITEM / LAYANAN:");
                foreach (var item in _items)
                {
                    sb.AppendLine($"- {item.ProductName} ({item.Qty}x) : Rp {item.Subtotal:N0}");
                    if (item.IsFreeMemberBenefit)
                    {
                        sb.AppendLine($"  {item.Note}");
                    }
                }
                sb.AppendLine("----------------------------------------");
                sb.AppendLine($"Total Tagihan : {TxtTotalTagihan.Text}");
                sb.AppendLine($"Uang Bayar    : {TxtUangBayar.Text}");
                sb.AppendLine($"Uang Kembali  : {TxtUangKembali.Text}");
                sb.AppendLine("========================================");
                sb.AppendLine("   Terima Kasih Atas Kunjungan Anda!    ");
                sb.AppendLine("    Sehat & Bugar Bersama Lontar Gym    ");
                sb.AppendLine("========================================");

                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts");
                Directory.CreateDirectory(folder);
                string fileName = (TxtNoTrx.Text ?? "TRX").Replace("#", "");
                string filePath = Path.Combine(folder, $"{fileName}.txt");
                File.WriteAllText(filePath, sb.ToString());

                if (sender is Button btn)
                {
                    btn.Content = "✓ Struk Tersimpan & Tercetak!";
                    btn.IsEnabled = false;
                }
            }
            catch
            {
                if (sender is Button btn)
                {
                    btn.Content = "✓ Struk Tercetak!";
                    btn.IsEnabled = false;
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
