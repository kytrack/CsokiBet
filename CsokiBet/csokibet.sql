-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Gép: 127.0.0.1
-- Létrehozás ideje: 2024. Sze 26. 14:37
-- Kiszolgáló verziója: 10.4.32-MariaDB
-- PHP verzió: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Adatbázis: `csokibet`
--

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `bets`
--

CREATE TABLE `bets` (
  `BetsID` int(11) NOT NULL,
  `BetDate` date NOT NULL,
  `Odds` float NOT NULL,
  `Amount` int(11) NOT NULL,
  `BettorsID` int(11) NOT NULL,
  `EventID` int(11) NOT NULL,
  `Status` tinyint(1) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `bettors`
--

CREATE TABLE `bettors` (
  `BettorsID` int(11) NOT NULL,
  `Username` varchar(50) NOT NULL,
  `Password` varchar(255) DEFAULT NULL,
  `Balance` int(11) NOT NULL,
  `Email` varchar(100) NOT NULL,
  `JoinDate` date NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT 1,
  `Role` enum('Bettor','Admin','Organizer') NOT NULL DEFAULT 'Bettor'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- A tábla adatainak kiíratása `bettors`
--

INSERT INTO `bettors` (`BettorsID`, `Username`, `Password`, `Balance`, `Email`, `JoinDate`, `IsActive`, `Role`) VALUES
(1, 'JohnDoe', '688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6', 1000, 'john.doe@example.com', '2023-01-15', 1, 'Bettor'),
(2, 'JaneSmith', '688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6', 2500, 'jane.smith@example.com', '2022-05-23', 1, 'Bettor'),
(3, 'AdminUser', '688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6', 5000, 'admin.user@example.com', '2020-09-10', 1, 'Admin'),
(4, 'EventOrg', '688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6', 3000, 'event.org@example.com', '2021-03-29', 1, 'Organizer'),
(5, 'InactiveBettor', '688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6', 1500, 'inactive.bettor@example.com', '2023-02-17', 0, 'Bettor'),
(10, 'vakond', '6bcf18fb89650e3d579be3d32f3f5b8326065df71c5a44a84ff93d405be538b3', 1000000, 'danko7119@gmail.com', '2024-10-11', 1, 'Admin');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `events`
--

CREATE TABLE `events` (
  `EventID` int(11) NOT NULL,
  `EventName` varchar(100) NOT NULL,
  `EventDate` date NOT NULL,
  `Category` varchar(50) NOT NULL,
  `Location` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_hungarian_ci;

--
-- Indexek a kiírt táblákhoz
--
-- Insert eldöntendő események az 'events' táblába különböző sportokkal
INSERT INTO `events` (`EventName`, `EventDate`, `Category`, `Location`)
VALUES
-- Futball események
('A Fradi 2 góllal nyeri meg a meccset', '2024-10-15', 'Futball', 'Budapest, Groupama Aréna'),
('Lesz 3 vagy több gól a meccsen', '2024-10-15', 'Futball', 'Budapest, Groupama Aréna'),
('Lesz piros lap a meccsen', '2024-10-18', 'Futball', 'Debrecen, Nagyerdei Stadion'),
('Mindkét csapat szerez gólt', '2024-10-20', 'Futball', 'Győr, ETO Park'),
('A mérkőzés döntetlennel ér véget', '2024-10-22', 'Futball', 'Budapest, Illovszky Rudolf Stadion'),
('A meccsen lesz tizenegyes', '2024-10-25', 'Futball', 'Szeged, Szent Gellért Fórum'),
('A Fradi gólt szerez az első félidőben', '2024-10-28', 'Futball', 'Szombathely, Haladás Sportkomplexum'),

-- Kézilabda események
('A Veszprém megnyeri a mérkőzést 5 gólos különbséggel', '2024-11-02', 'Kézilabda', 'Veszprém, Veszprém Aréna'),
('A Szeged szerzi az első gólt a meccsen', '2024-11-05', 'Kézilabda', 'Szeged, Pick Aréna'),
('Lesz piros lap a kézilabda mérkőzésen', '2024-11-08', 'Kézilabda', 'Debrecen, Főnix Csarnok'),
('A meccsen 60 gól fölött lesz az összesített eredmény', '2024-11-10', 'Kézilabda', 'Tatabánya, Multifunkcionális Csarnok'),
('A második félidőben a hazai csapat szerez több gólt', '2024-11-12', 'Kézilabda', 'Győr, Audi Aréna'),

-- Kosárlabda események
('A Falco szerez 80 pontot vagy többet', '2024-11-15', 'Kosárlabda', 'Szombathely, Arena Savaria'),
('Az Alba Fehérvár dob elsőként hárompontost', '2024-11-17', 'Kosárlabda', 'Székesfehérvár, Alba Regia Sportcsarnok'),
('Lesz 10 vagy több hárompontos a mérkőzésen', '2024-11-19', 'Kosárlabda', 'Kecskemét, Messzi István Sportcsarnok'),
('A mérkőzés 5 pontos különbséggel dől el', '2024-11-21', 'Kosárlabda', 'Zalaegerszeg, Zalakerámia Sportcsarnok'),
('A hazai csapat vezet az első negyed után', '2024-11-23', 'Kosárlabda', 'Pécs, Lauber Dezső Sportcsarnok'),

-- Vízilabda események
('A Szolnok nyeri a mérkőzést 3 gólos különbséggel', '2024-11-25', 'Vízilabda', 'Szolnok, Vízilabda Aréna'),
('Lesz több mint 15 gól a meccsen', '2024-11-27', 'Vízilabda', 'Eger, Bitskey Aladár Uszoda'),
('A meccsen mindkét csapat szerzi legalább 7 gólt', '2024-11-29', 'Vízilabda', 'Budapest, Komjádi Béla Uszoda'),
('Az első negyed döntetlennel zárul', '2024-12-01', 'Vízilabda', 'Szeged, Tiszavirág Sportuszoda'),
('A Fradi szerzi a legtöbb gólt a második félidőben', '2024-12-03', 'Vízilabda', 'Budapest, Népligeti Uszoda');

--
-- A tábla indexei `bets`
--
ALTER TABLE `bets`
  ADD PRIMARY KEY (`BetsID`),
  ADD KEY `BettorsID` (`BettorsID`),
  ADD KEY `EventID` (`EventID`);

--
-- A tábla indexei `bettors`
--
ALTER TABLE `bettors`
  ADD PRIMARY KEY (`BettorsID`);

--
-- A tábla indexei `events`
--
ALTER TABLE `events`
  ADD PRIMARY KEY (`EventID`);

--
-- A kiírt táblák AUTO_INCREMENT értéke
--

--
-- AUTO_INCREMENT a táblához `bets`
--
ALTER TABLE `bets`
  MODIFY `BetsID` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT a táblához `bettors`
--
ALTER TABLE `bettors`
  MODIFY `BettorsID` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT a táblához `events`
--
ALTER TABLE `events`
  MODIFY `EventID` int(11) NOT NULL AUTO_INCREMENT;

--
-- Megkötések a kiírt táblákhoz
--

--
-- Megkötések a táblához `bets`
--
ALTER TABLE `bets`
  ADD CONSTRAINT `bets_ibfk_1` FOREIGN KEY (`BettorsID`) REFERENCES `bettors` (`BettorsID`),
  ADD CONSTRAINT `bets_ibfk_2` FOREIGN KEY (`EventID`) REFERENCES `events` (`EventID`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
