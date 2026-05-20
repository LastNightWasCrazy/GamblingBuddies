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
            var adminRole = context.Set<RoleDictionary>().FirstOrDefault(r => r.Name == "Administrator");
            if (adminRole == null)
            {
                adminRole = new RoleDictionary
                {
                    Name = "Administrator",
                    Description = "Administrator systemu",
                    IsActive = true
                };
                context.Set<RoleDictionary>().Add(adminRole);
            }

            var managerRole = context.Set<RoleDictionary>().FirstOrDefault(r => r.Name == "Manager");
            if (managerRole == null)
            {
                managerRole = new RoleDictionary
                {
                    Name = "Manager",
                    Description = "Manager sali",
                    IsActive = true
                };
                context.Set<RoleDictionary>().Add(managerRole);
            }

            var employeeRole = context.Set<RoleDictionary>().FirstOrDefault(r => r.Name == "Employee");
            if (employeeRole == null)
            {
                employeeRole = new RoleDictionary
                {
                    Name = "Employee",
                    Description = "Pracownik",
                    IsActive = true
                };
                context.Set<RoleDictionary>().Add(employeeRole);
            }

            var dealerPosition = context.Set<EmployeePositionDictionary>()
    .FirstOrDefault(p => p.Name == "Dealer");

            if (dealerPosition == null)
            {
                dealerPosition = new EmployeePositionDictionary
                {
                    Name = "Dealer",
                    Description = "Krupier",
                    IsActive = true
                };

                context.Set<EmployeePositionDictionary>().Add(dealerPosition);
            }

            var managerPosition = context.Set<EmployeePositionDictionary>()
                .FirstOrDefault(p => p.Name == "Manager");

            if (managerPosition == null)
            {
                managerPosition = new EmployeePositionDictionary
                {
                    Name = "Manager",
                    Description = "Kierownik",
                    IsActive = true
                };

                context.Set<EmployeePositionDictionary>().Add(managerPosition);
            }

            var activeEmployeeStatus = context.Set<EmployeeStatusDictionary>()
                .FirstOrDefault(s => s.Name == "Active");

            if (activeEmployeeStatus == null)
            {
                activeEmployeeStatus = new EmployeeStatusDictionary
                {
                    Name = "Active",
                    Description = "Aktywny",
                    IsActive = true
                };

                context.Set<EmployeeStatusDictionary>().Add(activeEmployeeStatus);
            }

            var inactiveEmployeeStatus = context.Set<EmployeeStatusDictionary>()
                .FirstOrDefault(s => s.Name == "Inactive");

            if (inactiveEmployeeStatus == null)
            {
                inactiveEmployeeStatus = new EmployeeStatusDictionary
                {
                    Name = "Inactive",
                    Description = "Nieaktywny",
                    IsActive = true
                };

                context.Set<EmployeeStatusDictionary>().Add(inactiveEmployeeStatus);
            }

            var standardHallType = new HallTypeDictionary { Name = "Standard", Description = "Sala standardowa" };
            var vipHallType = new HallTypeDictionary { Name = "VIP", Description = "Sala VIP" };

            var cardsCategory = new GameCategoryDictionary { Name = "Cards", Description = "Gry karciane" };
            var rouletteCategory = new GameCategoryDictionary { Name = "Roulette", Description = "Ruletka" };

            var plannedSessionStatus = new SessionStatusDictionary { Name = "Planned", Description = "Zaplanowana" };
            var finishedSessionStatus = new SessionStatusDictionary { Name = "Finished", Description = "Zakończona" };

            var pendingReservationStatus = new ReservationStatusDictionary { Name = "Pending", Description = "Oczekująca" };
            var confirmedReservationStatus = new ReservationStatusDictionary { Name = "Confirmed", Description = "Potwierdzona" };

            var pendingPaymentStatus = new PaymentStatusDictionary { Name = "Pending", Description = "Oczekująca" };
            var paidPaymentStatus = new PaymentStatusDictionary { Name = "Paid", Description = "Opłacona" };

            var cashMethod = new PaymentMethodDictionary { Name = "Cash", Description = "Gotówka" };
            var cardMethod = new PaymentMethodDictionary { Name = "Card", Description = "Karta" };

            context.SaveChanges();

            var admin = context.Set<SystemUser>().FirstOrDefault(u => u.Login == "admin");
            if (admin == null)
            {
                admin = new SystemUser
                {
                    Login = "admin",
                    Email = "admin@gamblingbuddies.local",
                    PasswordHash = "admin123",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Set<SystemUser>().Add(admin);
                context.SaveChanges();
            }



            var manager = context.Set<SystemUser>().FirstOrDefault(u => u.Login == "manager");
            if (manager == null)
            {
                manager = new SystemUser
                {
                    Login = "manager",
                    Email = "manager@gamblingbuddies.local",
                    PasswordHash = "manager123",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                context.Set<SystemUser>().Add(manager);
                context.SaveChanges();
            }

            var employeeUser = context.Set<SystemUser>().FirstOrDefault(u => u.Login == "employee");

            if (employeeUser == null)
            {
                employeeUser = new SystemUser
                {
                    Login = "employee",
                    Email = "employee@gamblingbuddies.local",
                    PasswordHash = "employee123",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                context.Set<SystemUser>().Add(employeeUser);
                context.SaveChanges();
            }

            if (!context.Set<UserRole>().Any(ur =>
                    ur.SystemUserId == admin.SystemUserId &&
                    ur.RoleDictionaryId == adminRole.RoleDictionaryId))
            {
                context.Set<UserRole>().Add(new UserRole
                {
                    SystemUserId = admin.SystemUserId,
                    RoleDictionaryId = adminRole.RoleDictionaryId
                });
            }

            if (!context.Set<UserRole>().Any(ur =>
                    ur.SystemUserId == manager.SystemUserId &&
                    ur.RoleDictionaryId == managerRole.RoleDictionaryId))
            {
                context.Set<UserRole>().Add(new UserRole
                {
                    SystemUserId = manager.SystemUserId,
                    RoleDictionaryId = managerRole.RoleDictionaryId
                });
            }

            if (!context.Set<UserRole>().Any(ur =>
            ur.SystemUserId == employeeUser.SystemUserId &&
            ur.RoleDictionaryId == employeeRole.RoleDictionaryId))
            {
                context.Set<UserRole>().Add(new UserRole
                {
                    SystemUserId = employeeUser.SystemUserId,
                    RoleDictionaryId = employeeRole.RoleDictionaryId
                });
            }

            context.SaveChanges();
        }

        private static void SeedBusinessData(AppDbContext context)
        {
            var dealerPosition = GetOrCreateEmployeePosition(context, "Dealer", "Krupier");
            var managerPosition = GetOrCreateEmployeePosition(context, "Manager", "Kierownik");

            var activeEmployeeStatus = GetOrCreateEmployeeStatus(context, "Active", "Aktywny");
            var inactiveEmployeeStatus = GetOrCreateEmployeeStatus(context, "Inactive", "Nieaktywny");

            var standardHallType = GetOrCreateHallType(context, "Standard", "Sala standardowa");
            var vipHallType = GetOrCreateHallType(context, "VIP", "Sala VIP");
            var tournamentHallType = GetOrCreateHallType(context, "Tournament", "Sala turniejowa");

            var cardsCategory = GetOrCreateGameCategory(context, "Cards", "Gry karciane");
            var rouletteCategory = GetOrCreateGameCategory(context, "Roulette", "Ruletka");

            var plannedSessionStatus = GetOrCreateSessionStatus(context, "Planned", "Zaplanowana");
            var finishedSessionStatus = GetOrCreateSessionStatus(context, "Finished", "Zakończona");

            var pendingReservationStatus = GetOrCreateReservationStatus(context, "Pending", "Oczekująca");
            var confirmedReservationStatus = GetOrCreateReservationStatus(context, "Confirmed", "Potwierdzona");

            var pendingPaymentStatus = GetOrCreatePaymentStatus(context, "Pending", "Oczekująca");
            var paidPaymentStatus = GetOrCreatePaymentStatus(context, "Paid", "Opłacona");

            var cashMethod = GetOrCreatePaymentMethod(context, "Cash", "Gotówka");
            var cardMethod = GetOrCreatePaymentMethod(context, "Card", "Karta");

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
                EmployeeStatusDictionaryId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            var employee2 = new Employee
            {
                SystemUserId = employeeUser.SystemUserId,
                FirstName = "Anna",
                LastName = "Nowak",
                Phone = "500300400",
                HireDate = DateTime.Now.AddMonths(-3),
                PositionId = dealerPosition.EmployeePositionDictionaryId,
                EmployeeStatusDictionaryId = activeEmployeeStatus.EmployeeStatusDictionaryId
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
                HallId = hall2.HallId,
                TableNumber = 201,
                MinPlayers = 1,
                MaxPlayers = 5,
                IsActive = true
            };

            var table4 = new GameTable
            {
                HallId = hall3.HallId,
                TableNumber = 301,
                MinPlayers = 2,
                MaxPlayers = 10,
                IsActive = true
            };

            context.Set<GameTable>().AddRange(table1, table2, table3, table4);
            context.SaveChanges();
            // MIEJSCA

            context.Set<ReservationSeat>().RemoveRange(context.Set<ReservationSeat>());
            context.Set<Seat>().RemoveRange(context.Set<Seat>());
            context.SaveChanges();

            var seats = new List<Seat>();

            for (int i = 1; i <= table1.MaxPlayers; i++)
            {
                seats.Add(new Seat
                {
                    TableId = table1.GameTableId,
                    SeatNumber = i,
                    IsActive = true
                });
            }

            for (int i = 1; i <= table2.MaxPlayers; i++)
            {
                seats.Add(new Seat
                {
                    TableId = table2.GameTableId,
                    SeatNumber = i,
                    IsActive = true
                });
            }

            for (int i = 1; i <= table3.MaxPlayers; i++)
            {
                seats.Add(new Seat
                {
                    TableId = table3.GameTableId,
                    SeatNumber = i,
                    IsActive = true
                });
            }

            for (int i = 1; i <= table4.MaxPlayers; i++)
            {
                seats.Add(new Seat
                {
                    TableId = table4.GameTableId,
                    SeatNumber = i,
                    IsActive = true
                });
            }
            context.Set<Seat>().AddRange(seats);
            context.SaveChanges();

            // GRY

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

            context.Set<Game>().AddRange(poker, blackjack, roulette);
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

            context.Set<GameVariant>().AddRange(texasHoldem, blackjackClassic, europeanRoulette);
            context.SaveChanges();

            var session1 = new GameSession
            {
                GameVariantId = texasHoldem.GameVariantId,
                GameTableId = table1.GameTableId,
                StartAt = DateTime.Now.AddHours(2),
                EndAt = DateTime.Now.AddHours(5),
                SessionStatusId = plannedSessionStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session2 = new GameSession
            {
                GameVariantId = blackjackClassic.GameVariantId,
                GameTableId = table2.GameTableId,
                StartAt = DateTime.Now.AddHours(3),
                EndAt = DateTime.Now.AddHours(6),
                SessionStatusId = plannedSessionStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            var session3 = new GameSession
            {
                GameVariantId = europeanRoulette.GameVariantId,
                GameTableId = table3.GameTableId,
                StartAt = DateTime.Now.AddHours(1),
                EndAt = DateTime.Now.AddHours(4),
                SessionStatusId = plannedSessionStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            context.Set<GameSession>().AddRange(session1, session2, session3);
            context.SaveChanges();

            var reservation1 = new Reservation
            {
                PlayerId = player1.PlayerId,
                GameSessionId = session1.GameSessionId,
                ReservationStatusId = confirmedReservationStatus.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            var reservation2 = new Reservation
            {
                PlayerId = player2.PlayerId,
                GameSessionId = session3.GameSessionId,
                ReservationStatusId = pendingReservationStatus.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            context.Set<Reservation>().AddRange(reservation1, reservation2);
            context.SaveChanges();

            context.Set<ReservationSeat>().AddRange(
                new ReservationSeat
                {
                    ReservationId = reservation1.ReservationId,
                    SeatId = context.Set<Seat>().First(s => s.TableId == table1.GameTableId).SeatId,
                    GameSessionId = session1.GameSessionId
                },
                new ReservationSeat
                {
                    ReservationId = reservation2.ReservationId,
                    SeatId = context.Set<Seat>().First(s => s.TableId == table3.GameTableId).SeatId,
                    GameSessionId = session3.GameSessionId
                }
            );

            var payment1 = new Payment
            {
                ReservationId = reservation1.ReservationId,
                PaymentMethodId = cardMethod.PaymentMethodId,
                PaymentStatusId = paidPaymentStatus.PaymentStatusId,
                Amount = 100,
                CreatedAt = DateTime.Now,
                PaidAt = DateTime.Now
            };

            var payment2 = new Payment
            {
                ReservationId = reservation2.ReservationId,
                PaymentMethodId = cashMethod.PaymentMethodId,
                PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                Amount = 150,
                CreatedAt = DateTime.Now,
                PaidAt = null
            };

            context.Set<Payment>().AddRange(payment1, payment2);
            context.SaveChanges();

            context.Set<PaymentTransaction>().Add(
                new PaymentTransaction
                {
                    PaymentId = payment1.PaymentId,
                    ExternalTransactionId = "TXN-001",
                    ProviderResponseCode = "200",
                    ProviderResponseMessage = "Success",
                    CreatedAt = DateTime.Now
                }
            );

            context.Set<EmployeeAssignment>().AddRange(
                new EmployeeAssignment
                {
                    EmployeeId = employee1.EmployeeId,
                    GameSessionId = session1.GameSessionId,
                    AssignedByUserId = admin.SystemUserId
                },
                new EmployeeAssignment
                {
                    EmployeeId = employee2.EmployeeId,
                    GameSessionId = session3.GameSessionId,
                    AssignedByUserId = admin.SystemUserId
                }
            );

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

            /*var reportDefinition = new ReportDefinition
            {
                Name = "Daily Reservations",
                Description = "Raport dziennych rezerwacji",
                QueryTemplate_or_Definition = "{}",
                CreatedByUserId = admin.SystemUserId
            };

            context.Set<ReportDefinition>().Add(reportDefinition);
            context.SaveChanges();

            context.Set<ReportExecution>().Add(
                new ReportExecution
                {
                    ReportDefinitionId = reportDefinition.ReportDefinitionId,
                    GeneratedByUserId = admin.SystemUserId,
                    GeneratedAt = DateTime.Now,
                    ParametersJson = "{}"
                }
            );

            var document = new Document
            {
                Title = "Raport rezerwacji",
                DocumentType = "PDF",
                CreatedAt = DateTime.Now,
                CreatedByUserId = admin.SystemUserId,
                ReservationId = reservation1.ReservationId
            };

            context.Set<Document>().Add(document);
            context.SaveChanges();
           

            context.Set<DocumentFile>().Add(
                new DocumentFile
                {
                    DocumentId = document.DocumentId,
                    FileName = "report.pdf",
                    FilePath = "/files/report.pdf",
                    ContentType = "application/pdf",
                    FileSize = 1024,
                    UploadedAt = DateTime.Now
                }
            );

            context.Set<AuditLog>().Add(
                new AuditLog
                {
                    SystemUserId = admin.SystemUserId,
                    Action = "Create",
                    EntityName = "Reservation",
                    EntityId = reservation1.ReservationId,
                    Details = "Utworzono przykładową rezerwację",
                    CreatedAt = DateTime.Now,
                    IpAddress = "127.0.0.1"
                }
            );

            context.SaveChanges();
             */
        }

        private static EmployeePositionDictionary GetOrCreateEmployeePosition(AppDbContext context, string name, string description)
        {
            var item = context.Set<EmployeePositionDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new EmployeePositionDictionary { Name = name, Description = description, IsActive = true };
            context.Set<EmployeePositionDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static EmployeeStatusDictionary GetOrCreateEmployeeStatus(AppDbContext context, string name, string description)
        {
            var item = context.Set<EmployeeStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new EmployeeStatusDictionary { Name = name, Description = description, IsActive = true };
            context.Set<EmployeeStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static HallTypeDictionary GetOrCreateHallType(AppDbContext context, string name, string description)
        {
            var item = context.Set<HallTypeDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new HallTypeDictionary { Name = name, Description = description};
            context.Set<HallTypeDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static GameCategoryDictionary GetOrCreateGameCategory(AppDbContext context, string name, string description)
        {
            var item = context.Set<GameCategoryDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new GameCategoryDictionary { Name = name, Description = description };
            context.Set<GameCategoryDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static SessionStatusDictionary GetOrCreateSessionStatus(AppDbContext context, string name, string description)
        {
            var item = context.Set<SessionStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new SessionStatusDictionary { Name = name, Description = description };
            context.Set<SessionStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static ReservationStatusDictionary GetOrCreateReservationStatus(AppDbContext context, string name, string description)
        {
            var item = context.Set<ReservationStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new ReservationStatusDictionary { Name = name, Description = description };
            context.Set<ReservationStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static PaymentStatusDictionary GetOrCreatePaymentStatus(AppDbContext context, string name, string description)
        {
            var item = context.Set<PaymentStatusDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new PaymentStatusDictionary { Name = name, Description = description };
            context.Set<PaymentStatusDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }

        private static PaymentMethodDictionary GetOrCreatePaymentMethod(AppDbContext context, string name, string description)
        {
            var item = context.Set<PaymentMethodDictionary>().FirstOrDefault(x => x.Name == name);
            if (item != null) return item;

            item = new PaymentMethodDictionary { Name = name, Description = description };
            context.Set<PaymentMethodDictionary>().Add(item);
            context.SaveChanges();

            return item;
        }
    }
}