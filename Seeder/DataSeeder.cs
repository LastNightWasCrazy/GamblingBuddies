using GamblingBuddies.Models;

namespace GamblingBuddies
{
    public static class DataSeeder
    {
        public static void Seed(AppDbContext context)
        {
            SeedRolesAndUsers(context);

            if (context.Set<Hall>().Any() ||
                context.Set<Game>().Any() ||
                context.Set<GameTable>().Any())
            {
                return;
            }

            SeedBusinessData(context);
        }

        private static void SeedRolesAndUsers(AppDbContext context)
        {
            var adminRole = GetOrCreateRole(context, "Administrator", "Administrator systemu");
            var managerRole = GetOrCreateRole(context, "Manager", "Manager sali");
            var employeeRole = GetOrCreateRole(context, "Employee", "Pracownik");

            GetOrCreateEmployeePosition(context, "Dealer", "Krupier");
            GetOrCreateEmployeePosition(context, "Manager", "Kierownik");

            GetOrCreateEmployeeStatus(context, "Active", "Aktywny");
            GetOrCreateEmployeeStatus(context, "Inactive", "Nieaktywny");

            GetOrCreatePaymentStatus(context, "Pending", "Oczekująca");
            GetOrCreatePaymentStatus(context, "Paid", "Opłacona");
            GetOrCreatePaymentStatus(context, "Rejected", "Odrzucona");

            GetOrCreatePaymentMethod(context, "Cash", "Gotówka");
            GetOrCreatePaymentMethod(context, "Card", "Karta");

            GetOrCreateHallType(context, "Standard", "Sala standardowa");
            GetOrCreateHallType(context, "VIP", "Sala VIP");
            GetOrCreateHallType(context, "Tournament", "Sala turniejowa");

            GetOrCreateGameCategory(context, "Cards", "Gry karciane");
            GetOrCreateGameCategory(context, "Roulette", "Ruletka");

            GetOrCreateSessionStatus(context, "Planned", "Zaplanowana");
            GetOrCreateSessionStatus(context, "Finished", "Zakończona");
            GetOrCreateSessionStatus(context, "Active", "Aktywna");
            GetOrCreateSessionStatus(context, "Cancelled", "Anulowana");

            GetOrCreateReservationStatus(context, "Pending", "Oczekująca");
            GetOrCreateReservationStatus(context, "Confirmed", "Potwierdzona");
            GetOrCreateReservationStatus(context, "Cancelled", "Anulowana");

            var admin = GetOrCreateUser(
                context,
                "admin",
                "admin@gamblingbuddies.local",
                "admin123"
            );

            var manager = GetOrCreateUser(
                context,
                "manager",
                "manager@gamblingbuddies.local",
                "manager123"
            );

            var employeeUser = GetOrCreateUser(
                context,
                "employee",
                "employee@gamblingbuddies.local",
                "employee123"
            );

            AddUserRoleIfMissing(context, admin, adminRole);
            AddUserRoleIfMissing(context, manager, managerRole);
            AddUserRoleIfMissing(context, employeeUser, employeeRole);

            context.SaveChanges();
        }

        private static void SeedBusinessData(AppDbContext context)
        {
            var dealerPosition = GetOrCreateEmployeePosition(context, "Dealer", "Krupier");
            var managerPosition = GetOrCreateEmployeePosition(context, "Manager", "Kierownik");

            var activeEmployeeStatus = GetOrCreateEmployeeStatus(context, "Active", "Aktywny");

            var standardHallType = GetOrCreateHallType(context, "Standard", "Sala standardowa");
            var vipHallType = GetOrCreateHallType(context, "VIP", "Sala VIP");
            var tournamentHallType = GetOrCreateHallType(context, "Tournament", "Sala turniejowa");

            var cardsCategory = GetOrCreateGameCategory(context, "Cards", "Gry karciane");
            var rouletteCategory = GetOrCreateGameCategory(context, "Roulette", "Ruletka");
            var plannedStatus = GetOrCreateSessionStatus(context, "Planned", "Zaplanowana");

            var admin = context.Set<SystemUser>().First(u => u.Login == "admin");
            var manager = context.Set<SystemUser>().First(u => u.Login == "manager");
            var employeeUser = context.Set<SystemUser>().First(u => u.Login == "employee");

            var employee1 = new Employee
            {
                SystemUserId = manager.SystemUserId,
                FirstName = "Jan",
                LastName = "Kowalski",
                Phone = "500100200",
                HireDate = DateTime.Now.AddMonths(-6),
                PositionId = managerPosition.EmployeePositionDictionaryId,
                EmployeeStatusId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            var employee2 = new Employee
            {
                SystemUserId = employeeUser.SystemUserId,
                FirstName = "Anna",
                LastName = "Nowak",
                Phone = "500300400",
                HireDate = DateTime.Now.AddMonths(-3),
                PositionId = dealerPosition.EmployeePositionDictionaryId,
                EmployeeStatusId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            context.Set<Employee>().AddRange(employee1, employee2);

            var player1 = new Player
            {
                FirstName = "Piotr",
                LastName = "Zieliński",
                Email = "piotr@example.com",
                Phone = "600100200",
                CreatedAt = DateTime.Now
            };

            var player2 = new Player
            {
                FirstName = "Maria",
                LastName = "Wiśniewska",
                Email = "maria@example.com",
                Phone = "600300400",
                CreatedAt = DateTime.Now
            };

            context.Set<Player>().AddRange(player1, player2);
            context.SaveChanges();

            var hall1 = new Hall
            {
                Name = "Main Hall",
                HallTypeId = standardHallType.HallTypeId,
                Description = "Główna sala kasyna z klasycznymi grami stołowymi.",
                IsActive = true
            };

            var hall2 = new Hall
            {
                Name = "VIP Hall",
                HallTypeId = vipHallType.HallTypeId,
                Description = "Ekskluzywna sala dla klientów VIP.",
                IsActive = true
            };

            var hall3 = new Hall
            {
                Name = "Tournament Hall",
                HallTypeId = tournamentHallType.HallTypeId,
                Description = "Sala turniejowa przeznaczona do większych rozgrywek.",
                IsActive = true
            };

            context.Set<Hall>().AddRange(hall1, hall2, hall3);
            context.SaveChanges();

            var table1 = new GameTable
            {
                HallId = hall1.HallId,
                TableNumber = 101,
                MinPlayers = 1,
                MaxPlayers = 6,
                IsActive = true
            };

            var table2 = new GameTable
            {
                HallId = hall1.HallId,
                TableNumber = 102,
                MinPlayers = 2,
                MaxPlayers = 8,
                IsActive = true
            };

            var table3 = new GameTable
            {
                HallId = hall1.HallId,
                TableNumber = 103,
                MinPlayers = 1,
                MaxPlayers = 5,
                IsActive = true
            };

            var table4 = new GameTable
            {
                HallId = hall2.HallId,
                TableNumber = 201,
                MinPlayers = 1,
                MaxPlayers = 5,
                IsActive = true
            };

            var table5 = new GameTable
            {
                HallId = hall2.HallId,
                TableNumber = 202,
                MinPlayers = 2,
                MaxPlayers = 6,
                IsActive = true
            };

            var table6 = new GameTable
            {
                HallId = hall3.HallId,
                TableNumber = 301,
                MinPlayers = 2,
                MaxPlayers = 10,
                IsActive = true
            };

            context.Set<GameTable>().AddRange(table1, table2, table3, table4, table5, table6);
            context.SaveChanges();

            var seats = new List<Seat>();

            AddSeatsForTable(seats, table1);
            AddSeatsForTable(seats, table2);
            AddSeatsForTable(seats, table3);
            AddSeatsForTable(seats, table4);
            AddSeatsForTable(seats, table5);
            AddSeatsForTable(seats, table6);

            context.Set<Seat>().AddRange(seats);
            context.SaveChanges();

            var poker = new Game
            {
                Name = "Poker",
                GameCategoryId = cardsCategory.GameCategoryId,
                Description = "Gra karciana oparta na strategii, blefie i analizie przeciwników.",
                IsActive = true
            };

            var blackjack = new Game
            {
                Name = "Blackjack",
                GameCategoryId = cardsCategory.GameCategoryId,
                Description = "Gra karciana, której celem jest uzyskanie wyniku jak najbliższego 21.",
                IsActive = true
            };

            var roulette = new Game
            {
                Name = "Roulette",
                GameCategoryId = rouletteCategory.GameCategoryId,
                Description = "Klasyczna gra losowa oparta na kole ruletki.",
                IsActive = true
            };

            var wojna = new Game
            {
                Name = "Wojna",
                GameCategoryId = cardsCategory.GameCategoryId,
                Description = "Prosta gra karciana, w której wygrywa gracz z wyższą kartą.",
                IsActive = true
            };

            context.Set<Game>().AddRange(poker, blackjack, roulette, wojna);
            context.SaveChanges();

            var texasHoldem = new GameVariant
            {
                GameId = poker.GameId,
                Name = "Texas Holdem",
                RulesDescription = "Standardowe zasady Texas Holdem.",
                DefaultMinBet = 10,
                DefaultMaxBet = 500,
                IsActive = true
            };

            var blackjackClassic = new GameVariant
            {
                GameId = blackjack.GameId,
                Name = "Blackjack Classic",
                RulesDescription = "Klasyczne zasady blackjacka.",
                DefaultMinBet = 10,
                DefaultMaxBet = 400,
                IsActive = true
            };

            var europeanRoulette = new GameVariant
            {
                GameId = roulette.GameId,
                Name = "European Roulette",
                RulesDescription = "Ruletka europejska z pojedynczym zerem.",
                DefaultMinBet = 5,
                DefaultMaxBet = 300,
                IsActive = true
            };

            var wojnaClassic = new GameVariant
            {
                GameId = wojna.GameId,
                Name = "Wojna Classic",
                RulesDescription = "Podstawowy wariant gry Wojna.",
                DefaultMinBet = 5,
                DefaultMaxBet = 200,
                IsActive = true
            };

            context.Set<GameVariant>().AddRange(texasHoldem, blackjackClassic, europeanRoulette, wojnaClassic);
            context.SaveChanges();

            context.Set<GameTableGame>().AddRange(
                new GameTableGame
                {
                    GameTableId = table1.GameTableId,
                    GameId = poker.GameId
                },
                new GameTableGame
                {
                    GameTableId = table1.GameTableId,
                    GameId = blackjack.GameId
                },

                new GameTableGame
                {
                    GameTableId = table2.GameTableId,
                    GameId = blackjack.GameId
                },
                new GameTableGame
                {
                    GameTableId = table2.GameTableId,
                    GameId = roulette.GameId
                },

                new GameTableGame
                {
                    GameTableId = table3.GameTableId,
                    GameId = poker.GameId
                },
                new GameTableGame
                {
                    GameTableId = table3.GameTableId,
                    GameId = roulette.GameId
                },

                new GameTableGame
                {
                    GameTableId = table4.GameTableId,
                    GameId = poker.GameId
                },
                new GameTableGame
                {
                    GameTableId = table4.GameTableId,
                    GameId = blackjack.GameId
                },
                new GameTableGame
                {
                    GameTableId = table4.GameTableId,
                    GameId = roulette.GameId
                },

                new GameTableGame
                {
                    GameTableId = table5.GameTableId,
                    GameId = blackjack.GameId
                },

                new GameTableGame
                {
                    GameTableId = table6.GameTableId,
                    GameId = poker.GameId
                },
                new GameTableGame
                {
                    GameTableId = table3.GameTableId,
                    GameId = wojna.GameId
                },
                new GameTableGame
                {
                    GameTableId = table5.GameTableId,
                    GameId = wojna.GameId
                }
            );

            context.SaveChanges();

            var session1 = new GameSession
            {
                GameVariantId = texasHoldem.GameVariantId,
                GameTableId = table1.GameTableId,
                StartAt = DateTime.Now.AddHours(2),
                EndAt = DateTime.Now.AddHours(5),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session2 = new GameSession
            {
                GameVariantId = blackjackClassic.GameVariantId,
                GameTableId = table2.GameTableId,
                StartAt = DateTime.Now.AddHours(3),
                EndAt = DateTime.Now.AddHours(6),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session3 = new GameSession
            {
                GameVariantId = europeanRoulette.GameVariantId,
                GameTableId = table4.GameTableId,
                StartAt = DateTime.Now.AddHours(1),
                EndAt = DateTime.Now.AddHours(4),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session4 = new GameSession
            {
                GameVariantId = wojnaClassic.GameVariantId,
                GameTableId = table3.GameTableId,
                StartAt = DateTime.Now.AddHours(4),
                EndAt = DateTime.Now.AddHours(7),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            context.Set<GameSession>().AddRange(session1, session2, session3, session4);
            context.SaveChanges();

            context.Set<EmployeeAssignment>().AddRange(
                new EmployeeAssignment
                {
                    EmployeeId = employee1.EmployeeId,
                    GameSessionId = session1.GameSessionId,
                    AssignedByUserId = admin.SystemUserId,
                    Notes = "Przypisanie testowe do sesji pokera."
                },
                new EmployeeAssignment
                {
                    EmployeeId = employee2.EmployeeId,
                    GameSessionId = session4.GameSessionId,
                    AssignedByUserId = admin.SystemUserId,
                    Notes = "Przypisanie testowe do sesji gry Wojna."
                }
            );

            context.SaveChanges();

            context.Set<WorkShift>().AddRange(
                new WorkShift
                {
                    EmployeeId = employee1.EmployeeId,
                    StartAt = DateTime.Now.AddHours(1),
                    EndAt = DateTime.Now.AddHours(9),
                    CreatedByUserId = admin.SystemUserId
                },
                new WorkShift
                {
                    EmployeeId = employee2.EmployeeId,
                    StartAt = DateTime.Now.AddHours(2),
                    EndAt = DateTime.Now.AddHours(10),
                    CreatedByUserId = admin.SystemUserId
                }
            );

            context.SaveChanges();
        }

        private static void AddSeatsForTable(List<Seat> seats, GameTable table)
        {
            for (int i = 1; i <= table.MaxPlayers; i++)
            {
                seats.Add(new Seat
                {
                    TableId = table.GameTableId,
                    SeatNumber = i,
                    IsActive = true
                });
            }
        }

        private static RoleDictionary GetOrCreateRole(AppDbContext context, string name, string description)
        {
            var item = context.Set<RoleDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new RoleDictionary
            {
                Name = name,
                Description = description,
                IsActive = true
            };

            context.Set<RoleDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static SystemUser GetOrCreateUser(
            AppDbContext context,
            string login,
            string email,
            string passwordHash)
        {
            var user = context.Set<SystemUser>().FirstOrDefault(u => u.Login == login);
            if (user != null) return user;

            user = new SystemUser
            {
                Login = login,
                Email = email,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Set<SystemUser>().Add(user);
            context.SaveChanges();

            return user;
        }

        private static void AddUserRoleIfMissing(
            AppDbContext context,
            SystemUser user,
            RoleDictionary role)
        {
            var exists = context.Set<UserRole>().Any(ur =>
                ur.SystemUserId == user.SystemUserId &&
                ur.RoleDictionaryId == role.RoleDictionaryId);

            if (exists) return;

            context.Set<UserRole>().Add(new UserRole
            {
                SystemUserId = user.SystemUserId,
                RoleDictionaryId = role.RoleDictionaryId
            });

            context.SaveChanges();
        }

        private static EmployeePositionDictionary GetOrCreateEmployeePosition(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<EmployeePositionDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new EmployeePositionDictionary
            {
                Name = name,
                Description = description,
                IsActive = true
            };

            context.Set<EmployeePositionDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static EmployeeStatusDictionary GetOrCreateEmployeeStatus(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<EmployeeStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new EmployeeStatusDictionary
            {
                Name = name,
                Description = description,
                IsActive = true
            };

            context.Set<EmployeeStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static HallTypeDictionary GetOrCreateHallType(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<HallTypeDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new HallTypeDictionary
            {
                Name = name,
                Description = description
            };

            context.Set<HallTypeDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static GameCategoryDictionary GetOrCreateGameCategory(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<GameCategoryDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new GameCategoryDictionary
            {
                Name = name,
                Description = description
            };

            context.Set<GameCategoryDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static SessionStatusDictionary GetOrCreateSessionStatus(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<SessionStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new SessionStatusDictionary
            {
                Name = name,
                Description = description
            };

            context.Set<SessionStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static ReservationStatusDictionary GetOrCreateReservationStatus(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<ReservationStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new ReservationStatusDictionary
            {
                Name = name,
                Description = description
            };

            context.Set<ReservationStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static PaymentStatusDictionary GetOrCreatePaymentStatus(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<PaymentStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new PaymentStatusDictionary
            {
                Name = name,
                Description = description
            };

            context.Set<PaymentStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static PaymentMethodDictionary GetOrCreatePaymentMethod(
            AppDbContext context,
            string name,
            string description)
        {
            var item = context.Set<PaymentMethodDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new PaymentMethodDictionary
            {
                Name = name,
                Description = description
            };

            context.Set<PaymentMethodDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }
    }
}