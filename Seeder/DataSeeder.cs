namespace GamblingBuddies
{
    public static class DataSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.Set<SystemUser>().Any())
                return;

            // SŁOWNIKI

            var adminRole = new RoleDictionary { Name = "Administrator", Description = "Administrator systemu" };
            var managerRole = new RoleDictionary { Name = "Manager", Description = "Manager sali" };
            var employeeRole = new RoleDictionary { Name = "Employee", Description = "Pracownik" };

            var dealerPosition = new EmployeePositionDictionary { Name = "Dealer", Description = "Krupier" };
            var managerPosition = new EmployeePositionDictionary { Name = "Manager", Description = "Kierownik" };

            var activeEmployeeStatus = new EmployeeStatusDictionary { Name = "Active", Description = "Aktywny" };
            var inactiveEmployeeStatus = new EmployeeStatusDictionary { Name = "Inactive", Description = "Nieaktywny" };

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

            context.AddRange(
                adminRole, managerRole, employeeRole,
                dealerPosition, managerPosition,
                activeEmployeeStatus, inactiveEmployeeStatus,
                standardHallType, vipHallType,
                cardsCategory, rouletteCategory,
                plannedSessionStatus, finishedSessionStatus,
                pendingReservationStatus, confirmedReservationStatus,
                pendingPaymentStatus, paidPaymentStatus,
                cashMethod, cardMethod
            );

            context.SaveChanges();

            // UŻYTKOWNICY

            var admin = new SystemUser
            {
                Login = "admin",
                Email = "admin@gamblingbuddies.local",
                PasswordHash = "admin123",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var manager = new SystemUser
            {
                Login = "manager",
                Email = "manager@gamblingbuddies.local",
                PasswordHash = "manager123",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Set<SystemUser>().AddRange(admin, manager);
            context.SaveChanges();

            context.Set<UserRole>().AddRange(
                new UserRole { SystemUserId = admin.SystemUserId, RoleId = adminRole.RoleDictionaryId },
                new UserRole { SystemUserId = manager.SystemUserId, RoleId = managerRole.RoleDictionaryId }
            );

            // PRACOWNICY

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
                SystemUserId = null,
                FirstName = "Anna",
                LastName = "Nowak",
                Phone = "500300400",
                HireDate = DateTime.Now.AddMonths(-3),
                PositionId = dealerPosition.EmployeePositionDictionaryId,
                EmployeeStatusDictionaryId = activeEmployeeStatus.EmployeeStatusDictionaryId
            };

            context.Set<Employee>().AddRange(employee1, employee2);

            // GRACZE

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

            // SALE

            var hall1 = new Hall
            {
                Name = "Main Hall",
                HallTypeId = standardHallType.HallTypeId,
                Description = "Główna sala",
                IsActive = true
            };

            var hall2 = new Hall
            {
                Name = "VIP Hall",
                HallTypeId = vipHallType.HallTypeId,
                Description = "Sala VIP",
                IsActive = true
            };

            context.Set<Hall>().AddRange(hall1, hall2);
            context.SaveChanges();

            // STOŁY

            var table1 = new GameTable
            {
                HallId = hall1.HallId,
                TableNumber = 1,
                MinPlayers = 1,
                MaxPlayers = 6,
                IsActive = true
            };

            var table2 = new GameTable
            {
                HallId = hall2.HallId,
                TableNumber = 2,
                MinPlayers = 1,
                MaxPlayers = 5,
                IsActive = true
            };

            context.Set<GameTable>().AddRange(table1, table2);
            context.SaveChanges();

            // MIEJSCA

            var seat1 = new Seat { TableId = table1.GameTableId, SeatNumber = 1 };
            var seat2 = new Seat { TableId = table1.GameTableId, SeatNumber = 2 };
            var seat3 = new Seat { TableId = table2.GameTableId, SeatNumber = 1 };

            context.Set<Seat>().AddRange(seat1, seat2, seat3);

            // GRY

            var poker = new Game
            {
                Name = "Poker",
                GameCategoryId = cardsCategory.GameCategoryId,
                Description = "Gra karciana Poker",
                IsActive = true
            };

            var roulette = new Game
            {
                Name = "Roulette",
                GameCategoryId = rouletteCategory.GameCategoryId,
                Description = "Klasyczna ruletka",
                IsActive = true
            };

            context.Set<Game>().AddRange(poker, roulette);
            context.SaveChanges();

            // WARIANTY GIER

            var texasHoldem = new GameVariant
            {
                GameId = poker.GameId,
                Name = "Texas Holdem",
                RulesDescription = "Standardowe zasady Texas Holdem",
                DefaultMinBet = 10,
                DefaultMaxBet = 500,
                IsActive = true
            };

            var europeanRoulette = new GameVariant
            {
                GameId = roulette.GameId,
                Name = "European Roulette",
                RulesDescription = "Ruletka europejska",
                DefaultMinBet = 5,
                DefaultMaxBet = 300,
                IsActive = true
            };

            context.Set<GameVariant>().AddRange(texasHoldem, europeanRoulette);
            context.SaveChanges();

            // SESJE

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
                GameVariantId = europeanRoulette.GameVariantId,
                GameTableId = table2.GameTableId,
                StartAt = DateTime.Now.AddHours(3),
                EndAt = DateTime.Now.AddHours(6),
                SessionStatusId = plannedSessionStatus.SessionStatusId,
                CreatedByUserId = admin.SystemUserId
            };

            context.Set<GameSession>().AddRange(session1, session2);
            context.SaveChanges();

            // REZERWACJE

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
                GameSessionId = session2.GameSessionId,
                ReservationStatusId = pendingReservationStatus.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            context.Set<Reservation>().AddRange(reservation1, reservation2);
            context.SaveChanges();

            context.Set<ReservationSeat>().AddRange(
                new ReservationSeat { ReservationId = reservation1.ReservationId, SeatId = seat1.SeatId },
                new ReservationSeat { ReservationId = reservation2.ReservationId, SeatId = seat3.SeatId }
            );

            // PŁATNOŚCI

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

            // PRACOWNICY W SESJACH I ZMIANY

            context.Set<EmployeeAssignment>().AddRange(
                new EmployeeAssignment
                {
                    EmployeeId = employee1.EmployeeId,
                    GameSessionId = session1.GameSessionId,
                    AssingedByUserId = admin.SystemUserId
                },
                new EmployeeAssignment
                {
                    EmployeeId = employee2.EmployeeId,
                    GameSessionId = session2.GameSessionId,
                    AssingedByUserId = admin.SystemUserId
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

            // RAPORTY

            var reportDefinition = new ReportDefinition
            {
                Name = "Daily Reservations",
                Description = "Raport dziennych rezerwacji",
                QueryTemplate_or_Definition = "{}",
                CreatedByUserId = admin.SystemUserId
            };

            context.Set<ReportDefinition>().Add(reportDefinition);
            context.SaveChanges();

            var reportExecution = new ReportExecution
            {
                ReportDefinitionId = reportDefinition.ReportDefinitionId,
                GeneratedByUserId = admin.SystemUserId,
                GeneratedAt = DateTime.Now,
                ParametersJson = "{}"
            };

            context.Set<ReportExecution>().Add(reportExecution);
            context.SaveChanges();

            // DOKUMENTY

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

            // AUDYT

            context.Set<AuditLog>().Add(
                new AuditLog
                {
                    SystemUserId = admin.SystemUserId,
                    Action = "Create",
                    EntityName = "Reservation",
                    EntityId = reservation1.ReservationId,
                    Details = "Utworzono rezerwację",
                    CreatedAt = DateTime.Now,
                    IpAddress = "127.0.0.1"
                }
            );

            context.SaveChanges();
        }
    }
}