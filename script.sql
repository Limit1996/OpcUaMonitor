IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Opc_Channels] (
    [Id] uniqueidentifier NOT NULL,
    [Url] nvarchar(200) NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Opc_Channels] PRIMARY KEY ([Id])
);

CREATE TABLE [Opc_Devices] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [ChannelId] uniqueidentifier NULL,
    CONSTRAINT [PK_Opc_Devices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Opc_Devices_Opc_Channels_ChannelId] FOREIGN KEY ([ChannelId]) REFERENCES [Opc_Channels] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Opc_Tags] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [DataType] int NOT NULL,
    [ScanRate] int NOT NULL,
    [Remark] nvarchar(max) NOT NULL,
    [DeviceId] uniqueidentifier NULL,
    CONSTRAINT [PK_Opc_Tags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Opc_Tags_Opc_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Opc_Devices] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Opc_Events] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [TagId] uniqueidentifier NOT NULL,
    [IsActive] bit NOT NULL,
    [EventType] int NOT NULL,
    [Remark] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Opc_Events] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Opc_Events_Opc_Tags_TagId] FOREIGN KEY ([TagId]) REFERENCES [Opc_Tags] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Opc_EventLogs] (
    [Id] uniqueidentifier NOT NULL,
    [EventId] uniqueidentifier NOT NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT '2025-10-11T15:49:26.9406279+08:00',
    [Value] nvarchar(500) NOT NULL,
    CONSTRAINT [PK_Opc_EventLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Opc_EventLogs_Opc_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Opc_Events] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Opc_Devices_ChannelId] ON [Opc_Devices] ([ChannelId]);

CREATE INDEX [IX_Opc_EventLogs_EventId] ON [Opc_EventLogs] ([EventId]);

CREATE INDEX [IX_Opc_Events_TagId] ON [Opc_Events] ([TagId]);

CREATE INDEX [IX_Opc_Events_TagId_IsActive] ON [Opc_Events] ([TagId], [IsActive]) WHERE [IsActive] = 1;

CREATE INDEX [IX_Opc_Tags_DeviceId] ON [Opc_Tags] ([DeviceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251011084927_Initial', N'9.0.9');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-10-16T10:40:49.5171577+08:00' FOR [Timestamp];

ALTER TABLE [Opc_Channels] ADD [DeviceId] uniqueidentifier NULL;

CREATE TABLE [Sys_Processes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Area] int NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    CONSTRAINT [PK_Sys_Processes] PRIMARY KEY ([Id])
);

CREATE TABLE [Sys_Devices] (
    [Id] uniqueidentifier NOT NULL,
    [Code] nvarchar(100) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Manufacturer] nvarchar(200) NOT NULL,
    [IpAddress] nvarchar(50) NOT NULL,
    [Specification] nvarchar(1000) NOT NULL,
    [ProcessId] uniqueidentifier NULL,
    CONSTRAINT [PK_Sys_Devices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Sys_Devices_Sys_Processes_ProcessId] FOREIGN KEY ([ProcessId]) REFERENCES [Sys_Processes] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Opc_Channels_DeviceId] ON [Opc_Channels] ([DeviceId]);

CREATE UNIQUE INDEX [IX_Sys_Devices_Code] ON [Sys_Devices] ([Code]);

CREATE INDEX [IX_Sys_Devices_ProcessId] ON [Sys_Devices] ([ProcessId]);

CREATE UNIQUE INDEX [IX_Sys_Processes_Name] ON [Sys_Processes] ([Name]);

ALTER TABLE [Opc_Channels] ADD CONSTRAINT [FK_Opc_Channels_Sys_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Sys_Devices] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251016034050_AddSysProcessAndDevice', N'9.0.9');

ALTER TABLE [Opc_Channels] DROP CONSTRAINT [FK_Opc_Channels_Sys_Devices_DeviceId];

EXEC sp_rename N'[Opc_Channels].[DeviceId]', N'SysDeviceId', 'COLUMN';

EXEC sp_rename N'[Opc_Channels].[IX_Opc_Channels_DeviceId]', N'IX_Opc_Channels_SysDeviceId', 'INDEX';

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-10-16T11:09:20.8564977+08:00' FOR [Timestamp];

ALTER TABLE [Opc_Channels] ADD CONSTRAINT [FK_Opc_Channels_Sys_Devices_SysDeviceId] FOREIGN KEY ([SysDeviceId]) REFERENCES [Sys_Devices] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251016040921_UpdateChannelSysDeviceId', N'9.0.9');

DROP INDEX [IX_Sys_Devices_ProcessId] ON [Sys_Devices];
DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sys_Devices]') AND [c].[name] = N'ProcessId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Sys_Devices] DROP CONSTRAINT [' + @var2 + '];');
UPDATE [Sys_Devices] SET [ProcessId] = '00000000-0000-0000-0000-000000000000' WHERE [ProcessId] IS NULL;
ALTER TABLE [Sys_Devices] ALTER COLUMN [ProcessId] uniqueidentifier NOT NULL;
ALTER TABLE [Sys_Devices] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [ProcessId];
CREATE INDEX [IX_Sys_Devices_ProcessId] ON [Sys_Devices] ([ProcessId]);

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-10-17T10:06:10.4908891+08:00' FOR [Timestamp];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251017030610_AddKey', N'9.0.9');

ALTER TABLE [Opc_Events] ADD [ChannelId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-10-22T16:47:41.5438424+08:00' FOR [Timestamp];

CREATE INDEX [IX_Opc_Events_ChannelId] ON [Opc_Events] ([ChannelId]);

ALTER TABLE [Opc_Events] ADD CONSTRAINT [FK_Opc_Events_Opc_Channels_ChannelId] FOREIGN KEY ([ChannelId]) REFERENCES [Opc_Channels] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251022094741_AddChannelId', N'9.0.9');

COMMIT;
GO

