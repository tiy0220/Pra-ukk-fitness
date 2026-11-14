using System;
using System.Collections.Generic;
using MySqlConnector;
using fitnesss.Models;

namespace fitnesss.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString = "Server=localhost;Database=fitness_db;Uid=root;Pwd=;";

        public MySqlConnection GetConnection() => new MySqlConnection(_connectionString);

        // --- 0. INISIALISASI STRUKTUR DATABASE & SEED DATA SESUAI DOKUMEN UKK ---
        public void EnsureTablesExist()
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                string query = @"
                    CREATE TABLE IF NOT EXISTS users (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        username VARCHAR(45) NOT NULL UNIQUE,
                        password VARCHAR(45) NOT NULL,
                        nama VARCHAR(45) NOT NULL,
                        role ENUM('admin', 'kasir', 'owner') NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS products (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        nama_produk VARCHAR(45) NOT NULL,
                        harga_produk INT NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS transactions (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        id_produk INT NOT NULL,
                        nama_pelanggan VARCHAR(45) NOT NULL,
                        nomor_unik VARCHAR(10) NOT NULL,
                        uang_bayar INT NOT NULL,
                        uang_kembali INT NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                        INDEX idx_nomor_unik (nomor_unik),
                        INDEX idx_created_at (created_at)
                    );

                    CREATE TABLE IF NOT EXISTS log (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        id_user INT NOT NULL,
                        activity TEXT NOT NULL,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );

                    -- Seed Users jika belum ada
                    INSERT IGNORE INTO users (id, username, password, nama, role) VALUES
                    (1, 'admin', 'admin', 'Administrator Gym', 'admin'),
                    (2, 'kasir', 'kasir', 'Kasir Utama Lontar', 'kasir'),
                    (3, 'owner', 'owner', 'Pemilik Lontar Gym', 'owner');

                    -- Seed Products jika belum ada
                    INSERT IGNORE INTO products (id, nama_produk, harga_produk) VALUES
                    (1, 'Member Bulanan', 150000),
                    (2, 'Gym Harian', 20000),
                    (3, 'Personal Trainer (1 Sesi)', 100000),
                    (4, 'Air Mineral 600ml', 5000);
                ";
                using var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Diamkan agar ditangani pemanggil jika MySQL belum aktif
            }
        }

        // --- 1. LOG AKTIVITAS (ADMIN & KASIR) ---
        public void LogActivity(int userId, string activity)
        {
            try
            {
                using var conn = GetConnection();
                conn.Open();
                string query = "INSERT INTO log (id_user, activity, created_at) VALUES (@id_user, @activity, NOW())";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id_user", userId);
                cmd.Parameters.AddWithValue("@activity", activity);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Jangan menggagalkan alur utama jika log bermasalah
            }
        }

        // --- 2. AUTHENTICATION (LOGIN) ---
        public UserModel? Login(string username, string password)
        {
            EnsureTablesExist();
            using var conn = GetConnection();
            conn.Open();
            string query = "SELECT id, username, nama, role FROM users WHERE username = @u AND password = @p LIMIT 1";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserModel
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Nama = reader.GetString("nama"),
                    Role = reader.GetString("role")
                };
            }
            return null;
        }

        // --- 3. PRODUK (ADMIN & KASIR & OWNER) ---
        public List<ProductModel> GetProducts()
        {
            var list = new List<ProductModel>();
            using var conn = GetConnection();
            conn.Open();
            string query = "SELECT id, nama_produk, harga_produk, created_at FROM products ORDER BY id ASC";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ProductModel
                {
                    Id = reader.GetInt32("id"),
                    NamaProduk = reader.GetString("nama_produk"),
                    HargaProduk = reader.GetInt32("harga_produk"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return list;
        }

        public void AddProduct(string nama, int harga, int adminId)
        {
            using var conn = GetConnection();
            conn.Open();
            string query = "INSERT INTO products (nama_produk, harga_produk) VALUES (@n, @h)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@n", nama);
            cmd.Parameters.AddWithValue("@h", harga);
            cmd.ExecuteNonQuery();
            LogActivity(adminId, $"Menambah produk baru: {nama} (Rp {harga:N0})");
        }

        public void UpdateProduct(int id, string nama, int harga, int adminId)
        {
            using var conn = GetConnection();
            conn.Open();
            string query = "UPDATE products SET nama_produk = @n, harga_produk = @h WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@n", nama);
            cmd.Parameters.AddWithValue("@h", harga);
            cmd.ExecuteNonQuery();
            LogActivity(adminId, $"Mengupdate produk ID {id}: {nama} (Rp {harga:N0})");
        }

        public void DeleteProduct(int id, string namaProduk, int adminId)
        {
            using var conn = GetConnection();
            conn.Open();
            string query = "DELETE FROM products WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            LogActivity(adminId, $"Menghapus/menonaktifkan produk: {namaProduk} (ID: {id})");
        }

        // --- 4. MANAJEMEN USER (ADMIN) ---
        public List<UserModel> GetUsers()
        {
            var list = new List<UserModel>();
            using var conn = GetConnection();
            conn.Open();
            string query = "SELECT id, username, password, nama, role, created_at FROM users ORDER BY id ASC";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new UserModel
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password"),
                    Nama = reader.GetString("nama"),
                    Role = reader.GetString("role"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return list;
        }

        public void AddUser(UserModel user, int adminId)
        {
            using var conn = GetConnection();
            conn.Open();
            string query = "INSERT INTO users (username, password, nama, role) VALUES (@u, @p, @n, @r)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", user.Username);
            cmd.Parameters.AddWithValue("@p", user.Password);
            cmd.Parameters.AddWithValue("@n", user.Nama);
            cmd.Parameters.AddWithValue("@r", user.Role);
            cmd.ExecuteNonQuery();
            LogActivity(adminId, $"Menambah pengguna baru: {user.Username} (Role: {user.Role})");
        }

        public void UpdateUser(UserModel user, int adminId)
        {
            using var conn = GetConnection();
            conn.Open();
            string query = "UPDATE users SET username = @u, password = @p, nama = @n, role = @r WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", user.Id);
            cmd.Parameters.AddWithValue("@u", user.Username);
            cmd.Parameters.AddWithValue("@p", user.Password);
            cmd.Parameters.AddWithValue("@n", user.Nama);
            cmd.Parameters.AddWithValue("@r", user.Role);
            cmd.ExecuteNonQuery();
            LogActivity(adminId, $"Mengupdate data user ID {user.Id}: {user.Username} ({user.Role})");
        }

        public void DeleteUser(int id, string username, int adminId)
        {
            using var conn = GetConnection();
            conn.Open();
            string query = "DELETE FROM users WHERE id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            LogActivity(adminId, $"Menonaktifkan/menghapus user: {username} (ID: {id})");
        }

        // --- 5. LOGIKA TRANSAKSI & MEMBERSHIP SESUAI USER REQUEST ---
        // Pengecekan apakah nomor unik memiliki Membership aktif (30 hari terakhir)
        public bool IsActiveMember(string nomorUnik, out string? memberName, out DateTime? validUntil)
        {
            memberName = null;
            validUntil = null;
            if (string.IsNullOrWhiteSpace(nomorUnik)) return false;

            using var conn = GetConnection();
            conn.Open();
            string query = @"
                SELECT t.nama_pelanggan, t.created_at 
                FROM transactions t
                JOIN products p ON t.id_produk = p.id
                WHERE t.nomor_unik = @nu 
                  AND p.nama_produk LIKE '%Member%' 
                  AND p.nama_produk NOT LIKE '%Harian%'
                  AND t.created_at >= NOW() - INTERVAL 30 DAY
                ORDER BY t.created_at DESC 
                LIMIT 1";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nu", nomorUnik.Trim());
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                memberName = reader.GetString("nama_pelanggan");
                DateTime buyDate = reader.GetDateTime("created_at");
                validUntil = buyDate.AddDays(30);
                return true;
            }
            return false;
        }

        // Hitung berapa kali nomor unik ini telah check-in Gym Harian HARI INI
        public int GetDailyGymVisitsToday(string nomorUnik)
        {
            if (string.IsNullOrWhiteSpace(nomorUnik)) return 0;

            using var conn = GetConnection();
            conn.Open();
            string query = @"
                SELECT COUNT(*) 
                FROM transactions t
                JOIN products p ON t.id_produk = p.id
                WHERE t.nomor_unik = @nu 
                  AND (p.nama_produk LIKE '%Harian%' OR p.nama_produk LIKE '%Gym Harian%')
                  AND DATE(t.created_at) = CURRENT_DATE()";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nu", nomorUnik.Trim());
            var countObj = cmd.ExecuteScalar();
            return countObj != null ? Convert.ToInt32(countObj) : 0;
        }

        // Mengambil nama pelanggan terakhir berdasarkan nomor unik jika pernah transaksi
        public string? GetLatestCustomerName(string nomorUnik)
        {
            if (string.IsNullOrWhiteSpace(nomorUnik)) return null;

            using var conn = GetConnection();
            conn.Open();
            string query = "SELECT nama_pelanggan FROM transactions WHERE nomor_unik = @nu ORDER BY created_at DESC LIMIT 1";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@nu", nomorUnik.Trim());
            var res = cmd.ExecuteScalar();
            return res?.ToString();
        }

        // Simpan Transaksi Kasir
        public int AddTransaction(TransactionModel trx, int kasirId)
        {
            using var conn = GetConnection();
            conn.Open();
            string query = @"
                INSERT INTO transactions (id_produk, nama_pelanggan, nomor_unik, uang_bayar, uang_kembali, created_at) 
                VALUES (@idp, @np, @nu, @ub, @uk, NOW());
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@idp", trx.IdProduk);
            cmd.Parameters.AddWithValue("@np", trx.NamaPelanggan.Trim());
            cmd.Parameters.AddWithValue("@nu", trx.NomorUnik.Trim());
            cmd.Parameters.AddWithValue("@ub", trx.UangBayar);
            cmd.Parameters.AddWithValue("@uk", trx.UangKembali);

            int newId = Convert.ToInt32(cmd.ExecuteScalar());
            LogActivity(kasirId, $"Transaksi kasir #{newId} a.n {trx.NamaPelanggan} (Nomor Unik: {trx.NomorUnik}) - Total: Rp {trx.UangBayar - trx.UangKembali:N0}");
            return newId;
        }

        // --- 6. LAPORAN & LOG (OWNER & KASIR) ---
        public List<TransactionModel> GetTransactions(DateTime? start = null, DateTime? end = null)
        {
            var list = new List<TransactionModel>();
            using var conn = GetConnection();
            conn.Open();

            string query = @"SELECT t.id, t.id_produk, p.nama_produk, t.nama_pelanggan, t.nomor_unik, t.uang_bayar, t.uang_kembali, t.created_at 
                             FROM transactions t 
                             JOIN products p ON t.id_produk = p.id";

            if (start.HasValue && end.HasValue)
            {
                query += " WHERE DATE(t.created_at) BETWEEN @start AND @end";
            }
            query += " ORDER BY t.created_at DESC";

            using var cmd = new MySqlCommand(query, conn);
            if (start.HasValue && end.HasValue)
            {
                cmd.Parameters.AddWithValue("@start", start.Value.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@end", end.Value.ToString("yyyy-MM-dd"));
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TransactionModel
                {
                    Id = reader.GetInt32("id"),
                    IdProduk = reader.GetInt32("id_produk"),
                    NamaProduk = reader.GetString("nama_produk"),
                    NamaPelanggan = reader.GetString("nama_pelanggan"),
                    NomorUnik = reader.GetString("nomor_unik"),
                    UangBayar = reader.GetInt32("uang_bayar"),
                    UangKembali = reader.GetInt32("uang_kembali"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return list;
        }

        public List<LogModel> GetLogs()
        {
            var list = new List<LogModel>();
            using var conn = GetConnection();
            conn.Open();
            string query = @"SELECT l.id, l.id_user, COALESCE(u.username, 'Sistem') as username, COALESCE(u.role, '-') as role, l.activity, l.created_at 
                             FROM log l 
                             LEFT JOIN users u ON l.id_user = u.id 
                             ORDER BY l.created_at DESC";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new LogModel
                {
                    Id = reader.GetInt32("id"),
                    IdUser = reader.GetInt32("id_user"),
                    Username = reader.GetString("username"),
                    Role = reader.GetString("role"),
                    Activity = reader.GetString("activity"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
            return list;
        }
    }
}
