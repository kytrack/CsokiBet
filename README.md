# 🎲 Csokibet - WPF Fogadási Alkalmazás

Üdvözlünk a **Csokibet**-nél! Ez egy C#-al fejlesztett, WPF-alapú fogadási alkalmazás, amely lehetővé teszi a felhasználók számára, hogy különböző fogadási tevékenységeket végezzenek egy modern, felhasználóbarát felületen.

---

## 📝 Funkciók

- 💻 **WPF UI** - Modern és letisztult felhasználói felület a Windows Presentation Foundation (WPF) technológiával.
- 🔐 **Biztonságos** - Firebase alapú hitelesítés és email cím visszaigazolás a biztonságos bejelentkezéshez.
- 📊 **Fogadási Irányítópult** - Valós idejű frissítések és statisztikák az aktuális fogadásokról és eredményekről.
- 🛠️ **Admin és fogadásszervező felület** - Az alkalmazás külön admin és fogadásszervező felülettel rendelkezik.

---

## ‼️ Fontos tudnivalók

- FIGYELEM! Az alkalmazásban Google Firebase beléptetőrendszer működik, az alap MySql adatbázistól függetlenül. Regisztrációnál felveszi ugyan az adatokat a MySql-be de azok később (pl. jelszóváltoztatás) esetén nem módosulnak, valamint csak olyan fiókokkal lehet belépni amelyek szinkronizálva vannak a Firebase adatbázissal (az admin panel kiírja melyek azok a felhasználók). Hogy ez hiba nélkül működhessen, olvasd végig figyelmesen a telepítési útmutatót!

## 🚀 Első Lépések

### Szükséges eszközök

- Visual Studio 2022 vagy újabb
- .NET 6.0 SDK
- Firebase fiók a hitelesítéshez
- NuGet Packages: Firebase.Auth, Firebase.Database, FirebaseAdmin, MaterialDesignColors, MaterialDesignThemes, MySql.Data, Security.Cryptography

### Telepítés

1. Klónozd a repository-t:
   git clone https://github.com/kytrack/CsokiBet.git
2. Nyitsd meg a Visual Studio 2022 -t és töltsd be a projektet, vagy kattints a CsokiBet.sln-re
3. Ellenőrizd, hogy az összes NuGet package le van-e töltve, ha nem töltsd le ezeket
4. Töltsd le a FIREBASE_KEY.zip fájl-t innen: https://drive.google.com/file/d/18R7cZ088UMRTAqffNoHxR05oH1VM-2Du/view?usp=sharing
5. Csomagold ki és a firebase.json fájlt másold be a CsokiBet mappába (amibe az xaml-ek vanak)
6. Hozz létre az XAMPP segítségével egy adatbázist "csokibet" néven és a CsokiBet mappába található sql fájlt importáld be
7. Indítsd el a programot a Visual Studioban

### Készítők

- Projektet Dankó Dániel és Vedrán Krisztián készítette
