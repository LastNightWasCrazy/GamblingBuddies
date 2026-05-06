SELECT * FROM SystemUsers;

SELECT * FROM RoleDictionaries;



SELECT * FROM UserRoles;

INSERT INTO Roles (Name)
VALUES
('Administrator'),
('Manager'),
('Employee');

INSERT INTO UserRoles (SystemUserId, RoleId, RoleDictionaryId)
VALUES
(1, 1, 1),
(2, 2, 2);

SELECT 
    su.Login,
    rd.Name AS RoleName
FROM SystemUsers su
JOIN UserRoles ur ON su.SystemUserId = ur.SystemUserId
JOIN RoleDictionaries rd ON ur.RoleDictionaryId = rd.RoleDictionaryId;