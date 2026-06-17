using GamblingBuddies.Models;
using Bogus;


namespace GamblingBuddies
{
    public static class DataSeeder
    {
        private static void SeedLargeRandomData(AppDbContext context)
        {
            var faker = new Bogus.Faker("pl");

            if (context.Players.Count() < 200)
            {
                var playersToCreate = 200 - context.Players.Count();
                var players = new List<Player>();

                for (int i = 0; i < playersToCreate; i++)
                {
                    players.Add(new Player
                    {
                        FirstName = faker.Name.FirstName(),
                        LastName = faker.Name.LastName(),
                        Email = faker.Internet.Email(),
                        Phone = faker.Phone.PhoneNumber("#########"),
                        CreatedAt = faker.Date.Past(1)
                    });
                }

                context.Players.AddRange(players);
                context.SaveChanges();
            }

            if (context.Reservations.Count() < 500)
            {
                var players = context.Players.ToList();
                var sessions = context.GameSessions.ToList();
                var statuses = context.ReservationStatusDictionaries.ToList();

                if (players.Any() && sessions.Any() && statuses.Any())
                {
                    var reservationsToCreate = 500 - context.Reservations.Count();
                    var reservations = new List<Reservation>();

                    for (int i = 0; i < reservationsToCreate; i++)
                    {
                        var player = players[faker.Random.Int(0, players.Count - 1)];
                        var session = sessions[faker.Random.Int(0, sessions.Count - 1)];
                        var status = statuses[faker.Random.Int(0, statuses.Count - 1)];

                        reservations.Add(new Reservation
                        {
                            PlayerId = player.PlayerId,
                            GameSessionId = session.GameSessionId,
                            ReservationStatusId = status.ReservationStatusId,
                            ReservedAt = faker.Date.Recent(60)
                        });
                    }

                    context.Reservations.AddRange(reservations);
                    context.SaveChanges();
                }
            }

            if (context.Payments.Count() < 300)
            {
                var reservations = context.Reservations.ToList();
                var paymentMethods = context.PaymentMethods.ToList();
                var paymentStatuses = context.PaymentStatuses.ToList();

                if (reservations.Any() && paymentMethods.Any() && paymentStatuses.Any())
                {
                    var paymentsToCreate = 300 - context.Payments.Count();
                    var payments = new List<Payment>();

                    for (int i = 0; i < paymentsToCreate; i++)
                    {
                        var reservation = reservations[faker.Random.Int(0, reservations.Count - 1)];

                        payments.Add(new Payment
                        {
                            ReservationId = reservation.ReservationId,
                            PaymentMethodId = paymentMethods[faker.Random.Int(0, paymentMethods.Count - 1)].PaymentMethodId,
                            PaymentStatusId = paymentStatuses[faker.Random.Int(0, paymentStatuses.Count - 1)].PaymentStatusId,
                            Amount = faker.Random.Decimal(50, 1000),
                            CreatedAt = faker.Date.Recent(60),
                            PaidAt = faker.Random.Bool(0.45f) ? faker.Date.Recent(30) : null,
                            ExternalOrderId = $"TEST-{Guid.NewGuid()}",
                            PaymentProviderOrderId = faker.Random.Bool(0.6f) ? $"PROV-{Guid.NewGuid()}" : null,
                            PaymentProvider = "Seed"
                        });
                    }

                    context.Payments.AddRange(payments);
                    context.SaveChanges();
                }
            }

            if (context.Employees.Count() < 80)
            {
                var employeePositions = context.EmployeePositionDictionaries.ToList();
                var employeeStatuses = context.EmployeeStatusDictionaries.ToList();

                if (employeePositions.Any() && employeeStatuses.Any())
                {
                    var employeesToCreate = 80 - context.Employees.Count();
                    var employees = new List<Employee>();

                    for (int i = 0; i < employeesToCreate; i++)
                    {
                        employees.Add(new Employee
                        {
                            SystemUserId = null,
                            FirstName = faker.Name.FirstName(),
                            LastName = faker.Name.LastName(),
                            Phone = faker.Phone.PhoneNumber("#########"),
                            HireDate = faker.Date.Past(3),
                            PositionId = employeePositions[faker.Random.Int(0, employeePositions.Count - 1)].EmployeePositionDictionaryId,
                            EmployeeStatusId = employeeStatuses[faker.Random.Int(0, employeeStatuses.Count - 1)].EmployeeStatusDictionaryId
                        });
                    }

                    context.Employees.AddRange(employees);
                    context.SaveChanges();
                }
            }

            if (context.WorkShifts.Count() < 300)
            {
                var employees = context.Employees.ToList();
                var users = context.SystemUsers.ToList();

                if (employees.Any() && users.Any())
                {
                    var shiftsToCreate = 300 - context.WorkShifts.Count();
                    var shifts = new List<WorkShift>();

                    for (int i = 0; i < shiftsToCreate; i++)
                    {
                        var start = faker.Date.Between(DateTime.Now.AddDays(-30), DateTime.Now.AddDays(30));

                        shifts.Add(new WorkShift
                        {
                            EmployeeId = employees[faker.Random.Int(0, employees.Count - 1)].EmployeeId,
                            StartAt = start,
                            EndAt = start.AddHours(faker.Random.Int(6, 10)),
                            CreatedByUserId = users[faker.Random.Int(0, users.Count - 1)].SystemUserId
                        });
                    }

                    context.WorkShifts.AddRange(shifts);
                    context.SaveChanges();
                }
            }

            if (context.EmployeeAssignments.Count() < 200)
            {
                var employees = context.Employees.ToList();
                var sessions = context.GameSessions.ToList();
                var users = context.SystemUsers.ToList();

                if (employees.Any() && sessions.Any() && users.Any())
                {
                    var assignmentsToCreate = 200 - context.EmployeeAssignments.Count();
                    var assignments = new List<EmployeeAssignment>();

                    for (int i = 0; i < assignmentsToCreate; i++)
                    {
                        assignments.Add(new EmployeeAssignment
                        {
                            EmployeeId = employees[faker.Random.Int(0, employees.Count - 1)].EmployeeId,
                            GameSessionId = sessions[faker.Random.Int(0, sessions.Count - 1)].GameSessionId,
                            AssignedByUserId = users[faker.Random.Int(0, users.Count - 1)].SystemUserId,
                            Notes = faker.Lorem.Sentence(6)
                        });
                    }

                    context.EmployeeAssignments.AddRange(assignments);
                    context.SaveChanges();
                }
            }
        }
        public static void Seed(AppDbContext context)
        {
            SeedRolesAndUsers(context);

            if (!context.Set<Hall>().Any() &&
                !context.Set<Game>().Any() &&
                !context.Set<GameTable>().Any())
            {
                SeedBusinessData(context);
            }

            SeedLargeRandomData(context);
        }

        private static void SeedRolesAndUsers(AppDbContext context)
        {
            var adminRole = GetOrCreateRole(context, "Administrator", "Administrator systemu");
            var managerRole = GetOrCreateRole(context, "Manager", "Manager sali");
            var employeeRole = GetOrCreateRole(context, "Employee", "Pracownik");

            GetOrCreateEmployeePosition(context, "Dealer", "Krupier");
            GetOrCreateEmployeePosition(context, "Manager", "Kierownik");
            GetOrCreateEmployeePosition(context, "Cashier", "Kasjer");
            GetOrCreateEmployeePosition(context, "Security", "Ochrona");

            GetOrCreateEmployeeStatus(context, "Active", "Aktywny");
            GetOrCreateEmployeeStatus(context, "Inactive", "Nieaktywny");
            GetOrCreateEmployeeStatus(context, "Vacation", "Urlop");

            GetOrCreatePaymentStatus(context, "Pending", "Oczekująca");
            GetOrCreatePaymentStatus(context, "Paid", "Opłacona");
            GetOrCreatePaymentStatus(context, "Rejected", "Odrzucona");
            GetOrCreatePaymentStatus(context, "Refunded", "Zwrócona");

            GetOrCreatePaymentMethod(context, "Cash", "Gotówka");
            GetOrCreatePaymentMethod(context, "Card", "Karta");
            GetOrCreatePaymentMethod(context, "PayU", "Płatność online PayU");

            GetOrCreateHallType(context, "Standard", "Sala standardowa");
            GetOrCreateHallType(context, "VIP", "Sala VIP");
            GetOrCreateHallType(context, "Tournament", "Sala turniejowa");

            GetOrCreateGameCategory(context, "Cards", "Gry karciane");
            GetOrCreateGameCategory(context, "Roulette", "Ruletka");
            GetOrCreateGameCategory(context, "Dice", "Gry kościane");

            GetOrCreateSessionStatus(context, "Planned", "Zaplanowana");
            GetOrCreateSessionStatus(context, "Finished", "Zakończona");
            GetOrCreateSessionStatus(context, "Active", "Aktywna");
            GetOrCreateSessionStatus(context, "Cancelled", "Anulowana");

            GetOrCreateReservationStatus(context, "Pending", "Oczekująca");
            GetOrCreateReservationStatus(context, "Confirmed", "Potwierdzona");
            GetOrCreateReservationStatus(context, "Cancelled", "Anulowana");

            var admin = GetOrCreateUser(context, "admin", "admin@gamblingbuddies.local", "admin123");
            var manager = GetOrCreateUser(context, "manager", "manager@gamblingbuddies.local", "manager123");
            var employeeUser = GetOrCreateUser(context, "employee", "employee@gamblingbuddies.local", "employee123");
            var dealer2User = GetOrCreateUser(context, "dealer2", "dealer2@gamblingbuddies.local", "dealer123");
            var cashierUser = GetOrCreateUser(context, "cashier", "cashier@gamblingbuddies.local", "cashier123");

            AddUserRoleIfMissing(context, admin, adminRole);
            AddUserRoleIfMissing(context, manager, managerRole);
            AddUserRoleIfMissing(context, employeeUser, employeeRole);
            AddUserRoleIfMissing(context, dealer2User, employeeRole);
            AddUserRoleIfMissing(context, cashierUser, employeeRole);

            context.SaveChanges();
        }

        private static void SeedBusinessData(AppDbContext context)
        {
            var now = DateTime.Now;

            var dealerPosition = GetOrCreateEmployeePosition(context, "Dealer", "Krupier");
            var managerPosition = GetOrCreateEmployeePosition(context, "Manager", "Kierownik");
            var cashierPosition = GetOrCreateEmployeePosition(context, "Cashier", "Kasjer");
            var securityPosition = GetOrCreateEmployeePosition(context, "Security", "Ochrona");

            var activeEmployeeStatus = GetOrCreateEmployeeStatus(context, "Active", "Aktywny");
            var inactiveEmployeeStatus = GetOrCreateEmployeeStatus(context, "Inactive", "Nieaktywny");

            var standardHallType = GetOrCreateHallType(context, "Standard", "Sala standardowa");
            var vipHallType = GetOrCreateHallType(context, "VIP", "Sala VIP");
            var tournamentHallType = GetOrCreateHallType(context, "Tournament", "Sala turniejowa");

            var cardsCategory = GetOrCreateGameCategory(context, "Cards", "Gry karciane");
            var rouletteCategory = GetOrCreateGameCategory(context, "Roulette", "Ruletka");
            var diceCategory = GetOrCreateGameCategory(context, "Dice", "Gry kościane");

            var plannedStatus = GetOrCreateSessionStatus(context, "Planned", "Zaplanowana");
            var activeSessionStatus = GetOrCreateSessionStatus(context, "Active", "Aktywna");
            var finishedSessionStatus = GetOrCreateSessionStatus(context, "Finished", "Zakończona");

            var pendingReservationStatus = GetOrCreateReservationStatus(context, "Pending", "Oczekująca");
            var confirmedReservationStatus = GetOrCreateReservationStatus(context, "Confirmed", "Potwierdzona");
            var cancelledReservationStatus = GetOrCreateReservationStatus(context, "Cancelled", "Anulowana");

            var pendingPaymentStatus = GetOrCreatePaymentStatus(context, "Pending", "Oczekująca");
            var paidPaymentStatus = GetOrCreatePaymentStatus(context, "Paid", "Opłacona");
            var rejectedPaymentStatus = GetOrCreatePaymentStatus(context, "Rejected", "Odrzucona");

            var cashMethod = GetOrCreatePaymentMethod(context, "Cash", "Gotówka");
            var cardMethod = GetOrCreatePaymentMethod(context, "Card", "Karta");
            var payuMethod = GetOrCreatePaymentMethod(context, "PayU", "Płatność online PayU");

            var admin = context.Set<SystemUser>().First(u => u.Login == "admin");
            var manager = context.Set<SystemUser>().First(u => u.Login == "manager");
            var employeeUser = context.Set<SystemUser>().First(u => u.Login == "employee");
            var dealer2User = context.Set<SystemUser>().First(u => u.Login == "dealer2");
            var cashierUser = context.Set<SystemUser>().First(u => u.Login == "cashier");

            var employee1 = new Employee
            {
                SystemUserId = manager.SystemUserId,
                FirstName = "Jan",
                LastName = "Kowalski",
                Phone = "500100200",
                HireDate = now.AddMonths(-10),
                PositionId = managerPosition.EmployeePositionDictionaryId,
                EmployeeStatusId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            var employee2 = new Employee
            {
                SystemUserId = employeeUser.SystemUserId,
                FirstName = "Anna",
                LastName = "Nowak",
                Phone = "500300400",
                HireDate = now.AddMonths(-7),
                PositionId = dealerPosition.EmployeePositionDictionaryId,
                EmployeeStatusId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            var employee3 = new Employee
            {
                SystemUserId = dealer2User.SystemUserId,
                FirstName = "Marek",
                LastName = "Wiśniewski",
                Phone = "500500600",
                HireDate = now.AddMonths(-4),
                PositionId = dealerPosition.EmployeePositionDictionaryId,
                EmployeeStatusId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            var employee4 = new Employee
            {
                SystemUserId = cashierUser.SystemUserId,
                FirstName = "Katarzyna",
                LastName = "Zalewska",
                Phone = "500700800",
                HireDate = now.AddMonths(-2),
                PositionId = cashierPosition.EmployeePositionDictionaryId,
                EmployeeStatusId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            var employee5 = new Employee
            {
                SystemUserId = null,
                FirstName = "Tomasz",
                LastName = "Wójcik",
                Phone = "500900100",
                HireDate = now.AddMonths(-1),
                PositionId = securityPosition.EmployeePositionDictionaryId,
                EmployeeStatusId = inactiveEmployeeStatus.EmployeeStatusDictionaryId
            };

            context.Set<Employee>().AddRange(employee1, employee2, employee3, employee4, employee5);

            var player1 = new Player
            {
                FirstName = "Piotr",
                LastName = "Zieliński",
                Email = "piotr@example.com",
                Phone = "600100200",
                CreatedAt = now.AddDays(-20)
            };

            var player2 = new Player
            {
                FirstName = "Maria",
                LastName = "Wiśniewska",
                Email = "maria@example.com",
                Phone = "600300400",
                CreatedAt = now.AddDays(-15)
            };

            var player3 = new Player
            {
                FirstName = "Adam",
                LastName = "Lewandowski",
                Email = "adam@example.com",
                Phone = "600500600",
                CreatedAt = now.AddDays(-10)
            };

            var player4 = new Player
            {
                FirstName = "Ewa",
                LastName = "Kamińska",
                Email = "ewa@example.com",
                Phone = "600700800",
                CreatedAt = now.AddDays(-5)
            };

            context.Set<Player>().AddRange(player1, player2, player3, player4);
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

            var hall4 = new Hall
            {
                Name = "Training Hall",
                HallTypeId = standardHallType.HallTypeId,
                Description = "Mniejsza sala do pokazowych i testowych sesji gry.",
                IsActive = true
            };

            context.Set<Hall>().AddRange(hall1, hall2, hall3, hall4);
            context.SaveChanges();

            var table1 = new GameTable { HallId = hall1.HallId, TableNumber = 101, MinPlayers = 1, MaxPlayers = 6, IsActive = true };
            var table2 = new GameTable { HallId = hall1.HallId, TableNumber = 102, MinPlayers = 2, MaxPlayers = 8, IsActive = true };
            var table3 = new GameTable { HallId = hall1.HallId, TableNumber = 103, MinPlayers = 1, MaxPlayers = 5, IsActive = true };
            var table4 = new GameTable { HallId = hall2.HallId, TableNumber = 201, MinPlayers = 1, MaxPlayers = 5, IsActive = true };
            var table5 = new GameTable { HallId = hall2.HallId, TableNumber = 202, MinPlayers = 2, MaxPlayers = 6, IsActive = true };
            var table6 = new GameTable { HallId = hall3.HallId, TableNumber = 301, MinPlayers = 2, MaxPlayers = 10, IsActive = true };
            var table7 = new GameTable { HallId = hall4.HallId, TableNumber = 401, MinPlayers = 1, MaxPlayers = 4, IsActive = true };

            context.Set<GameTable>().AddRange(table1, table2, table3, table4, table5, table6, table7);
            context.SaveChanges();

            var seats = new List<Seat>();
            AddSeatsForTable(seats, table1);
            AddSeatsForTable(seats, table2);
            AddSeatsForTable(seats, table3);
            AddSeatsForTable(seats, table4);
            AddSeatsForTable(seats, table5);
            AddSeatsForTable(seats, table6);
            AddSeatsForTable(seats, table7);

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

            var craps = new Game
            {
                Name = "Craps",
                GameCategoryId = diceCategory.GameCategoryId,
                Description = "Gra kościana rozgrywana przy stole kasynowym.",
                IsActive = true
            };

            context.Set<Game>().AddRange(poker, blackjack, roulette, wojna, craps);
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

            var omaha = new GameVariant
            {
                GameId = poker.GameId,
                Name = "Omaha",
                RulesDescription = "Wariant pokera z czterema kartami własnymi.",
                DefaultMinBet = 20,
                DefaultMaxBet = 800,
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

            var crapsClassic = new GameVariant
            {
                GameId = craps.GameId,
                Name = "Craps Classic",
                RulesDescription = "Podstawowy wariant gry Craps.",
                DefaultMinBet = 10,
                DefaultMaxBet = 300,
                IsActive = true
            };

            context.Set<GameVariant>().AddRange(texasHoldem, omaha, blackjackClassic, europeanRoulette, wojnaClassic, crapsClassic);
            context.SaveChanges();

            context.Set<GameTableGame>().AddRange(
                new GameTableGame { GameTableId = table1.GameTableId, GameId = poker.GameId },
                new GameTableGame { GameTableId = table1.GameTableId, GameId = blackjack.GameId },
                new GameTableGame { GameTableId = table2.GameTableId, GameId = blackjack.GameId },
                new GameTableGame { GameTableId = table2.GameTableId, GameId = roulette.GameId },
                new GameTableGame { GameTableId = table3.GameTableId, GameId = poker.GameId },
                new GameTableGame { GameTableId = table3.GameTableId, GameId = roulette.GameId },
                new GameTableGame { GameTableId = table3.GameTableId, GameId = wojna.GameId },
                new GameTableGame { GameTableId = table4.GameTableId, GameId = poker.GameId },
                new GameTableGame { GameTableId = table4.GameTableId, GameId = blackjack.GameId },
                new GameTableGame { GameTableId = table4.GameTableId, GameId = roulette.GameId },
                new GameTableGame { GameTableId = table5.GameTableId, GameId = blackjack.GameId },
                new GameTableGame { GameTableId = table5.GameTableId, GameId = wojna.GameId },
                new GameTableGame { GameTableId = table6.GameTableId, GameId = poker.GameId },
                new GameTableGame { GameTableId = table6.GameTableId, GameId = craps.GameId },
                new GameTableGame { GameTableId = table7.GameTableId, GameId = wojna.GameId },
                new GameTableGame { GameTableId = table7.GameTableId, GameId = blackjack.GameId }
            );
            context.SaveChanges();

            var session1 = new GameSession
            {
                GameVariantId = texasHoldem.GameVariantId,
                GameTableId = table1.GameTableId,
                StartAt = now.AddHours(2),
                EndAt = now.AddHours(5),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session2 = new GameSession
            {
                GameVariantId = blackjackClassic.GameVariantId,
                GameTableId = table2.GameTableId,
                StartAt = now.AddHours(3),
                EndAt = now.AddHours(6),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session3 = new GameSession
            {
                GameVariantId = europeanRoulette.GameVariantId,
                GameTableId = table4.GameTableId,
                StartAt = now.AddHours(1),
                EndAt = now.AddHours(4),
                SessionStatusId = activeSessionStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session4 = new GameSession
            {
                GameVariantId = wojnaClassic.GameVariantId,
                GameTableId = table3.GameTableId,
                StartAt = now.AddHours(4),
                EndAt = now.AddHours(7),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = manager.SystemUserId
            };

            var session5 = new GameSession
            {
                GameVariantId = omaha.GameVariantId,
                GameTableId = table6.GameTableId,
                StartAt = now.AddDays(1).Date.AddHours(18),
                EndAt = now.AddDays(1).Date.AddHours(22),
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = manager.SystemUserId
            };

            var session6 = new GameSession
            {
                GameVariantId = crapsClassic.GameVariantId,
                GameTableId = table6.GameTableId,
                StartAt = now.AddDays(-1).Date.AddHours(18),
                EndAt = now.AddDays(-1).Date.AddHours(21),
                SessionStatusId = finishedSessionStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            context.Set<GameSession>().AddRange(session1, session2, session3, session4, session5, session6);
            context.SaveChanges();

            context.Set<WorkShift>().AddRange(
                new WorkShift
                {
                    EmployeeId = employee1.EmployeeId,
                    StartAt = now.AddHours(1),
                    EndAt = now.AddHours(9),
                    CreatedByUserId = admin.SystemUserId
                },
                new WorkShift
                {
                    EmployeeId = employee2.EmployeeId,
                    StartAt = now.AddHours(1),
                    EndAt = now.AddHours(10),
                    CreatedByUserId = admin.SystemUserId
                },
                new WorkShift
                {
                    EmployeeId = employee3.EmployeeId,
                    StartAt = now.AddHours(2),
                    EndAt = now.AddHours(10),
                    CreatedByUserId = manager.SystemUserId
                },
                new WorkShift
                {
                    EmployeeId = employee4.EmployeeId,
                    StartAt = now.AddHours(0),
                    EndAt = now.AddHours(8),
                    CreatedByUserId = manager.SystemUserId
                },
                new WorkShift
                {
                    EmployeeId = employee2.EmployeeId,
                    StartAt = now.AddDays(1).Date.AddHours(14),
                    EndAt = now.AddDays(1).Date.AddHours(23),
                    CreatedByUserId = admin.SystemUserId
                },
                new WorkShift
                {
                    EmployeeId = employee3.EmployeeId,
                    StartAt = now.AddDays(1).Date.AddHours(14),
                    EndAt = now.AddDays(1).Date.AddHours(23),
                    CreatedByUserId = admin.SystemUserId
                }
            );
            context.SaveChanges();

            context.Set<EmployeeAssignment>().AddRange(
                new EmployeeAssignment
                {
                    EmployeeId = employee1.EmployeeId,
                    GameSessionId = session1.GameSessionId,
                    AssignedByUserId = admin.SystemUserId,
                    Notes = "Manager nadzoruje sesję pokera."
                },
                new EmployeeAssignment
                {
                    EmployeeId = employee2.EmployeeId,
                    GameSessionId = session2.GameSessionId,
                    AssignedByUserId = admin.SystemUserId,
                    Notes = "Krupier przypisany do blackjacka."
                },
                new EmployeeAssignment
                {
                    EmployeeId = employee3.EmployeeId,
                    GameSessionId = session4.GameSessionId,
                    AssignedByUserId = manager.SystemUserId,
                    Notes = "Przypisanie testowe do gry Wojna."
                },
                new EmployeeAssignment
                {
                    EmployeeId = employee2.EmployeeId,
                    GameSessionId = session5.GameSessionId,
                    AssignedByUserId = admin.SystemUserId,
                    Notes = "Jutrzejsza sesja turniejowa."
                },
                new EmployeeAssignment
                {
                    EmployeeId = employee3.EmployeeId,
                    GameSessionId = session5.GameSessionId,
                    AssignedByUserId = manager.SystemUserId,
                    Notes = "Drugi pracownik przy turnieju."
                }
            );
            context.SaveChanges();

            var reservation1 = new Reservation
            {
                PlayerId = player1.PlayerId,
                GameSessionId = session1.GameSessionId,
                ReservationStatusId = confirmedReservationStatus.ReservationStatusId,
                ReservedAt = now.AddDays(-2)
            };

            var reservation2 = new Reservation
            {
                PlayerId = player2.PlayerId,
                GameSessionId = session2.GameSessionId,
                ReservationStatusId = pendingReservationStatus.ReservationStatusId,
                ReservedAt = now.AddDays(-1)
            };

            var reservation3 = new Reservation
            {
                PlayerId = player3.PlayerId,
                GameSessionId = session3.GameSessionId,
                ReservationStatusId = confirmedReservationStatus.ReservationStatusId,
                ReservedAt = now.AddHours(-8)
            };

            var reservation4 = new Reservation
            {
                PlayerId = player4.PlayerId,
                GameSessionId = session5.GameSessionId,
                ReservationStatusId = cancelledReservationStatus.ReservationStatusId,
                ReservedAt = now.AddHours(-3)
            };

            context.Set<Reservation>().AddRange(reservation1, reservation2, reservation3, reservation4);
            context.SaveChanges();

            var table1Id = table1.GameTableId;
            var table2Id = table2.GameTableId;
            var table4Id = table4.GameTableId;
            var table6Id = table6.GameTableId;

            var table1Seats = context.Set<Seat>().Where(s => s.TableId == table1Id).OrderBy(s => s.SeatNumber).Take(2).ToList();
            var table2Seats = context.Set<Seat>().Where(s => s.TableId == table2Id).OrderBy(s => s.SeatNumber).Take(3).ToList();
            var table4Seats = context.Set<Seat>().Where(s => s.TableId == table4Id).OrderBy(s => s.SeatNumber).Take(1).ToList();
            var table6Seats = context.Set<Seat>().Where(s => s.TableId == table6Id).OrderBy(s => s.SeatNumber).Take(4).ToList();

            var reservationSeats = new List<ReservationSeat>();

            reservationSeats.AddRange(table1Seats.Select(seat => new ReservationSeat
            {
                ReservationId = reservation1.ReservationId,
                SeatId = seat.SeatId,
                GameSessionId = session1.GameSessionId
            }));

            reservationSeats.AddRange(table2Seats.Select(seat => new ReservationSeat
            {
                ReservationId = reservation2.ReservationId,
                SeatId = seat.SeatId,
                GameSessionId = session2.GameSessionId
            }));

            reservationSeats.AddRange(table4Seats.Select(seat => new ReservationSeat
            {
                ReservationId = reservation3.ReservationId,
                SeatId = seat.SeatId,
                GameSessionId = session3.GameSessionId
            }));

            reservationSeats.AddRange(table6Seats.Select(seat => new ReservationSeat
            {
                ReservationId = reservation4.ReservationId,
                SeatId = seat.SeatId,
                GameSessionId = session5.GameSessionId
            }));

            context.Set<ReservationSeat>().AddRange(reservationSeats);
            context.SaveChanges();

            var payments = new List<Payment>
            {
                new Payment
                {
                    ReservationId = reservation1.ReservationId,
                    PaymentMethodId = cardMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 150m,
                    CreatedAt = now.AddDays(-20),
                    PaidAt = now.AddDays(-20).AddMinutes(5),
                    ExternalOrderId = "EXT-RES-001",
                    PaymentProviderOrderId = "CARD-ORDER-001",
                    PaymentProvider = "Card"
                },
                new Payment
                {
                    ReservationId = reservation2.ReservationId,
                    PaymentMethodId = cashMethod.PaymentMethodId,
                    PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                    Amount = 100m,
                    CreatedAt = now.AddDays(-19),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-002",
                    PaymentProviderOrderId = null,
                    PaymentProvider = "Cash"
                },
                new Payment
                {
                    ReservationId = reservation3.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 200m,
                    CreatedAt = now.AddDays(-18),
                    PaidAt = now.AddDays(-18).AddMinutes(7),
                    ExternalOrderId = "EXT-RES-003",
                    PaymentProviderOrderId = "PAYU-ORDER-003",
                    PaymentProvider = "PayU"
                },
                new Payment
                {
                    ReservationId = reservation4.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = rejectedPaymentStatus.PaymentStatusId,
                    Amount = 120m,
                    CreatedAt = now.AddDays(-17),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-004",
                    PaymentProviderOrderId = "PAYU-ORDER-004",
                    PaymentProvider = "PayU"
                },
                new Payment
                {
                    ReservationId = reservation1.ReservationId,
                    PaymentMethodId = cashMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 80m,
                    CreatedAt = now.AddDays(-16),
                    PaidAt = now.AddDays(-16).AddMinutes(3),
                    ExternalOrderId = "EXT-RES-005",
                    PaymentProviderOrderId = null,
                    PaymentProvider = "Cash"
                },
                new Payment
                {
                    ReservationId = reservation2.ReservationId,
                    PaymentMethodId = cardMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 250m,
                    CreatedAt = now.AddDays(-15),
                    PaidAt = now.AddDays(-15).AddMinutes(10),
                    ExternalOrderId = "EXT-RES-006",
                    PaymentProviderOrderId = "CARD-ORDER-006",
                    PaymentProvider = "Card"
                },
                new Payment
                {
                    ReservationId = reservation3.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                    Amount = 300m,
                    CreatedAt = now.AddDays(-14),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-007",
                    PaymentProviderOrderId = "PAYU-ORDER-007",
                    PaymentProvider = "PayU"
                },
                new Payment
                {
                    ReservationId = reservation4.ReservationId,
                    PaymentMethodId = cashMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 60m,
                    CreatedAt = now.AddDays(-13),
                    PaidAt = now.AddDays(-13).AddMinutes(2),
                    ExternalOrderId = "EXT-RES-008",
                    PaymentProviderOrderId = null,
                    PaymentProvider = "Cash"
                },
                new Payment
                {
                    ReservationId = reservation1.ReservationId,
                    PaymentMethodId = cardMethod.PaymentMethodId,
                    PaymentStatusId = rejectedPaymentStatus.PaymentStatusId,
                    Amount = 90m,
                    CreatedAt = now.AddDays(-12),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-009",
                    PaymentProviderOrderId = "CARD-ORDER-009",
                    PaymentProvider = "Card"
                },
                new Payment
                {
                    ReservationId = reservation2.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 180m,
                    CreatedAt = now.AddDays(-11),
                    PaidAt = now.AddDays(-11).AddMinutes(8),
                    ExternalOrderId = "EXT-RES-010",
                    PaymentProviderOrderId = "PAYU-ORDER-010",
                    PaymentProvider = "PayU"
                },
                new Payment
                {
                    ReservationId = reservation3.ReservationId,
                    PaymentMethodId = cashMethod.PaymentMethodId,
                    PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                    Amount = 110m,
                    CreatedAt = now.AddDays(-10),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-011",
                    PaymentProviderOrderId = null,
                    PaymentProvider = "Cash"
                },
                new Payment
                {
                    ReservationId = reservation4.ReservationId,
                    PaymentMethodId = cardMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 220m,
                    CreatedAt = now.AddDays(-9),
                    PaidAt = now.AddDays(-9).AddMinutes(6),
                    ExternalOrderId = "EXT-RES-012",
                    PaymentProviderOrderId = "CARD-ORDER-012",
                    PaymentProvider = "Card"
                },
                new Payment
                {
                    ReservationId = reservation1.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 140m,
                    CreatedAt = now.AddDays(-8),
                    PaidAt = now.AddDays(-8).AddMinutes(4),
                    ExternalOrderId = "EXT-RES-013",
                    PaymentProviderOrderId = "PAYU-ORDER-013",
                    PaymentProvider = "PayU"
                },
                new Payment
                {
                    ReservationId = reservation2.ReservationId,
                    PaymentMethodId = cashMethod.PaymentMethodId,
                    PaymentStatusId = rejectedPaymentStatus.PaymentStatusId,
                    Amount = 70m,
                    CreatedAt = now.AddDays(-7),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-014",
                    PaymentProviderOrderId = null,
                    PaymentProvider = "Cash"
                },
                new Payment
                {
                    ReservationId = reservation3.ReservationId,
                    PaymentMethodId = cardMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 160m,
                    CreatedAt = now.AddDays(-6),
                    PaidAt = now.AddDays(-6).AddMinutes(9),
                    ExternalOrderId = "EXT-RES-015",
                    PaymentProviderOrderId = "CARD-ORDER-015",
                    PaymentProvider = "Card"
                },
                new Payment
                {
                    ReservationId = reservation4.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 210m,
                    CreatedAt = now.AddDays(-5),
                    PaidAt = now.AddDays(-5).AddMinutes(11),
                    ExternalOrderId = "EXT-RES-016",
                    PaymentProviderOrderId = "PAYU-ORDER-016",
                    PaymentProvider = "PayU"
                },
                new Payment
                {
                    ReservationId = reservation1.ReservationId,
                    PaymentMethodId = cashMethod.PaymentMethodId,
                    PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                    Amount = 95m,
                    CreatedAt = now.AddDays(-4),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-017",
                    PaymentProviderOrderId = null,
                    PaymentProvider = "Cash"
                },
                new Payment
                {
                    ReservationId = reservation2.ReservationId,
                    PaymentMethodId = cardMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 130m,
                    CreatedAt = now.AddDays(-3),
                    PaidAt = now.AddDays(-3).AddMinutes(5),
                    ExternalOrderId = "EXT-RES-018",
                    PaymentProviderOrderId = "CARD-ORDER-018",
                    PaymentProvider = "Card"
                },
                new Payment
                {
                    ReservationId = reservation3.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = rejectedPaymentStatus.PaymentStatusId,
                    Amount = 175m,
                    CreatedAt = now.AddDays(-2),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-019",
                    PaymentProviderOrderId = "PAYU-ORDER-019",
                    PaymentProvider = "PayU"
                },
                new Payment
                {
                    ReservationId = reservation4.ReservationId,
                    PaymentMethodId = cardMethod.PaymentMethodId,
                    PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                    Amount = 125m,
                    CreatedAt = now.AddDays(-1),
                    PaidAt = null,
                    ExternalOrderId = "EXT-RES-020",
                    PaymentProviderOrderId = "CARD-ORDER-020",
                    PaymentProvider = "Card"
                },
                new Payment
                {
                    ReservationId = reservation1.ReservationId,
                    PaymentMethodId = payuMethod.PaymentMethodId,
                    PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                    Amount = 240m,
                    CreatedAt = now.AddHours(-8),
                    PaidAt = now.AddHours(-7).AddMinutes(45),
                    ExternalOrderId = "EXT-RES-021",
                    PaymentProviderOrderId = "PAYU-ORDER-021",
                    PaymentProvider = "PayU"
                }
            };

            context.Set<Payment>().AddRange(payments);
            context.SaveChanges();

            var payment1 = payments[0];
            var payment3 = payments[2];

            context.Set<PaymentTransaction>().AddRange(
                new PaymentTransaction
                {
                    PaymentId = payments[0].PaymentId,
                    ExternalTransactionId = "TRX-001",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność Card zakończona poprawnie.",
                    CreatedAt = now.AddDays(-20).AddMinutes(5)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[1].PaymentId,
                    ExternalTransactionId = "TRX-002",
                    ProviderResponseCode = "PENDING",
                    ProviderResponseMessage = "Płatność Cash oczekuje na potwierdzenie.",
                    CreatedAt = now.AddDays(-19).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[2].PaymentId,
                    ExternalTransactionId = "TRX-003",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność PayU zakończona poprawnie.",
                    CreatedAt = now.AddDays(-18).AddMinutes(7)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[3].PaymentId,
                    ExternalTransactionId = "TRX-004",
                    ProviderResponseCode = "REJECTED",
                    ProviderResponseMessage = "Płatność PayU została odrzucona.",
                    CreatedAt = now.AddDays(-17).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[4].PaymentId,
                    ExternalTransactionId = "TRX-005",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność Cash zakończona poprawnie.",
                    CreatedAt = now.AddDays(-16).AddMinutes(3)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[5].PaymentId,
                    ExternalTransactionId = "TRX-006",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność Card zakończona poprawnie.",
                    CreatedAt = now.AddDays(-15).AddMinutes(10)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[6].PaymentId,
                    ExternalTransactionId = "TRX-007",
                    ProviderResponseCode = "PENDING",
                    ProviderResponseMessage = "Płatność PayU oczekuje na potwierdzenie.",
                    CreatedAt = now.AddDays(-14).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[7].PaymentId,
                    ExternalTransactionId = "TRX-008",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność Cash zakończona poprawnie.",
                    CreatedAt = now.AddDays(-13).AddMinutes(2)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[8].PaymentId,
                    ExternalTransactionId = "TRX-009",
                    ProviderResponseCode = "REJECTED",
                    ProviderResponseMessage = "Płatność Card została odrzucona.",
                    CreatedAt = now.AddDays(-12).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[9].PaymentId,
                    ExternalTransactionId = "TRX-010",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność PayU zakończona poprawnie.",
                    CreatedAt = now.AddDays(-11).AddMinutes(8)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[10].PaymentId,
                    ExternalTransactionId = "TRX-011",
                    ProviderResponseCode = "PENDING",
                    ProviderResponseMessage = "Płatność Cash oczekuje na potwierdzenie.",
                    CreatedAt = now.AddDays(-10).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[11].PaymentId,
                    ExternalTransactionId = "TRX-012",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność Card zakończona poprawnie.",
                    CreatedAt = now.AddDays(-9).AddMinutes(6)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[12].PaymentId,
                    ExternalTransactionId = "TRX-013",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność PayU zakończona poprawnie.",
                    CreatedAt = now.AddDays(-8).AddMinutes(4)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[13].PaymentId,
                    ExternalTransactionId = "TRX-014",
                    ProviderResponseCode = "REJECTED",
                    ProviderResponseMessage = "Płatność Cash została odrzucona.",
                    CreatedAt = now.AddDays(-7).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[14].PaymentId,
                    ExternalTransactionId = "TRX-015",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność Card zakończona poprawnie.",
                    CreatedAt = now.AddDays(-6).AddMinutes(9)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[15].PaymentId,
                    ExternalTransactionId = "TRX-016",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność PayU zakończona poprawnie.",
                    CreatedAt = now.AddDays(-5).AddMinutes(11)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[16].PaymentId,
                    ExternalTransactionId = "TRX-017",
                    ProviderResponseCode = "PENDING",
                    ProviderResponseMessage = "Płatność Cash oczekuje na potwierdzenie.",
                    CreatedAt = now.AddDays(-4).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[17].PaymentId,
                    ExternalTransactionId = "TRX-018",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność Card zakończona poprawnie.",
                    CreatedAt = now.AddDays(-3).AddMinutes(5)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[18].PaymentId,
                    ExternalTransactionId = "TRX-019",
                    ProviderResponseCode = "REJECTED",
                    ProviderResponseMessage = "Płatność PayU została odrzucona.",
                    CreatedAt = now.AddDays(-2).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[19].PaymentId,
                    ExternalTransactionId = "TRX-020",
                    ProviderResponseCode = "PENDING",
                    ProviderResponseMessage = "Płatność Card oczekuje na potwierdzenie.",
                    CreatedAt = now.AddDays(-1).AddMinutes(1)
                },
                new PaymentTransaction
                {
                    PaymentId = payments[20].PaymentId,
                    ExternalTransactionId = "TRX-021",
                    ProviderResponseCode = "SUCCESS",
                    ProviderResponseMessage = "Płatność PayU zakończona poprawnie.",
                    CreatedAt = now.AddHours(-7).AddMinutes(45)
                }
            );
            context.SaveChanges();


            var document1 = new Document
            {
                Title = "Potwierdzenie rezerwacji Poker",
                DocumentType = "ReservationConfirmation",
                CreatedAt = now.AddDays(-2),
                CreatedByUserId = admin.SystemUserId,
                ReservationId = reservation1.ReservationId
            };

            var document2 = new Document
            {
                Title = "Regulamin sali VIP",
                DocumentType = "Rules",
                CreatedAt = now.AddDays(-5),
                CreatedByUserId = manager.SystemUserId,
                ReservationId = null
            };

            var document3 = new Document
            {
                Title = "Potwierdzenie płatności PayU",
                DocumentType = "PaymentConfirmation",
                CreatedAt = now.AddHours(-7),
                CreatedByUserId = admin.SystemUserId,
                ReservationId = reservation3.ReservationId
            };

            context.Set<Document>().AddRange(document1, document2, document3);
            context.SaveChanges();

            context.Set<DocumentFile>().AddRange(
                new DocumentFile
                {
                    DocumentId = document1.DocumentId,
                    FileName = "reservation-poker.pdf",
                    FilePath = "/uploads/documents/reservation-poker.pdf",
                    ContentType = "application/pdf",
                    FileSize = 24576,
                    UploadedAt = now.AddDays(-2)
                },
                new DocumentFile
                {
                    DocumentId = document2.DocumentId,
                    FileName = "vip-rules.pdf",
                    FilePath = "/uploads/documents/vip-rules.pdf",
                    ContentType = "application/pdf",
                    FileSize = 32768,
                    UploadedAt = now.AddDays(-5)
                },
                new DocumentFile
                {
                    DocumentId = document3.DocumentId,
                    FileName = "payu-confirmation.pdf",
                    FilePath = "/uploads/documents/payu-confirmation.pdf",
                    ContentType = "application/pdf",
                    FileSize = 18432,
                    UploadedAt = now.AddHours(-7)
                }
            );
            context.SaveChanges();

            var reportDefinition1 = new ReportDefinition
            {
                Name = "Raport płatności",
                Description = "Raport pokazujący płatności w wybranym okresie.",
                QueryTemplate_or_Definition = "Payments by date range and status",
                CreatedByUserId = admin.SystemUserId
            };

            var reportDefinition2 = new ReportDefinition
            {
                Name = "Raport rezerwacji",
                Description = "Raport pokazujący rezerwacje według sal i statusów.",
                QueryTemplate_or_Definition = "Reservations by hall and status",
                CreatedByUserId = manager.SystemUserId
            };

            context.Set<ReportDefinition>().AddRange(reportDefinition1, reportDefinition2);
            context.SaveChanges();

            context.Set<ReportExecution>().AddRange(
                new ReportExecution
                {
                    ReportDefinitionId = reportDefinition1.ReportDefinitionId,
                    GeneratedByUserId = admin.SystemUserId,
                    GeneratedAt = now.AddDays(-1),
                    ParametersJson = "{ \"status\": \"Paid\" }"
                },
                new ReportExecution
                {
                    ReportDefinitionId = reportDefinition2.ReportDefinitionId,
                    GeneratedByUserId = manager.SystemUserId,
                    GeneratedAt = now.AddHours(-6),
                    ParametersJson = "{ \"hall\": \"Main Hall\" }"
                }
            );
            context.SaveChanges();

            context.Set<PaymentReport>().AddRange(
                new PaymentReport
                {
                    ReportName = "Raport płatności testowy",
                    Description = "Przykładowy raport płatności wygenerowany przez seedera.",
                    GeneratedDate = now.AddHours(-5),
                    StartDate = now.AddDays(-7),
                    EndDate = now,
                    TotalAmount = 3205m,
                    TransactionsCount = 21,
                    FiltersApplied = "Status: Paid, Pending, Rejected",
                    GeneratedByUserId = admin.SystemUserId,
                    PdfFilePath = "/reports/payment-report-test.pdf"
                },
                new PaymentReport
                {
                    ReportName = "Raport płatności opłaconych",
                    Description = "Przykładowy raport tylko dla płatności opłaconych.",
                    GeneratedDate = now.AddHours(-2),
                    StartDate = now.AddDays(-3),
                    EndDate = now,
                    TotalAmount = 2020m,
                    TransactionsCount = 12,
                    FiltersApplied = "Status: Paid",
                    GeneratedByUserId = manager.SystemUserId,
                    PdfFilePath = "/reports/paid-payments-test.pdf"
                }
            );
            context.SaveChanges();

            context.Set<AuditLog>().AddRange(
                new AuditLog
                {
                    SystemUserId = admin.SystemUserId,
                    Action = "Seed",
                    EntityName = "Database",
                    EntityId = null,
                    Details = "Dodano dane startowe aplikacji.",
                    CreatedAt = now,
                    IpAddress = "127.0.0.1"
                },
                new AuditLog
                {
                    SystemUserId = manager.SystemUserId,
                    Action = "Create",
                    EntityName = "Reservation",
                    EntityId = reservation1.ReservationId,
                    Details = "Przykładowe utworzenie rezerwacji.",
                    CreatedAt = now.AddDays(-2),
                    IpAddress = "127.0.0.1"
                },
                new AuditLog
                {
                    SystemUserId = admin.SystemUserId,
                    Action = "PaymentConfirmed",
                    EntityName = "Payment",
                    EntityId = payment3.PaymentId,
                    Details = "Przykładowe potwierdzenie płatności PayU.",
                    CreatedAt = now.AddHours(-7),
                    IpAddress = "127.0.0.1"
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
            if (user == null)
                throw new InvalidOperationException("Nie znaleziono użytkownika podczas dodawania roli.");

            if (role == null)
                throw new InvalidOperationException("Nie znaleziono roli podczas dodawania roli użytkownikowi.");

            var userId = user.SystemUserId;
            var roleId = role.RoleDictionaryId;

            var exists = context.Set<UserRole>().Any(ur =>
                ur.SystemUserId == userId &&
                ur.RoleDictionaryId == roleId);

            if (exists) return;

            context.Set<UserRole>().Add(new UserRole
            {
                SystemUserId = userId,
                RoleDictionaryId = roleId
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
