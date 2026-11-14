using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using fitnesss.Data;
using fitnesss.Models;

namespace fitnesss.Views
{
    public partial class RiwayatTransaksiWindow : Window
    {
        private readonly DatabaseService _dbService = new DatabaseService();
        private readonly string _kasirNama;

        public RiwayatTransaksiWindow()
        {
            InitializeComponent();
            _kasirNama = "Kasir";
            LoadData();
        }

        public RiwayatTransaksiWindow(string kasirNama)
        {
            InitializeComponent();
            _kasirNama = kasirNama;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var now = DateTime.Today;
                var list = _dbService.GetTransactions(now, now);
                DgTodayTransactions.ItemsSource = list;
            }
            catch
            {
                // Diamkan jika error
            }
        }

        private void BtnCetakUlang_Click(object sender, RoutedEventArgs e)
        {
            if (DgTodayTransactions.SelectedItem is TransactionModel selected)
            {
                int totalTagihan = selected.UangBayar - selected.UangKembali;
                bool isGratis = (totalTagihan == 0);

                var struk = new StrukWindow(
                    selected.Id,
                    _kasirNama,
                    selected.NamaPelanggan,
                    selected.NomorUnik,
                    selected.NamaProduk,
                    totalTagihan,
                    selected.UangBayar,
                    selected.UangKembali,
                    isGratis
                );
                struk.ShowDialog(this);
            }
        }

        private void BtnTutup_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
