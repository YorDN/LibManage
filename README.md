
<div align="center">
  
[![Logo](https://github.com/YorDN/LibManage/blob/master/LibManage%20Logo-med.png?raw=true)](http://batedan4o-001-site1.jtempurl.com/)

  ## Your personal digital library — *organized, social, and easy to use!*

[![language](https://img.shields.io/badge/language-C%23-239120)](https://learn.microsoft.com/en/dotnet/csharp/tour-of-csharp/overview)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/w/YorDN/LibManage)](https://github.com/YorDN/LibManage/commits/master/)
[![GitHub repo size](https://img.shields.io/github/repo-size/YorDN/LibManage)](#)
[![GitHub last commit](https://img.shields.io/github/last-commit/YorDN/LibManage)](#)
[![GitHub issues](https://img.shields.io/github/issues/YorDN/LibManage)](#)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/YorDN/LibManage)](#)

<img width="1695" height="950" alt="Screenshot_22" src="https://github.com/user-attachments/assets/2a830a3d-ea14-40a7-81b2-afa39109e5da" />

</div>

## Table of Contents
- [About](#-about)
- [Technologies Used](#%EF%B8%8F-technologies)
  - [Requirements](#%EF%B8%8F-requirements)
- [Installation](#-installation)
  - [Seeded data](#-seeded-data)

## ❓ About
**LibManage** is a library management tool, based on the web. It is build using ASP.NET Core MVC with .NET 8 for the perpose of being final project for the SoftUni's ASP.NET Advanced course! It utilises key OOP concepts following the *Model-View-Controller* pattern. You can access the full website from [here](http://batedan4o-001-site1.jtempurl.com/) (I am sorry it is not https, have no money 😶‍🌫️)

## ✨ Features
**Libmanage** has many different features, all regarding user's interaaction with a digital library. The main funtions are as follows:
### 🔄 Renting
Every user can rent (borrow) a book. The books are three types: *physical, digital and audio*. Audio and digital books can be borrowed at the same time by different users, while physical books can only be rented by one user at a time (to simulate real library). The automatic renting period is 14 days. Once the 14 days are over the book is returned automatically. During those 14 days the user can read or listen to his borrowed books (if they are digital or audio respectivly). At any point of the renting period the user can return their borrowed book.

![2025-08-0415-11-41-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/d48e6450-a661-4522-bbee-de567e22c221)

### 📖 Reading
Once a user borrows a book (refer to the [renting](#-renting) section) and if the book is digital, the user can read it. The books in the library are stored in .EPUB (*electronic publication*) format. The system tracks the user's progress and loads the last read chapter when the user wants to continue reading the book. Even after returning the book, the progress is stored for future borrows of the book by the user. The reader is made with the VersOne.Epub NuGet package (for more information refer to their [official website](https://os.vers.one/EpubReader/) or their [github](https://github.com/vers-one/EpubReader))

![2025-08-0613-04-45-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/c27c462c-b9a5-4d69-b87d-53a00cf3d604)

### 🔊 Listenting
Simular to [reading](#-reading), once borrowed, audio books can be listened to by their borrowers. The audio books are stored in .MP3 format. 
### ⭐ Reviewing
Every user can review a book. The reviews are star based from 1-5 and consist of rating and user comment. Uppon successfully writing a review, the review will be moderated by one of the managers. If the review has harmful messages or hateful speech, the review will not be approved. One user can leave only one review per book (this is done for the perpose of avoiding spam). If the user's review is not approved by a manager only the user (and the manager) will see it. If the manager deletes the user's review, the review will be lost and the user will have the ability to leave a new one. Approved reviews are publically vissable and are calculated for the book's average rating. 

![2025-08-0613-10-32-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/abef351f-607e-465b-a97b-475bc1ced35d)

### 👑 Admin Features
Along with the upper mentioned features, admins have a whole different set of capabilities. These include:
- Adding/Editing/Deleting Books
- Adding/Editing/Deleting Authors
- Adding/Editing/Deleting Publishers
- Have access to the admin dashboard, containing useful info about the library
- Assign roles and delete (deactivate) users.
![2025-08-0612-58-46-ezgif com-video-to-gif-converter](https://github.com/user-attachments/assets/3f2aa714-2d7c-4a1e-a276-4819021cfd76)


## ⚙️ Technologies
[![Microsoft SQL Server](https://custom-icon-badges.demolab.com/badge/Microsoft%20SQL%20Server-CC2927?logo=mssqlserver-white&logoColor=white)](#)
[![.NET](https://img.shields.io/badge/.NET-512BD4?logo=dotnet&logoColor=fff)](#)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?logo=bootstrap&logoColor=fff)](#)
[![Chart.js](https://img.shields.io/badge/Chart.js-FF6384?logo=chartdotjs&logoColor=fff)](#)
[![Visual Studio](https://custom-icon-badges.demolab.com/badge/Visual%20Studio-5C2D91.svg?&logo=visualstudio&logoColor=white)](#)

As mentioned **LibManage** uses ASP.NET Core MVC with .NET 8. For the database it uses SQL Server (MsSql) 2022. Authorization and authentication are done via the Microsoft.Identity systen. The frontend is mostly done with Bootstrap. On the admin pages, it utilizes the chart.js framework for displaying useful information to the admin. 

### ⚠️ Requirements
Every person who wants to work on the website should have these technologies (and tools) installed:
- .NET 8 or higher
- SQL Server 2022 or newer
- ASP.NET Core Enviorment
- IDE, supporting the .NET Framework (Prefferably Microsoft Visual Studio or JetBrains Rider)

## 🟢 Installation
LibManage doesn't require installation to work. You can visit the website from [here](http://batedan4o-001-site1.jtempurl.com/). 

But if you wish to install it, the installation process is straightforward. First you have to clone the repository in your desired IDE (or as a matter of fact in your file system). From there you have to make a database and connect it via connection string in either the appsettings.json or user secrets (Refer to the [technologies](#%EF%B8%8F-technologies) section for requirements regarding technology). \

The connection string should be in the following format
``` bash
"ConnectionStrings": {
  "DefaultConnection": "Server=your_server;Database=your_db;User Id=...;Password=...;"
}
```
or if you have a different setup, refer to [this](https://www.connectionstrings.com/sql-server/) website 

From then you only have to start the application. The DbSeeder class will handle the migration/seeding of the database. 

### 📃 Seeded data

Upon successful startup the database will fill with the following data:
- **3 authors** 👤 \
  Including *Dimitar Talev*, *Stephen King* and *Ivan Vazov*. Pioneers of both bulgarian and world literature and poetry.
- **2 publishers** 👤 \
  Including *Zahari Stoyanov* and *Pleiad*
- **3 books** 📚 \
  Including "The Iron Candlestick" (Железният светилник), "The Volunteers at Shipka" (Опълченците на Шипка) and "The Dark Tower: The Gunslinger" (Тъмната кула 1: Стрелецът). Each of them representing the *three different types of books* available in the library: physical, digital and audio. Please refer to the [features](#-features) section for more information.
