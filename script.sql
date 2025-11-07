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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    CREATE TABLE [Opc_Channels] (
        [Id] uniqueidentifier NOT NULL,
        [Url] nvarchar(200) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Opc_Channels] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    CREATE TABLE [Opc_Devices] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [ChannelId] uniqueidentifier NULL,
        CONSTRAINT [PK_Opc_Devices] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Opc_Devices_Opc_Channels_ChannelId] FOREIGN KEY ([ChannelId]) REFERENCES [Opc_Channels] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    CREATE TABLE [Opc_EventLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EventId] uniqueidentifier NOT NULL,
        [Timestamp] datetime2 NOT NULL DEFAULT '2025-10-11T15:49:26.9406279+07:00',
        [Value] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_Opc_EventLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Opc_EventLogs_Opc_Events_EventId] FOREIGN KEY ([EventId]) REFERENCES [Opc_Events] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    CREATE INDEX [IX_Opc_Devices_ChannelId] ON [Opc_Devices] ([ChannelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    CREATE INDEX [IX_Opc_EventLogs_EventId] ON [Opc_EventLogs] ([EventId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    CREATE INDEX [IX_Opc_Events_TagId] ON [Opc_Events] ([TagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_Opc_Events_TagId_IsActive] ON [Opc_Events] ([TagId], [IsActive]) WHERE [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    CREATE INDEX [IX_Opc_Tags_DeviceId] ON [Opc_Tags] ([DeviceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251011084927_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251011084927_Initial', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-10-16T10:40:49.5171577+07:00' FOR [Timestamp];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    ALTER TABLE [Opc_Channels] ADD [DeviceId] uniqueidentifier NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    CREATE TABLE [Sys_Processes] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Area] int NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_Sys_Processes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    CREATE INDEX [IX_Opc_Channels_DeviceId] ON [Opc_Channels] ([DeviceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sys_Devices_Code] ON [Sys_Devices] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    CREATE INDEX [IX_Sys_Devices_ProcessId] ON [Sys_Devices] ([ProcessId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sys_Processes_Name] ON [Sys_Processes] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    ALTER TABLE [Opc_Channels] ADD CONSTRAINT [FK_Opc_Channels_Sys_Devices_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Sys_Devices] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016034050_AddSysProcessAndDevice'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251016034050_AddSysProcessAndDevice', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016040921_UpdateChannelSysDeviceId'
)
BEGIN
    ALTER TABLE [Opc_Channels] DROP CONSTRAINT [FK_Opc_Channels_Sys_Devices_DeviceId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016040921_UpdateChannelSysDeviceId'
)
BEGIN
    EXEC sp_rename N'[Opc_Channels].[DeviceId]', N'SysDeviceId', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016040921_UpdateChannelSysDeviceId'
)
BEGIN
    EXEC sp_rename N'[Opc_Channels].[IX_Opc_Channels_DeviceId]', N'IX_Opc_Channels_SysDeviceId', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016040921_UpdateChannelSysDeviceId'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-10-16T11:09:20.8564977+07:00' FOR [Timestamp];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016040921_UpdateChannelSysDeviceId'
)
BEGIN
    ALTER TABLE [Opc_Channels] ADD CONSTRAINT [FK_Opc_Channels_Sys_Devices_SysDeviceId] FOREIGN KEY ([SysDeviceId]) REFERENCES [Sys_Devices] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251016040921_UpdateChannelSysDeviceId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251016040921_UpdateChannelSysDeviceId', N'9.0.9');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017030610_AddKey'
)
BEGIN
    DROP INDEX [IX_Sys_Devices_ProcessId] ON [Sys_Devices];
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sys_Devices]') AND [c].[name] = N'ProcessId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Sys_Devices] DROP CONSTRAINT [' + @var2 + '];');
    EXEC(N'UPDATE [Sys_Devices] SET [ProcessId] = ''00000000-0000-0000-0000-000000000000'' WHERE [ProcessId] IS NULL');
    ALTER TABLE [Sys_Devices] ALTER COLUMN [ProcessId] uniqueidentifier NOT NULL;
    ALTER TABLE [Sys_Devices] ADD DEFAULT '00000000-0000-0000-0000-000000000000' FOR [ProcessId];
    CREATE INDEX [IX_Sys_Devices_ProcessId] ON [Sys_Devices] ([ProcessId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017030610_AddKey'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Opc_EventLogs]') AND [c].[name] = N'Timestamp');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Opc_EventLogs] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Opc_EventLogs] ADD DEFAULT '2025-10-17T10:06:10.4908891+07:00' FOR [Timestamp];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20251017030610_AddKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20251017030610_AddKey', N'9.0.9');
END;

COMMIT;
GO

