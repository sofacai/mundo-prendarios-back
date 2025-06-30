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
GO

CREATE TABLE [Canales] (
    [Id] int NOT NULL IDENTITY,
    [NombreFantasia] nvarchar(max) NULL,
    [RazonSocial] nvarchar(max) NULL,
    [Provincia] nvarchar(max) NULL,
    [Localidad] nvarchar(max) NULL,
    [Cuit] nvarchar(max) NULL,
    [CBU] nvarchar(max) NULL,
    [Alias] nvarchar(max) NULL,
    [Banco] nvarchar(max) NULL,
    [NumCuenta] nvarchar(max) NULL,
    [TipoCanal] nvarchar(max) NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Canales] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Planes] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [FechaInicio] datetime2 NOT NULL,
    [FechaFin] datetime2 NOT NULL,
    [MontoMinimo] decimal(18,2) NOT NULL,
    [MontoMaximo] decimal(18,2) NOT NULL,
    [CuotasAplicables] nvarchar(max) NULL,
    [Tasa] decimal(18,2) NOT NULL,
    [MontoFijo] decimal(18,2) NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Planes] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ReglasCotizacion] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [MontoMinimo] decimal(18,2) NOT NULL,
    [MontoMaximo] decimal(18,2) NOT NULL,
    [CuotasAplicables] nvarchar(max) NULL,
    [Tasa] decimal(18,2) NOT NULL,
    [MontoFijo] decimal(18,2) NOT NULL,
    [Activo] bit NOT NULL,
    [FechaInicio] datetime2 NOT NULL,
    [FechaFin] datetime2 NOT NULL,
    CONSTRAINT [PK_ReglasCotizacion] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Roles] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Clientes] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [Apellido] nvarchar(max) NULL,
    [Cuil] nvarchar(max) NULL,
    [Dni] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Telefono] nvarchar(max) NULL,
    [Provincia] nvarchar(max) NULL,
    [CanalId] int NOT NULL,
    CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Clientes_Canales_CanalId] FOREIGN KEY ([CanalId]) REFERENCES [Canales] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PlanesCanales] (
    [Id] int NOT NULL IDENTITY,
    [PlanId] int NOT NULL,
    [CanalId] int NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_PlanesCanales] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlanesCanales_Canales_CanalId] FOREIGN KEY ([CanalId]) REFERENCES [Canales] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanesCanales_Planes_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Planes] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Usuarios] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [Apellido] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Telefono] nvarchar(max) NULL,
    [PasswordHash] nvarchar(max) NULL,
    [RolId] int NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Usuarios_Roles_RolId] FOREIGN KEY ([RolId]) REFERENCES [Roles] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Subcanales] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [Provincia] nvarchar(max) NULL,
    [Localidad] nvarchar(max) NULL,
    [CanalId] int NOT NULL,
    [AdminCanalId] int NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Subcanales] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Subcanales_Canales_CanalId] FOREIGN KEY ([CanalId]) REFERENCES [Canales] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Subcanales_Usuarios_AdminCanalId] FOREIGN KEY ([AdminCanalId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Gastos] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [Porcentaje] decimal(18,2) NOT NULL,
    [SubcanalId] int NOT NULL,
    CONSTRAINT [PK_Gastos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Gastos_Subcanales_SubcanalId] FOREIGN KEY ([SubcanalId]) REFERENCES [Subcanales] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Operaciones] (
    [Id] int NOT NULL IDENTITY,
    [Monto] decimal(18,2) NOT NULL,
    [Meses] int NOT NULL,
    [Tasa] decimal(18,2) NOT NULL,
    [ClienteId] int NOT NULL,
    [PlanId] int NOT NULL,
    [VendedorId] int NOT NULL,
    [SubcanalId] int NOT NULL,
    [CanalId] int NOT NULL,
    [FechaCreacion] datetime2 NOT NULL,
    CONSTRAINT [PK_Operaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Operaciones_Canales_CanalId] FOREIGN KEY ([CanalId]) REFERENCES [Canales] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Operaciones_Clientes_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Clientes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Operaciones_Planes_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Planes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Operaciones_Subcanales_SubcanalId] FOREIGN KEY ([SubcanalId]) REFERENCES [Subcanales] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Operaciones_Usuarios_VendedorId] FOREIGN KEY ([VendedorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SubcanalVendors] (
    [Id] int NOT NULL IDENTITY,
    [SubcanalId] int NOT NULL,
    [UsuarioId] int NOT NULL,
    CONSTRAINT [PK_SubcanalVendors] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SubcanalVendors_Subcanales_SubcanalId] FOREIGN KEY ([SubcanalId]) REFERENCES [Subcanales] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SubcanalVendors_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([Id], [Nombre])
VALUES (1, N'Admin'),
(2, N'AdminCanal'),
(3, N'Vendor');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO

CREATE INDEX [IX_Clientes_CanalId] ON [Clientes] ([CanalId]);
GO

CREATE INDEX [IX_Gastos_SubcanalId] ON [Gastos] ([SubcanalId]);
GO

CREATE INDEX [IX_Operaciones_CanalId] ON [Operaciones] ([CanalId]);
GO

CREATE INDEX [IX_Operaciones_ClienteId] ON [Operaciones] ([ClienteId]);
GO

CREATE INDEX [IX_Operaciones_PlanId] ON [Operaciones] ([PlanId]);
GO

CREATE INDEX [IX_Operaciones_SubcanalId] ON [Operaciones] ([SubcanalId]);
GO

CREATE INDEX [IX_Operaciones_VendedorId] ON [Operaciones] ([VendedorId]);
GO

CREATE INDEX [IX_PlanesCanales_CanalId] ON [PlanesCanales] ([CanalId]);
GO

CREATE INDEX [IX_PlanesCanales_PlanId] ON [PlanesCanales] ([PlanId]);
GO

CREATE INDEX [IX_Subcanales_AdminCanalId] ON [Subcanales] ([AdminCanalId]);
GO

CREATE INDEX [IX_Subcanales_CanalId] ON [Subcanales] ([CanalId]);
GO

CREATE INDEX [IX_SubcanalVendors_SubcanalId] ON [SubcanalVendors] ([SubcanalId]);
GO

CREATE INDEX [IX_SubcanalVendors_UsuarioId] ON [SubcanalVendors] ([UsuarioId]);
GO

CREATE INDEX [IX_Usuarios_RolId] ON [Usuarios] ([RolId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250303202621_InitialCreate', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [CanalOficialesComerciales] (
    [Id] int NOT NULL IDENTITY,
    [CanalId] int NOT NULL,
    [OficialComercialId] int NOT NULL,
    [FechaAsignacion] datetime2 NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_CanalOficialesComerciales] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CanalOficialesComerciales_Canales_CanalId] FOREIGN KEY ([CanalId]) REFERENCES [Canales] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CanalOficialesComerciales_Usuarios_OficialComercialId] FOREIGN KEY ([OficialComercialId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] ON;
INSERT INTO [Roles] ([Id], [Nombre])
VALUES (4, N'OficialComercial');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Roles]'))
    SET IDENTITY_INSERT [Roles] OFF;
GO

CREATE UNIQUE INDEX [IX_CanalOficialesComerciales_CanalId_OficialComercialId] ON [CanalOficialesComerciales] ([CanalId], [OficialComercialId]);
GO

CREATE INDEX [IX_CanalOficialesComerciales_OficialComercialId] ON [CanalOficialesComerciales] ([OficialComercialId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250324215842_AddOficialComercialRole', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250324223659_AddCanalOficialComercialTable', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250324230339_AddCanalOficialComercialTable1', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Operaciones] ADD [Estado] nvarchar(max) NULL;
GO

ALTER TABLE [Operaciones] ADD [FechaAprobacion] datetime2 NULL;
GO

ALTER TABLE [Operaciones] ADD [FechaLiquidacion] datetime2 NULL;
GO

ALTER TABLE [Operaciones] ADD [Liquidada] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Operaciones] ADD [MesesAprobados] int NULL;
GO

ALTER TABLE [Operaciones] ADD [MontoAprobado] decimal(18,2) NULL;
GO

ALTER TABLE [Operaciones] ADD [PlanAprobadoId] int NULL;
GO

ALTER TABLE [Operaciones] ADD [TasaAprobada] decimal(18,2) NULL;
GO

ALTER TABLE [Operaciones] ADD [UsuarioCreadorId] int NULL;
GO

CREATE INDEX [IX_Operaciones_PlanAprobadoId] ON [Operaciones] ([PlanAprobadoId]);
GO

CREATE INDEX [IX_Operaciones_UsuarioCreadorId] ON [Operaciones] ([UsuarioCreadorId]);
GO

ALTER TABLE [Operaciones] ADD CONSTRAINT [FK_Operaciones_Planes_PlanAprobadoId] FOREIGN KEY ([PlanAprobadoId]) REFERENCES [Planes] ([Id]);
GO

ALTER TABLE [Operaciones] ADD CONSTRAINT [FK_Operaciones_Usuarios_UsuarioCreadorId] FOREIGN KEY ([UsuarioCreadorId]) REFERENCES [Usuarios] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250408194944_ActualizacionOperaciones', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250408195306_ConfiguracionRelacionesOperacion', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Operaciones] DROP CONSTRAINT [FK_Operaciones_Planes_PlanAprobadoId];
GO

ALTER TABLE [Usuarios] ADD [CreadorId] int NULL;
GO

CREATE INDEX [IX_Usuarios_CreadorId] ON [Usuarios] ([CreadorId]);
GO

ALTER TABLE [Operaciones] ADD CONSTRAINT [FK_Operaciones_Planes_PlanAprobadoId] FOREIGN KEY ([PlanAprobadoId]) REFERENCES [Planes] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Usuarios] ADD CONSTRAINT [FK_Usuarios_Usuarios_CreadorId] FOREIGN KEY ([CreadorId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250410005036_AgregarCampoCreadorEnUsuario', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Clientes] ADD [Auto] nvarchar(max) NULL;
GO

ALTER TABLE [Clientes] ADD [CodigoPostal] int NULL;
GO

ALTER TABLE [Clientes] ADD [Ingresos] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250410213123_AddCamposExtraCliente', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Clientes] ADD [FechaNacimiento] datetime2 NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250410213431_Addfecha', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Operaciones] ADD [PlanAprobadoNombre] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250420044722_AddPlanAprobadoNombreToOperacion', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250426213314_vendedornull', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Operaciones]') AND [c].[name] = N'VendedorId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Operaciones] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Operaciones] ALTER COLUMN [VendedorId] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250426215207_MakeVendedorIdNullable', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PlanesTasas] (
    [Id] int NOT NULL IDENTITY,
    [PlanId] int NOT NULL,
    [Plazo] int NOT NULL,
    [Tasa] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PlanesTasas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlanesTasas_Planes_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Planes] ([Id]) ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX [IX_PlanesTasas_PlanId_Plazo] ON [PlanesTasas] ([PlanId], [Plazo]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250427001434_AddPlanTasasTable', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[PlanesTasas].[Tasa]', N'TasaC', N'COLUMN';
GO

ALTER TABLE [PlanesTasas] ADD [TasaA] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [PlanesTasas] ADD [TasaB] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250427184919_Año', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PlanesTasas] ADD [Activo] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250428152021_plazos', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Operaciones] ADD [AutoAprobado] nvarchar(max) NULL;
GO

ALTER TABLE [Operaciones] ADD [AutoInicial] nvarchar(max) NULL;
GO

ALTER TABLE [Operaciones] ADD [CuotaInicial] decimal(18,2) NULL;
GO

ALTER TABLE [Operaciones] ADD [CuotaInicialAprobada] decimal(18,2) NULL;
GO

ALTER TABLE [Operaciones] ADD [CuotaPromedio] decimal(18,2) NULL;
GO

ALTER TABLE [Operaciones] ADD [CuotaPromedioAprobada] decimal(18,2) NULL;
GO

ALTER TABLE [Operaciones] ADD [Observaciones] nvarchar(max) NULL;
GO

ALTER TABLE [Operaciones] ADD [UrlAprobadoDefinitivo] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250501202629_AddNuevosCamposOperacion', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Clientes] ADD [DniConyuge] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250502153937_dniConyuge', N'7.0.14');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Operaciones] ADD [BancoAprobado] nvarchar(max) NULL;
GO

ALTER TABLE [Operaciones] ADD [BancoInicial] nvarchar(max) NULL;
GO

ALTER TABLE [Operaciones] ADD [GastoAprobado] decimal(18,2) NULL;
GO

ALTER TABLE [Operaciones] ADD [GastoInicial] decimal(18,2) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250610203435_AgregarCamposGastoBancoOperacion', N'7.0.14');
GO

COMMIT;
GO

