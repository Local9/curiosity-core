-- --------------------------------------------------------
-- Värd:                         127.0.0.1
-- Serverversion:                10.1.37-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win32
-- HeidiSQL Version:             9.5.0.5196
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;


-- Dumping database structure for roleplay
CREATE DATABASE IF NOT EXISTS `roleplay` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_swedish_ci */;
USE `roleplay`;

-- Dumping structure for tabell roleplay.bills
CREATE TABLE IF NOT EXISTS `bills` (
  `Seed` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Receiver` longtext COLLATE utf8mb4_swedish_ci,
  `Sender` longtext COLLATE utf8mb4_swedish_ci,
  `Designation` longtext COLLATE utf8mb4_swedish_ci,
  `AmountLines` longtext COLLATE utf8mb4_swedish_ci,
  `Amount` int(11) DEFAULT NULL,
  `IsActive` int(11) DEFAULT NULL,
  `BillNumber` int(11) DEFAULT NULL,
  `ClientNumber` int(11) DEFAULT NULL,
  `OrderNumber` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;

-- Dumpar data för tabell roleplay.bills: ~0 rows (ungefär)
/*!40000 ALTER TABLE `bills` DISABLE KEYS */;
/*!40000 ALTER TABLE `bills` ENABLE KEYS */;

-- Dumping structure for tabell roleplay.businesses
CREATE TABLE IF NOT EXISTS `businesses` (
  `Seed` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Balance` int(11) DEFAULT NULL,
  `Registered` bigint(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;

-- Dumpar data för tabell roleplay.businesses: ~0 rows (ungefär)
/*!40000 ALTER TABLE `businesses` DISABLE KEYS */;
/*!40000 ALTER TABLE `businesses` ENABLE KEYS */;

-- Dumping structure for tabell roleplay.characters
CREATE TABLE IF NOT EXISTS `characters` (
  `Seed` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Owner` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Name` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Surname` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `DateOfBirth` varchar(10) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `LastDigits` varchar(4) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Health` int(11) DEFAULT NULL,
  `Shield` int(11) DEFAULT NULL,
  `Cash` int(11) DEFAULT NULL,
  `BankAccount` text COLLATE utf8mb4_swedish_ci,
  `Style` text COLLATE utf8mb4_swedish_ci,
  `MarkedAsRegistered` bit(1) DEFAULT NULL,
  `Metadata` longtext COLLATE utf8mb4_swedish_ci
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;

-- Dumpar data för tabell roleplay.characters: ~12 rows (ungefär)
/*!40000 ALTER TABLE `characters` DISABLE KEYS */;
/*!40000 ALTER TABLE `characters` ENABLE KEYS */;

-- Dumping structure for tabell roleplay.storages
CREATE TABLE IF NOT EXISTS `storages` (
  `Seed` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Owner` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `Metadata` longtext COLLATE utf8mb4_swedish_ci
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;

-- Dumpar data för tabell roleplay.storages: ~0 rows (ungefär)
/*!40000 ALTER TABLE `storages` DISABLE KEYS */;
/*!40000 ALTER TABLE `storages` ENABLE KEYS */;

-- Dumping structure for tabell roleplay.users
CREATE TABLE IF NOT EXISTS `users` (
  `Seed` varchar(50) COLLATE utf8mb4_swedish_ci DEFAULT NULL,
  `SteamId` longtext COLLATE utf8mb4_swedish_ci,
  `LastName` longtext COLLATE utf8mb4_swedish_ci,
  `Role` tinyint(4) DEFAULT NULL,
  `LatestActivity` date DEFAULT NULL,
  `ConnectionHistory` longtext COLLATE utf8mb4_swedish_ci,
  `Metadata` longtext COLLATE utf8mb4_swedish_ci
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_swedish_ci;

-- Dumpar data för tabell roleplay.users: ~8 rows (ungefär)
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
/*!40000 ALTER TABLE `users` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
