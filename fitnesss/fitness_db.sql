-- Database: fitness_db
CREATE DATABASE IF NOT EXISTS `fitness_db`;
USE `fitness_db`;

-- 1. Table users (Sesuai ERD Soal UKK)
CREATE TABLE IF NOT EXISTS `users` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `username` VARCHAR(45) NOT NULL UNIQUE,
    `password` VARCHAR(45) NOT NULL,
    `nama` VARCHAR(45) NOT NULL,
    `role` ENUM('admin', 'kasir', 'owner') NOT NULL,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Table products (Sesuai ERD Soal UKK)
CREATE TABLE IF NOT EXISTS `products` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `nama_produk` VARCHAR(45) NOT NULL,
    `harga_produk` INT NOT NULL,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Table transactions (Sesuai ERD Soal UKK)
CREATE TABLE IF NOT EXISTS `transactions` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_produk` INT NOT NULL,
    `nama_pelanggan` VARCHAR(45) NOT NULL,
    `nomor_unik` VARCHAR(10) NOT NULL,
    `uang_bayar` INT NOT NULL,
    `uang_kembali` INT NOT NULL,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_nomor_unik (`nomor_unik`),
    INDEX idx_created_at (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Table log (Sesuai ERD Soal UKK)
CREATE TABLE IF NOT EXISTS `log` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `id_user` INT NOT NULL,
    `activity` TEXT NOT NULL,
    `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Data Awal Pengguna (Users)
INSERT INTO `users` (`id`, `username`, `password`, `nama`, `role`) VALUES
(1, 'admin', 'admin', 'Administrator Gym', 'admin'),
(2, 'kasir', 'kasir', 'Kasir Utama Lontar', 'kasir'),
(3, 'owner', 'owner', 'Pemilik Lontar Gym', 'owner')
ON DUPLICATE KEY UPDATE `username` = `username`;

-- Data Awal Produk (Products)
INSERT INTO `products` (`id`, `nama_produk`, `harga_produk`) VALUES
(1, 'Member Bulanan', 150000),
(2, 'Gym Harian', 20000),
(3, 'Personal Trainer (1 Sesi)', 100000),
(4, 'Air Mineral 600ml', 5000)
ON DUPLICATE KEY UPDATE `nama_produk` = `nama_produk`;
