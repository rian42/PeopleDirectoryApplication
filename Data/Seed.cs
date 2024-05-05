using Microsoft.AspNetCore.Identity;
using PeopleDirectoryApplication.Data.Enum;
using PeopleDirectoryApplication.Models;
using PeopleDirectoryApplication.Models.Identity;

namespace PeopleDirectoryApplication.Data
{
    public class Seed
    {
        public Seed()
        { }

        public static void SeedPersonData(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                context.Database.EnsureCreated();

                if (!context.Persons.Any())
                {
                    context.Persons.AddRange(new List<Person>()
                    {
                        new Person
                        {
                            Name = "Siyabonga",
                            Surname = "Mthembu",
                            Country = "South Africa",
                            City = "Durban",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "siyabonga@example.com"
                        },
                        new Person
                        {
                            Name = "Noluthando",
                            Surname = "Van Wyk",
                            Country = "South Africa",
                            City = "Cape Town",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "noluthando@example.com"
                        },
                        new Person
                        {
                            Name = "Thabo",
                            Surname = "Botha",
                            Country = "South Africa",
                            City = "Johannesburg",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "thabo@example.com"
                        },
                        new Person
                        {
                            Name = "Lebohang",
                            Surname = "Nkosi",
                            Country = "South Africa",
                            City = "Pretoria",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "lebohang@example.com"
                        },
                        new Person
                        {
                            Name = "Bongani",
                            Surname = "Dlamini",
                            Country = "South Africa",
                            City = "Bloemfontein",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "bongani@example.com"
                        },
                        new Person
                        {
                            Name = "Nomfundo",
                            Surname = "Mhlongo",
                            Country = "South Africa",
                            City = "Port Elizabeth",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "nomfundo@example.com"
                        },
                        new Person
                        {
                            Name = "Andile",
                            Surname = "Ngubane",
                            Country = "South Africa",
                            City = "East London",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "andile@example.com"
                        },
                        new Person
                        {
                            Name = "Thembi",
                            Surname = "Mkhize",
                            Country = "South Africa",
                            City = "Nelspruit",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "thembi@example.com"
                        },
                        new Person
                        {
                            Name = "Sipho",
                            Surname = "Van Der Merwe",
                            Country = "South Africa",
                            City = "Kimberley",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "sipho@example.com"
                        },
                        new Person
                        {
                            Name = "Nomsa",
                            Surname = "Nkosi",
                            Country = "South Africa",
                            City = "Mbombela",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "nomsa@example.com"
                        },
                        new Person
                        {
                            Name = "Kawana",
                            Surname = "Ndeunyema",
                            Country = "Namibia",
                            City = "Windhoek",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "kawana@example.com"
                        },
                        new Person
                        {
                            Name = "Liina",
                            Surname = "Shilunga",
                            Country = "Namibia",
                            City = "Walvis Bay",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "liina@example.com"
                        },
                        new Person
                        {
                            Name = "Moses",
                            Surname = "Shivute",
                            Country = "Namibia",
                            City = "Rundu",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "moses@example.com"
                        },
                        new Person
                        {
                            Name = "Naimi",
                            Surname = "Shikongo",
                            Country = "Namibia",
                            City = "Swakopmund",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "naimi@example.com"
                        },
                        new Person
                        {
                            Name = "Josef",
                            Surname = "Nekongo",
                            Country = "Namibia",
                            City = "Oshakati",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "josef@example.com"
                        },
                        new Person
                        {
                            Name = "Anna",
                            Surname = "Nghipondoka",
                            Country = "Namibia",
                            City = "Grootfontein",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "anna@example.com"
                        },
                        new Person
                        {
                            Name = "Simon",
                            Surname = "Shilongo",
                            Country = "Namibia",
                            City = "Otjiwarongo",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "simon@example.com"
                        },
                        new Person
                        {
                            Name = "Helena",
                            Surname = "Nghilumbwa",
                            Country = "Namibia",
                            City = "Keetmanshoop",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "helena@example.com"
                        },
                        new Person
                        {
                            Name = "Martin",
                            Surname = "Shikongo",
                            Country = "Namibia",
                            City = "Okahandja",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "martin@example.com"
                        },
                        new Person
                        {
                            Name = "Ndapewa",
                            Surname = "Nghilumbwa",
                            Country = "Namibia",
                            City = "Tsumeb",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "ndapewa@example.com"
                        },
                        new Person
                        {
                            Name = "Lukas",
                            Surname = "Müller",
                            Country = "Germany",
                            City = "Berlin",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "lukas@example.com"
                        },
                        new Person
                        {
                            Name = "Sophie",
                            Surname = "Schmidt",
                            Country = "Germany",
                            City = "Hamburg",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "sophie@example.com"
                        },
                        new Person
                        {
                            Name = "Maximilian",
                            Surname = "Hofmann",
                            Country = "Germany",
                            City = "Munich",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "maximilian@example.com"
                        },
                        new Person
                        {
                            Name = "Lena",
                            Surname = "Becker",
                            Country = "Germany",
                            City = "Frankfurt",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "lena@example.com"
                        },
                        new Person
                        {
                            Name = "Finn",
                            Surname = "Wagner",
                            Country = "Germany",
                            City = "Stuttgart",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "finn@example.com"
                        },
                        new Person
                        {
                            Name = "Sergio",
                            Surname = "García",
                            Country = "Spain",
                            City = "Madrid",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "sergio@example.com"
                        },
                        new Person
                        {
                            Name = "Lucía",
                            Surname = "Martínez",
                            Country = "Spain",
                            City = "Barcelona",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "lucia@example.com"
                        },
                        new Person
                        {
                            Name = "Javier",
                            Surname = "Fernández",
                            Country = "Spain",
                            City = "Valencia",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "javier@example.com"
                        },
                        new Person
                        {
                            Name = "Elena",
                            Surname = "González",
                            Country = "Spain",
                            City = "Seville",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "elena@example.com"
                        },
                        new Person
                        {
                            Name = "Alejandro",
                            Surname = "Rodríguez",
                            Country = "Spain",
                            City = "Zaragoza",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "alejandro@example.com"
                        },
                        new Person
                        {
                            Name = "Luca",
                            Surname = "Rossi",
                            Country = "Italy",
                            City = "Rome",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "luca@example.com"
                        },
                        new Person
                        {
                            Name = "Giulia",
                            Surname = "Bianchi",
                            Country = "Italy",
                            City = "Milan",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "giulia@example.com"
                        },
                        new Person
                        {
                            Name = "Marco",
                            Surname = "Rizzo",
                            Country = "Italy",
                            City = "Naples",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "marco@example.com"
                        },
                        new Person
                        {
                            Name = "Chiara",
                            Surname = "Romano",
                            Country = "Italy",
                            City = "Turin",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "chiara@example.com"
                        },
                        new Person
                        {
                            Name = "Alessandro",
                            Surname = "Moretti",
                            Country = "Italy",
                            City = "Palermo",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "alessandro@example.com"
                        },
                        new Person
                        {
                            Name = "James",
                            Surname = "Smith",
                            Country = "United Kingdom",
                            City = "London",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "james@example.com"
                        },
                        new Person
                        {
                            Name = "Emily",
                            Surname = "Johnson",
                            Country = "United Kingdom",
                            City = "Manchester",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "emily@example.com"
                        },
                        new Person
                        {
                            Name = "Oliver",
                            Surname = "Williams",
                            Country = "United Kingdom",
                            City = "Birmingham",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "oliver@example.com"
                        },
                        new Person
                        {
                            Name = "Charlotte",
                            Surname = "Jones",
                            Country = "United Kingdom",
                            City = "Liverpool",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "charlotte@example.com"
                        },
                        new Person
                        {
                            Name = "Jack",
                            Surname = "Brown",
                            Country = "United Kingdom",
                            City = "Glasgow",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "jack@example.com"
                        },
                        new Person
                        {
                            Name = "Amelia",
                            Surname = "Davis",
                            Country = "United Kingdom",
                            City = "Edinburgh",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "amelia@example.com"
                        },
                        new Person
                        {
                            Name = "Harry",
                            Surname = "Miller",
                            Country = "United Kingdom",
                            City = "Leeds",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "harry@example.com"
                        },
                        new Person
                        {
                            Name = "Grace",
                            Surname = "Wilson",
                            Country = "United Kingdom",
                            City = "Bristol",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "grace@example.com"
                        },
                        new Person
                        {
                            Name = "Thomas",
                            Surname = "Moore",
                            Country = "United Kingdom",
                            City = "Cardiff",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Male,
                            EmailAddress = "thomas@example.com"
                        },
                        new Person
                        {
                            Name = "Sophia",
                            Surname = "Taylor",
                            Country = "United Kingdom",
                            City = "Belfast",
                            ProfilePicture = "https://www.avatarsinpixels.com/Public/images/previews/minipix4.png",
                            MobileNumber = "1234567890",
                            Gender = Gender.Female,
                            EmailAddress = "sophia@example.com"
                        }
                    });

                    context.SaveChanges();
                }
            }
        }

        public static async Task SeedRolesAsync(IApplicationBuilder applicationBuilder, IConfiguration config)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();

                var newAdminUser = new User()
                {
                    UserName = config.GetSection("AdminUser")["UserName"],
                    Surname = config.GetSection("AdminUser")["Surname"],
                    Email = config.GetSection("AdminUser")["Email"],
                    EmailConfirmed = true,
                };

                await userManager.CreateAsync(newAdminUser, "!Rr1234");
                await userManager.AddToRoleAsync(newAdminUser, UserRoles.Admin);
            }
        }
    }
}