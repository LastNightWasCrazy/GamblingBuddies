USE GamblingBuddies;
GO
--podstawowe tabele
SELECT TOP 20 *
FROM SystemUsers;
GO

SELECT TOP 20 *
FROM Employees;
GO

SELECT TOP 20 *
FROM Payments;
GO

SELECT TOP 20 *
FROM Reservations;
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

--role uzytkowników
SELECT
    su.SystemUserId,
    su.Login,
    su.Email,
    rd.Name AS RoleName
FROM SystemUsers su
JOIN UserRoles ur
    ON su.SystemUserId = ur.SystemUserId
JOIN RoleDictionaries rd
    ON ur.RoleDictionaryId = rd.RoleDictionaryId;
GO

--role bazodanowe
SELECT
    dp.name AS UserName,
    rp.name AS RoleName
FROM sys.database_role_members drm
JOIN sys.database_principals rp
    ON drm.role_principal_id = rp.principal_id
JOIN sys.database_principals dp
    ON drm.member_principal_id = dp.principal_id
WHERE dp.name = 'GamblingManager';
GO

SELECT
    pr.name AS PrincipalName,
    pr.type_desc AS PrincipalType,
    pe.permission_name,
    pe.state_desc,
    OBJECT_NAME(pe.major_id) AS ObjectName
FROM sys.database_permissions pe
JOIN sys.database_principals pr
    ON pe.grantee_principal_id = pr.principal_id
WHERE pr.name = 'db_gambling_manager';
GO

--schemat raportów rpt
SELECT *
FROM rpt.view_UsersWithRoles;
GO

SELECT *
FROM rpt.fn_UserRoleStats();
GO

SELECT *
FROM rpt.fn_EmployeePositionStats();
GO

EXEC rpt.proc_GetUsersWithRoles;
GO

EXEC rpt.proc_GetEmployeePositionStats;
GO

--historia zmian
SELECT *
FROM RegistrationRequestStatusHistory
ORDER BY ChangedAt DESC;
GO

SELECT *
FROM PaymentArchive
ORDER BY ArchivedAt DESC;
GO

--audyt
SELECT *
FROM SBD_AuditLogs
ORDER BY ExecutedAt DESC;
GO

SELECT *
FROM SBD_DatabaseAuditLogs
ORDER BY ExecutedAt DESC;
GO

--indeksy
SELECT
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
FROM sys.indexes i
JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE i.name IS NOT NULL
ORDER BY t.name, i.name;
GO

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

--optymalizacja payments
SET STATISTICS IO ON;
SET STATISTICS TIME ON;
GO

SELECT
    PaymentStatusId,
    COUNT(*) AS PaymentsCount,
    SUM(Amount) AS TotalAmount
FROM Payments
WHERE CreatedAt BETWEEN '2026-04-01' AND '2026-06-30'
GROUP BY PaymentStatusId;
GO

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
GO