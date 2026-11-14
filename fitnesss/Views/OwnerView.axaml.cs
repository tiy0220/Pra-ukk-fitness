using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using fitnesss.Data;
using fitnesss.Models;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace fitnesss.Views
{
    public partial class OwnerWindow : Window
    {
        private readonly DatabaseService _dbService = new DatabaseService();
        private readonly UserModel _currentUser;
        private List<TransactionModel> _currentTransactions = new();

        public OwnerWindow()
        {
            InitializeComponent();
            _currentUser = new UserModel { Id = 3, Nama = "Pemilik Lontar Gym", Role = "owner" };
            TxtOwnerName.Text = _currentUser.Nama;
            LoadOwnerData();
        }

        public OwnerWindow(UserModel user)
        {
            InitializeComponent();
            _currentUser = user;
            TxtOwnerName.Text = _currentUser.Nama;
            LoadOwnerData();
        }

        private void LoadOwnerData()
        {
            try
            {
                _currentTransactions = _dbService.GetTransactions();
                DgTransactions.ItemsSource = _currentTransactions;

                var products = _dbService.GetProducts();
                DgOwnerProducts.ItemsSource = products;

                var logs = _dbService.GetLogs();
                DgLogs.ItemsSource = logs;

                UpdateSummaryMetrics(_currentTransactions, logs.Count);
                TxtFilterInfo.Text = $"Menampilkan {_currentTransactions.Count} transaksi (Semua Tanggal)";
            }
            catch (Exception ex)
            {
                TxtFilterInfo.Text = $"Gagal memuat data: {ex.Message}";
            }
        }

        private void UpdateSummaryMetrics(List<TransactionModel> trxList, int logCount)
        {
            long totalOmset = trxList.Sum(t => (long)(t.UangBayar - t.UangKembali));
            TxtSummaryOmset.Text = $"Rp {totalOmset:N0}";
            TxtSummaryTrxCount.Text = $"{trxList.Count} Transaksi";
            TxtSummaryLogCount.Text = $"{logCount} Aktivitas";
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (DpStart.SelectedDate.HasValue && DpEnd.SelectedDate.HasValue)
            {
                DateTime start = DpStart.SelectedDate.Value.DateTime.Date;
                DateTime end = DpEnd.SelectedDate.Value.DateTime.Date;

                if (start > end)
                {
                    TxtFilterInfo.Text = "Tanggal mulai tidak boleh lebih besar dari tanggal sampai!";
                    return;
                }

                _currentTransactions = _dbService.GetTransactions(start, end);
                DgTransactions.ItemsSource = _currentTransactions;

                long filteredOmset = _currentTransactions.Sum(t => (long)(t.UangBayar - t.UangKembali));
                TxtSummaryOmset.Text = $"Rp {filteredOmset:N0}";
                TxtSummaryTrxCount.Text = $"{_currentTransactions.Count} Transaksi";
                TxtFilterInfo.Text = $"Filter: {start:dd MMM yyyy} s/d {end:dd MMM yyyy} ({_currentTransactions.Count} transaksi)";
            }
            else
            {
                TxtFilterInfo.Text = "Pilih kedua tanggal (Mulai dan Sampai) untuk memfilter!";
            }
        }

        private void BtnResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DpStart.SelectedDate = null;
            DpEnd.SelectedDate = null;
            LoadOwnerData();
        }

        // --- EKSPOR LAPORAN TRANSAKSI KE FILE PDF ---
        private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTransactions == null || _currentTransactions.Count == 0)
            {
                TxtFilterInfo.Text = "Tidak ada data transaksi untuk diekspor ke PDF!";
                return;
            }

            try
            {
                var doc = new PdfDocument();
                doc.Info.Title = "Laporan Transaksi - Lontar Gym";
                doc.Info.Author = _currentUser.Nama;

                var page = doc.AddPage();
                page.Size = PageSize.A4;
                page.Orientation = PageOrientation.Portrait;

                var gfx = XGraphics.FromPdfPage(page);

                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSubtitle = new XFont("Arial", 10, XFontStyle.Regular);
                var fontHeader = new XFont("Arial", 9, XFontStyle.Bold);
                var fontRegular = new XFont("Arial", 8, XFontStyle.Regular);
                var fontBold = new XFont("Arial", 8, XFontStyle.Bold);

                double margin = 30;
                double y = 40;

                // Header Dokumen
                gfx.DrawString("LONTAR GYM - FITNESS CENTER", fontTitle, XBrushes.Navy, new XRect(margin, y, page.Width - 2 * margin, 25), XStringFormats.TopCenter);
                y += 24;
                gfx.DrawString("LAPORAN PENDAPATAN & TRANSAKSI (OFFLINE SYSTEM)", fontSubtitle, XBrushes.DarkSlateGray, new XRect(margin, y, page.Width - 2 * margin, 18), XStringFormats.TopCenter);
                y += 18;

                string period = (DpStart.SelectedDate.HasValue && DpEnd.SelectedDate.HasValue)
                    ? $"{DpStart.SelectedDate.Value.DateTime:dd MMM yyyy} s/d {DpEnd.SelectedDate.Value.DateTime:dd MMM yyyy}"
                    : "Semua Tanggal (All-Time)";
                gfx.DrawString($"Periode: {period} | Dicetak: {DateTime.Now:dd-MM-yyyy HH:mm} oleh {_currentUser.Nama}", fontRegular, XBrushes.Gray, new XRect(margin, y, page.Width - 2 * margin, 15), XStringFormats.TopCenter);
                y += 20;

                // Garis Pemisah
                gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 1), margin, y, page.Width - margin, y);
                y += 15;

                // Kotak Ringkasan Omset
                long totalOmset = _currentTransactions.Sum(t => (long)(t.UangBayar - t.UangKembali));
                XRect summaryRect = new XRect(margin, y, page.Width - 2 * margin, 32);
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(245, 247, 250)), summaryRect);
                gfx.DrawRectangle(new XPen(XColor.FromArgb(220, 225, 230)), summaryRect);

                gfx.DrawString($"Total Transaksi: {_currentTransactions.Count} Transaksi", fontBold, XBrushes.DarkBlue, new XPoint(margin + 15, y + 20));
                gfx.DrawString($"Total Omset / Pendapatan: Rp {totalOmset:N0}", fontBold, XBrushes.DarkGreen, new XPoint(margin + 260, y + 20));
                y += 45;

                // Konfigurasi Kolom Tabel
                double[] colWidths = { 28, 85, 95, 70, 135, 60, 60 };
                string[] colHeaders = { "No", "Waktu", "Pelanggan", "No. Unik", "Layanan / Produk", "Bayar", "Kembali" };

                // Header Tabel
                double x = margin;
                XRect headerRow = new XRect(margin, y, page.Width - 2 * margin, 20);
                gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(230, 235, 245)), headerRow);
                gfx.DrawRectangle(new XPen(XColor.FromArgb(200, 210, 225)), headerRow);

                for (int c = 0; c < colHeaders.Length; c++)
                {
                    gfx.DrawString(colHeaders[c], fontHeader, XBrushes.Black, new XRect(x + 3, y + 4, colWidths[c] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[c];
                }
                y += 20;

                // Render Baris Transaksi
                int rowNum = 1;
                foreach (var trx in _currentTransactions)
                {
                    // Cek jika butuh halaman baru
                    if (y > page.Height - 50)
                    {
                        page = doc.AddPage();
                        page.Size = PageSize.A4;
                        page.Orientation = PageOrientation.Portrait;
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;

                        // Gambar ulang header tabel pada halaman baru
                        x = margin;
                        headerRow = new XRect(margin, y, page.Width - 2 * margin, 20);
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(230, 235, 245)), headerRow);
                        gfx.DrawRectangle(new XPen(XColor.FromArgb(200, 210, 225)), headerRow);

                        for (int c = 0; c < colHeaders.Length; c++)
                        {
                            gfx.DrawString(colHeaders[c], fontHeader, XBrushes.Black, new XRect(x + 3, y + 4, colWidths[c] - 6, 14), XStringFormats.TopLeft);
                            x += colWidths[c];
                        }
                        y += 20;
                    }

                    // Belang-belang baris tabel
                    if (rowNum % 2 == 0)
                    {
                        gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(250, 250, 252)), new XRect(margin, y, page.Width - 2 * margin, 18));
                    }

                    x = margin;
                    gfx.DrawString(rowNum.ToString(), fontRegular, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[0] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[0];

                    gfx.DrawString(trx.CreatedAt.ToString("dd/MM/yy HH:mm"), fontRegular, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[1] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[1];

                    gfx.DrawString(trx.NamaPelanggan, fontRegular, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[2] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[2];

                    gfx.DrawString(trx.NomorUnik, fontRegular, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[3] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[3];

                    gfx.DrawString(trx.NamaProduk, fontRegular, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[4] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[4];

                    gfx.DrawString($"Rp {trx.UangBayar:N0}", fontRegular, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[5] - 6, 14), XStringFormats.TopLeft);
                    x += colWidths[5];

                    gfx.DrawString($"Rp {trx.UangKembali:N0}", fontRegular, XBrushes.Black, new XRect(x + 3, y + 3, colWidths[6] - 6, 14), XStringFormats.TopLeft);

                    y += 18;
                    rowNum++;
                }

                // Simpan ke direktori Reports
                string reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                Directory.CreateDirectory(reportsDir);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string pdfPath = Path.Combine(reportsDir, $"Laporan_Transaksi_{timestamp}.pdf");

                doc.Save(pdfPath);

                _dbService.LogActivity(_currentUser.Id, $"Owner mengekspor laporan transaksi ke PDF: Laporan_Transaksi_{timestamp}.pdf");

                TxtFilterInfo.Text = $"✓ Sukses ekspor PDF: Reports/Laporan_Transaksi_{timestamp}.pdf";

                // Buka otomatis file PDF
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pdfPath,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // Diamkan jika viewer default tidak tersedia
                }
            }
            catch (Exception ex)
            {
                TxtFilterInfo.Text = $"Gagal mengekspor PDF: {ex.Message}";
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            _dbService.LogActivity(_currentUser.Id, $"Owner '{_currentUser.Username}' logout.");
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
