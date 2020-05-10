-- Settings Table ========================
CREATE TABLE Settings (
    Id uniqueidentifier PRIMARY KEY NONCLUSTERED,
    Name nvarchar(200)  NOT NULL,
    PasswordResetTicketExpiryMinutes int  NOT NULL,
    CacheVersion int  NOT NULL
);

EXEC sp_addextendedproperty @name=N'ReferenceData', @value='Enum', @level0type=N'SCHEMA', @level0name='dbo', @level1type=N'TABLE', @level1name='Settings';
