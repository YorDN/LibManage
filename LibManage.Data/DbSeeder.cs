using LibManage.Common;
using LibManage.Data.Models.Library;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using static LibManage.Data.Models.Library.Book;

namespace LibManage.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();

            await SeedRolesAndUsersAsync(scope.ServiceProvider);
            await SeedPublishersAsync(context);
            await SeedAuthorsAsync(context);
            await SeedBooksAsync(context);
        }
        public static async Task SeedRolesAndUsersAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roleNames = { UserRoles.Admin, UserRoles.Manager, UserRoles.User };

            foreach (var role in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            string adminEmail = "admin@abv.bg";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new User()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    ProfilePicture = "/uploads/pfps/user/DefaultUser.png"
                };
                await userManager.CreateAsync(admin, "Admin123");
                await userManager.AddToRoleAsync(admin, "Admin");

            }

            var managerEmail = "manager@abv.bg";
            if (await userManager.FindByEmailAsync(managerEmail) == null)
            {
                var manager = new User
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true,
                    ProfilePicture = "/uploads/pfps/user/DefaultUser.png"
                };

                await userManager.CreateAsync(manager, "Manager123");
                await userManager.AddToRoleAsync(manager, "Manager");
            }

            var userEmail = "user@abv.bg";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var user = new User
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    ProfilePicture = "/uploads/pfps/user/DefaultUser.png"
                };

                await userManager.CreateAsync(user, "User1234");
                await userManager.AddToRoleAsync(user, "User");
            }
        }
        public static async Task SeedPublishersAsync(ApplicationDbContext context)
        {
            if (context.Publishers.Any())
                return;

            context.Publishers.AddRange(
                new Publisher
                {
                    Name = "Zahari Stoyanov",
                    Country = "BG",
                    Website = "https://zstoyanov.com",
                    LogoUrl = "/uploads/pfps/publisher/dc005b6b-1e23-4b26-9563-8906ecbfeedf.jpg",
                    Description = "Издателство „Захарий Стоянов” е основано през 1997 г. За изтеклите 26 години успява да издаде " +
                    "над 5000 заглавия в общ тираж над 3 500 000 екземпляра. Някои от заглавията продължават стратегическата позиция " +
                    "да се отпечатват и преиздават върхови творби на отечествената литературна класика, както и нови, оригинални" +
                    " книги на съвременни български писатели. Издателство „Захарий Стоянов” успява да създаде собствен " +
                    "естетически и литературнохудожествен кръг от най-ярки съвременни писатели, учени, художници, режисьори, артисти и музиканти."
                },
                new Publisher
                {
                    Name = "Pleiad Books",
                    LogoUrl = "/uploads/pfps/publisher/DefaultPublisher.png",
                    Country = "BG",
                    Website = "https://pleiadbooks.com",
                    Description = "Годината е 1991. Изгрява ново съзвездие - издателство Плеяда - и заблестява с ярка светлина." +
                    "\r\nПървата стъпка е направена с книжки с популярни приказки, илюстрирани от български художници. " +
                    "Много труд, много ентусиазъм... и ето, че на книжния пазар се появява Сейлъмс Лот от Стивън Кинг. " +
                    "Роман, с който се утвърждава традицията всяка нова книга на прочутия автор да бъде издавана от Плеяда.\r\n" +
                    "Не след дълго настъпва епохата на книгите-игри. Във времето, когато компютърът още не е влязъл в почти всеки дом, " +
                    "тези книги, развиващи способността за творческо мислене и инициативността, бързо добиват широка популярност.\r\nНаред " +
                    "с романите на Стивън Кинг Плеяда започва да публикува книгите на друг майстор в този жанр - Дийн Кунц " +
                    "(досега са издадени над трийсет заглавия), които също са радушно приети от читателите.\r\nГолям успех " +
                    "имат романите на Колийн Маккълоу за Древния Рим. В продължение на десет години читателите с нетърпение " +
                    "очакваха появяването на всяка следваща книга.\r\nНе са забравени и романтични"
                });

            await context.SaveChangesAsync();
        }
        public static async Task SeedAuthorsAsync(ApplicationDbContext context)
        {
            if (context.Authors.Any())
                return;

            context.Authors.AddRange(
                new Author
                {
                    FullName = "Stephen King",
                    Photo = "/uploads/pfps/author/fa11f4e1-dbae-4222-8f36-9f19a0cb6ded.jpg",
                    DateOfBirth = new DateTime(1947, 9, 21),
                    DateOfDeath = null,
                    Biography = "Той несъмнено е Кралят на ужаса. Името му е гаранция за безсънни нощи и оживели кошмари. " +
                    "Той е Стивън Кинг.\r\nРоден е на 21 септември 1947 г. в Портланд. Споделя в интервю, че прочитането на томче с " +
                    "разкази от Х. Ф. Лъфкрафт го накарало да се почувства у дома си и това определило бъдещето му. Издаването на първият " +
                    "му роман Кери (1974) му позволява да се отдаде изцяло на писането. Следват Сейлъм'с Лот, Сияние, Мъртвата зона, " +
                    "Живата факла, Куджо, Кристин, Гробище за домашни любимци, То, Мизъри, Зеленият път, сборници с разкази, поредицата " +
                    "Тъмната кула и още, и още..."
                },
                new Author
                {
                    FullName = "Dimitar Talev",
                    Photo = "/uploads/pfps/author/29fcf156-ae19-4746-86e9-d1f27208b761.jpg",
                    DateOfBirth = new DateTime(1898, 9, 1),
                    DateOfDeath = new DateTime(1966, 10, 20),
                    Biography = "Димитър Талев Петров Палисламов е български писател и журналист. Роден през 1898 година в град Прилеп в Македония," +
                    " той завършва Софийския университет и от средата на 20-те години се занимава с литературна дейност, като дълго време " +
                    "е журналист в популярните вестници „Македония“ и „Зора“. След Деветосептемврийския преврат от 1944 година и установяването " +
                    "на тоталитарния комунистически режим в страната е обявен за „великобългарски шовинист“ и прекарва няколко години в затвори" +
                    " и концентрационни лагери. След освобождаването си е постепенно реабилитиран от режима и до смъртта си през 1966 година " +
                    "издава поредица от исторически романи, сред които „Преспанската тетралогия“ – „Железният светилник“, „Преспанските камбани“, " +
                    "„Илинден“ и „Гласовете ви чувам“ – които му донасят голяма популярност и го утвърждават като един от класиците на " +
                    "българската литература.\r\n\r\n"
                },
                new Author
                {
                    FullName = "Иван Вазов",
                    Photo = "/uploads/pfps/author/fc5f37d3-318b-423f-8186-2e3a37e622be.jpeg",
                    DateOfBirth = new DateTime(1850, 7, 9),
                    DateOfDeath = new DateTime(1921, 9, 22),
                    Biography = "Иван Минчов Вазов е български поет, писател и драматург, наричан често „Патриарх на българската литература“. " +
                    "Творчеството на Вазов е отражение на две исторически епохи – Възраждането и следосвобожденска България. " +
                    "Иван Вазов е академик на Българската академия на науките и министър на народното просвещение от 7 септември 1897 г. " +
                    "до 30 януари 1899 г. от Народната партия."
                });
            await context.SaveChangesAsync();
        }
        public static async Task SeedBooksAsync(ApplicationDbContext context)
        {
            if (context.Books.Any())
                return;

            var sKing = await context.Authors.FirstOrDefaultAsync(a => a.FullName == "Stephen King");
            var dTalev = await context.Authors.FirstOrDefaultAsync(a => a.FullName == "Dimitar Talev");
            var iVazov = await context.Authors.FirstOrDefaultAsync(a => a.FullName == "Иван Вазов");

            var publisherZ = await context.Publishers.FirstOrDefaultAsync(p => p.Name == "Zahari Stoyanov");
            var publisherP = await context.Publishers.FirstOrDefaultAsync(p => p.Name == "Pleiad Books");

            if (sKing is null || dTalev is null || iVazov is null || publisherZ is null || publisherP is null)
                return;

            context.Books.AddRange(
                new Book
                {
                    Title = "Железният светилник",
                    ISBN = "9789540907765",
                    Language = "Bulgarian",
                    Genre = "History",
                    Type = BookType.Physical,
                    Description = "„Железният светилник“ е първата книга от известната тетралогия на Димитър Талев. " +
                    "В него са отразени българските борби за политическа и църковна независимост. Романът проследява съдбата на " +
                    "едно типично възрожденско семейство. Концентрира се най-вече върху фигурата на Лазар Глаушев и ролята му " +
                    "в борбата против фанариотите и задружните усилия на повечето жители на Преспа за установяване на българска църква.",
                    Cover = "/uploads/covers/9a261f0b-a39e-4a5b-9706-909060e15e95.jpg",
                    AuthorId = dTalev.Id,
                    PublisherId = publisherZ.Id,
                },
                new Book
                {
                    Title = "Тъмната кула 1: Стрелецът",
                    ISBN = "9789544094003",
                    ReleaseDate = new DateTime(2019, 6, 19),
                    Language = "Bulgarian",
                    Genre = "Thriller",
                    Type = BookType.Digital,
                    Description = "„Тъмната кула“ се смята за едно от най-забележителните произведения на Стивън Кинг. Иконичната поредица на " +
                    "майстора на ужаса събира в себе си елементи на научна фантастика, футуристична дистопия, спагети уестърн и " +
                    "фентъзи.\r\nЧовекът в черно бягаше през пустинята и Стрелецът го следваше. " +
                    "Преди повече от 40 години светът прочита това изречение… и вече не е същият.\r\nТака започва поредицата за " +
                    "Тъмната кула, най-мащабната и най-епичната творба на Стивън Кинг.",
                    Cover = "/uploads/covers/3231d6ed-78be-4579-94a4-79fecc6ad4bd.jpg",
                    BookFilePath = "/uploads/files/digital/0ddc29e6-4113-480f-a0b2-52d693789f17.epub",
                    AuthorId = sKing.Id,
                    PublisherId = publisherP.Id,
                },
                new Book
                {
                    Title = "Опълченците на Шипка",
                    ISBN = "Audio",
                    Language = "Bulgarian",
                    Genre = "History",
                    Type = BookType.Audio,
                    Duration = new TimeSpan(0, 5, 40),
                    Cover = "/uploads/covers/no_cover_available.png",
                    BookFilePath = "/uploads/files/audio/6b074947-c71c-4c4e-a53d-654cafbd3968.mp3",
                    AuthorId = iVazov.Id,
                    PublisherId = publisherP.Id,
                });

            await context.SaveChangesAsync();
        }
    }
}
