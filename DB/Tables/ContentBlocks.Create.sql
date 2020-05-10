-- ContentBlocks Table ========================
CREATE TABLE ContentBlocks (
    Id uniqueidentifier PRIMARY KEY NONCLUSTERED,
    [Key] nvarchar(200)  NOT NULL,
    Content nvarchar(MAX)  NOT NULL
);

EXEC sp_addextendedproperty @name=N'ReferenceData', @value='Enum', @level0type=N'SCHEMA', @level0name='dbo', @level1type=N'TABLE', @level1name='ContentBlocks';
