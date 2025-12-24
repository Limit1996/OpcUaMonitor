BEGIN TRANSACTION;
DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-12-24T15:51:33.4983072+08:00' FOR [Timestamp];

ALTER TABLE [Opc_EventLogs] ADD [Parameters] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251224075134_AddParameters', N'9.0.9');

COMMIT;
GO

