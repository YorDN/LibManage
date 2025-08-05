
<div align="center">
  
[![Logo](https://github.com/YorDN/LibManage/blob/master/LibManage%20Logo-med.png?raw=true)](http://batedan4o-001-site1.jtempurl.com/)

  ## Your personal digital library — *organized, social, and easy to use!*

[![language](https://img.shields.io/badge/language-C%23-239120)](https://learn.microsoft.com/en/dotnet/csharp/tour-of-csharp/overview)
[![GitHub commit activity](https://img.shields.io/github/commit-activity/w/YorDN/LibManage)](https://github.com/YorDN/LibManage/commits/master/)
![GitHub repo size](https://img.shields.io/github/repo-size/YorDN/LibManage)
![GitHub last commit](https://img.shields.io/github/last-commit/YorDN/LibManage)
![GitHub issues](https://img.shields.io/github/issues/YorDN/LibManage)
![GitHub pull requests](https://img.shields.io/github/issues-pr/YorDN/LibManage)

<img width="1695" height="950" alt="Screenshot_22" src="https://github.com/user-attachments/assets/2a830a3d-ea14-40a7-81b2-afa39109e5da" />

</div>

## Table of Contents
- [About](#-about)
- [Installation](#-installation)
  - [Seeded data](#-seeded-data)

## ❓ About
**LibManage** is a library management tool, based on the web. It is build using ASP.NET Core MVC with .NET 8 for the perpose of being final project for the SoftUni's ASP.NET Advanced course! It utilises key OOP concepts following the *Model-View-Controller* pattern. You can access the full website from [here](http://batedan4o-001-site1.jtempurl.com/) (I am sorry it is not https, have no money 😶‍🌫️)

## 🟢 Installation
LibManage doesn't require installation to work. You can visit the website from [here](http://batedan4o-001-site1.jtempurl.com/). 

But if you wish to install it, the installation process is straightforward. First you have to clone the repository in your desired IDE (or as a matter of fact in your file system). From there you have to make a database and connect it via connection string in either the appsettings.json or user secrets (Refer to the [technologies](#-technology) section for requirements regarding technology). \

The connection string should be in the following format
``` bash
"ConnectionStrings": {
  "DefaultConnection": "Server=your_server;Database=your_db;User Id=...;Password=...;"
}
```
or if you have different setup, refer to [this](https://www.connectionstrings.com/sql-server/) website 

From then you only have to start the application. The DbSeeder class will handle the migration/seeding of the database. 

### 📃 Seeded data

Upon successful startup the database will fill with the following data:
- **3 authors** 👤 \
  Including *Dimitar Talev*, *Stephen King* and *Ivan Vazov*. Pioneers of both bulgarian and world literature and poetry.
- **2 publishers** 👤 \
  Including *Zahari Stoyanov* and *Pleiad*
- **3 books** 📚 \
  Including "The Iron Candlestick" (Железният светилник), "The Volunteers at Shipka" (Опълченците на Шипка) and "The Dark Tower: The Gunslinger" (Тъмната кула 1: Стрелецът). Each of them representing the *three different types of books* available in the library: physical, digital and audio. Please refer to the [features](#-features) section for more information.
