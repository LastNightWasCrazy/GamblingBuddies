USE GamblingBuddies;
GO

-- Widok użytkowników z rolami
SELECT *
FROM rpt.view_UsersWithRoles;
GO

-- Statystyka użytkowników według ról
SELECT *
FROM rpt.fn_UserRoleStats();
GO

-- Statystyka pracowników według stanowisk
SELECT *
FROM rpt.fn_EmployeePositionStats();
GO

-- Procedura: użytkownicy z rolami
EXEC rpt.proc_GetUsersWithRoles;
GO

-- Procedura: pracownicy według stanowisk
EXEC rpt.proc_GetEmployeePositionStats;
GO

-- Historia zmian statusów rejestracji
SELECT *
FROM dbo.RegistrationRequestStatusHistory;
GO

-- Archiwum płatności
SELECT *
FROM dbo.PaymentArchive;
GO

-- Audyt operacji DML na SystemUsers
SELECT *
FROM dbo.SBD_AuditLogs;
GO

-- Indeksy dla SystemUsers
SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM sys.indexes i
JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE t.name = 'SystemUsers'
  AND i.name IS NOT NULL;
GO

-- Indeksy dla Payments
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Payments_CreatedAt_Status'
      AND object_id = OBJECT_ID('dbo.Payments')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Payments_CreatedAt_Status
    ON dbo.Payments (CreatedAt, PaymentStatusId)
    INCLUDE (Amount, PaidAt, ReservationId, PaymentMethodId);
END;
GO

-- Sprawdzenie indeksów dla Payments
SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM sys.indexes i
JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE t.name = 'Payments'
  AND i.name IS NOT NULL;
GO

-- Indeks optymalizujący podgląd i filtrowanie rezerwacji
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Reservations_Status_ReservedAt'
      AND object_id = OBJECT_ID('dbo.Reservations')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reservations_Status_ReservedAt
    ON dbo.Reservations (ReservationStatusId, ReservedAt DESC)
    INCLUDE (PlayerId, GameSessionId);
END;
GO

-- Sprawdzenie indeksów dla Reservations
SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM sys.indexes i
JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE t.name = 'Reservations'
  AND i.name IS NOT NULL;
GO