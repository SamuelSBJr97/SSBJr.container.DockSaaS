using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SSBJr.DockSaaS.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentApiCallsAndCurrentStorageToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceInstances_ServiceDefinitions_ServiceDefinitionId",
                table: "ServiceInstances");

            migrationBuilder.DropIndex(
                name: "IX_ServiceMetrics_ServiceInstanceId_MetricName_Timestamp",
                table: "ServiceMetrics");

            migrationBuilder.DropIndex(
                name: "IX_ServiceInstances_Name_TenantId",
                table: "ServiceInstances");

            migrationBuilder.DropIndex(
                name: "IX_ServiceInstances_TenantId",
                table: "ServiceInstances");

            migrationBuilder.DropIndex(
                name: "IX_ServiceDefinitions_Name",
                table: "ServiceDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId_Timestamp",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "ServiceDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "ServiceDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "ServiceDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "ServiceDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "ServiceDefinitions",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.AddColumn<int>(
                name: "CurrentApiCalls",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "CurrentStorage",
                table: "Tenants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "ServiceMetrics",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceInstanceId1",
                table: "ServiceMetrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceInstances",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Created",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Created");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServiceInstances",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EndpointUrl",
                table: "ServiceInstances",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Configuration",
                table: "ServiceInstances",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "ServiceInstances",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceDefinitionId1",
                table: "ServiceInstances",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ServiceDefinitions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServiceDefinitions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ServiceDefinitions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultConfiguration",
                table: "ServiceDefinitions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationSchema",
                table: "ServiceDefinitions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "OldValues",
                table: "AuditLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewValues",
                table: "AuditLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "AuditLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Info",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Info");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AuditLogs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId1",
                table: "AuditLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "AuditLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetRoles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scopes = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IpWhitelist = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UsageCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_ServiceInstances_ServiceInstanceId",
                        column: x => x.ServiceInstanceId,
                        principalTable: "ServiceInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillingAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AlertLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingAlerts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Channel = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "normal"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceBackups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BackupType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageLocation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceBackups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceBackups_ServiceInstances_ServiceInstanceId",
                        column: x => x.ServiceInstanceId,
                        principalTable: "ServiceInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "info"),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessingResult = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceEvents_ServiceInstances_ServiceInstanceId",
                        column: x => x.ServiceInstanceId,
                        principalTable: "ServiceInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConfigurationTemplate = table.Column<string>(type: "text", nullable: false),
                    DefaultValues = table.Column<string>(type: "text", nullable: false),
                    RequiredPlan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Free"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IconUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "User"),
                    InvitationToken = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantInvitations_AspNetUsers_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TenantInvitations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SettingValue = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "string"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsEditable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsageRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceInstanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageRecords_ServiceInstances_ServiceInstanceId",
                        column: x => x.ServiceInstanceId,
                        principalTable: "ServiceInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsageRecords_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMetrics_ServiceInstanceId_Timestamp_MetricName",
                table: "ServiceMetrics",
                columns: new[] { "ServiceInstanceId", "Timestamp", "MetricName" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMetrics_ServiceInstanceId1",
                table: "ServiceMetrics",
                column: "ServiceInstanceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInstances_ServiceDefinitionId1",
                table: "ServiceInstances",
                column: "ServiceDefinitionId1");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInstances_TenantId_Name",
                table: "ServiceInstances",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDefinitions_Type",
                table: "ServiceDefinitions",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TenantId1",
                table: "AuditLogs",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId1",
                table: "AuditLogs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyHash",
                table: "ApiKeys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_ServiceInstanceId",
                table: "ApiKeys",
                column: "ServiceInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_TenantId_IsActive",
                table: "ApiKeys",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BillingAlerts_TenantId_IsActive",
                table: "BillingAlerts",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status_CreatedAt",
                table: "Notifications",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantId",
                table: "Notifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceBackups_ServiceInstanceId_CreatedAt",
                table: "ServiceBackups",
                columns: new[] { "ServiceInstanceId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvents_IsProcessed_Timestamp",
                table: "ServiceEvents",
                columns: new[] { "IsProcessed", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEvents_ServiceInstanceId_Timestamp",
                table: "ServiceEvents",
                columns: new[] { "ServiceInstanceId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvitations_Email_TenantId_Status",
                table: "TenantInvitations",
                columns: new[] { "Email", "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvitations_InvitationToken",
                table: "TenantInvitations",
                column: "InvitationToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvitations_InvitedByUserId",
                table: "TenantInvitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantInvitations_TenantId",
                table: "TenantInvitations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSettings_TenantId_SettingKey",
                table: "TenantSettings",
                columns: new[] { "TenantId", "SettingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_ServiceInstanceId",
                table: "UsageRecords",
                column: "ServiceInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_TenantId_Timestamp_MetricType",
                table: "UsageRecords",
                columns: new[] { "TenantId", "Timestamp", "MetricType" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId1",
                table: "AuditLogs",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId1",
                table: "AuditLogs",
                column: "TenantId1",
                principalTable: "Tenants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceInstances_ServiceDefinitions_ServiceDefinitionId",
                table: "ServiceInstances",
                column: "ServiceDefinitionId",
                principalTable: "ServiceDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceInstances_ServiceDefinitions_ServiceDefinitionId1",
                table: "ServiceInstances",
                column: "ServiceDefinitionId1",
                principalTable: "ServiceDefinitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceMetrics_ServiceInstances_ServiceInstanceId1",
                table: "ServiceMetrics",
                column: "ServiceInstanceId1",
                principalTable: "ServiceInstances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AspNetUsers_UserId1",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Tenants_TenantId1",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceInstances_ServiceDefinitions_ServiceDefinitionId",
                table: "ServiceInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceInstances_ServiceDefinitions_ServiceDefinitionId1",
                table: "ServiceInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceMetrics_ServiceInstances_ServiceInstanceId1",
                table: "ServiceMetrics");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "BillingAlerts");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ServiceBackups");

            migrationBuilder.DropTable(
                name: "ServiceEvents");

            migrationBuilder.DropTable(
                name: "ServiceTemplates");

            migrationBuilder.DropTable(
                name: "TenantInvitations");

            migrationBuilder.DropTable(
                name: "TenantSettings");

            migrationBuilder.DropTable(
                name: "UsageRecords");

            migrationBuilder.DropIndex(
                name: "IX_ServiceMetrics_ServiceInstanceId_Timestamp_MetricName",
                table: "ServiceMetrics");

            migrationBuilder.DropIndex(
                name: "IX_ServiceMetrics_ServiceInstanceId1",
                table: "ServiceMetrics");

            migrationBuilder.DropIndex(
                name: "IX_ServiceInstances_ServiceDefinitionId1",
                table: "ServiceInstances");

            migrationBuilder.DropIndex(
                name: "IX_ServiceInstances_TenantId_Name",
                table: "ServiceInstances");

            migrationBuilder.DropIndex(
                name: "IX_ServiceDefinitions_Type",
                table: "ServiceDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TenantId1",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId1",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CurrentApiCalls",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CurrentStorage",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ServiceInstanceId1",
                table: "ServiceMetrics");

            migrationBuilder.DropColumn(
                name: "ServiceDefinitionId1",
                table: "ServiceInstances");

            migrationBuilder.DropColumn(
                name: "TenantId1",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AuditLogs");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "ServiceMetrics",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ServiceInstances",
                type: "text",
                nullable: false,
                defaultValue: "Created",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Created");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServiceInstances",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "EndpointUrl",
                table: "ServiceInstances",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Configuration",
                table: "ServiceInstances",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ApiKey",
                table: "ServiceInstances",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "ServiceDefinitions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ServiceDefinitions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ServiceDefinitions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefaultConfiguration",
                table: "ServiceDefinitions",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ConfigurationSchema",
                table: "ServiceDefinitions",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "OldValues",
                table: "AuditLogs",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewValues",
                table: "AuditLogs",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "AuditLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Info",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Info");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AuditLogs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetRoles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AspNetRoles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "ServiceDefinitions",
                columns: new[] { "Id", "ConfigurationSchema", "CreatedAt", "DefaultConfiguration", "Description", "IconUrl", "IsActive", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "{\r\n    \"type\": \"object\",\r\n    \"properties\": {\r\n        \"bucketName\": {\"type\": \"string\", \"required\": true},\r\n        \"region\": {\"type\": \"string\", \"default\": \"us-east-1\"},\r\n        \"encryption\": {\"type\": \"boolean\", \"default\": false},\r\n        \"versioning\": {\"type\": \"boolean\", \"default\": false}\r\n    }\r\n}", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(501), "{\r\n    \"bucketName\": \"\",\r\n    \"region\": \"us-east-1\",\r\n    \"encryption\": false,\r\n    \"versioning\": false\r\n}", "Object storage service similar to AWS S3", "/icons/storage.svg", true, "S3-like Storage", "S3Storage", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(504) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "{\r\n    \"type\": \"object\",\r\n    \"properties\": {\r\n        \"engine\": {\"type\": \"string\", \"enum\": [\"postgresql\", \"mysql\", \"sqlserver\"], \"default\": \"postgresql\"},\r\n        \"instanceClass\": {\"type\": \"string\", \"default\": \"db.t3.micro\"},\r\n        \"allocatedStorage\": {\"type\": \"integer\", \"default\": 20},\r\n        \"multiAZ\": {\"type\": \"boolean\", \"default\": false}\r\n    }\r\n}", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(517), "{\r\n    \"engine\": \"postgresql\",\r\n    \"instanceClass\": \"db.t3.micro\",\r\n    \"allocatedStorage\": 20,\r\n    \"multiAZ\": false\r\n}", "Relational database service similar to AWS RDS", "/icons/database.svg", true, "RDS-like Database", "RDSDatabase", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(517) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "{\r\n    \"type\": \"object\",\r\n    \"properties\": {\r\n        \"tableName\": {\"type\": \"string\", \"required\": true},\r\n        \"billingMode\": {\"type\": \"string\", \"enum\": [\"PROVISIONED\", \"PAY_PER_REQUEST\"], \"default\": \"PAY_PER_REQUEST\"},\r\n        \"readCapacity\": {\"type\": \"integer\", \"default\": 5},\r\n        \"writeCapacity\": {\"type\": \"integer\", \"default\": 5}\r\n    }\r\n}", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(522), "{\r\n    \"tableName\": \"\",\r\n    \"billingMode\": \"PAY_PER_REQUEST\",\r\n    \"readCapacity\": 5,\r\n    \"writeCapacity\": 5\r\n}", "NoSQL database service similar to AWS DynamoDB", "/icons/nosql.svg", true, "DynamoDB-like NoSQL", "NoSQLDatabase", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(522) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "{\r\n    \"type\": \"object\",\r\n    \"properties\": {\r\n        \"queueName\": {\"type\": \"string\", \"required\": true},\r\n        \"visibilityTimeout\": {\"type\": \"integer\", \"default\": 30},\r\n        \"messageRetention\": {\"type\": \"integer\", \"default\": 1209600},\r\n        \"delaySeconds\": {\"type\": \"integer\", \"default\": 0}\r\n    }\r\n}", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(527), "{\r\n    \"queueName\": \"\",\r\n    \"visibilityTimeout\": 30,\r\n    \"messageRetention\": 1209600,\r\n    \"delaySeconds\": 0\r\n}", "Message queue service similar to AWS SQS", "/icons/queue.svg", true, "SQS-like Queue", "Queue", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(527) },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "{\r\n    \"type\": \"object\",\r\n    \"properties\": {\r\n        \"functionName\": {\"type\": \"string\", \"required\": true},\r\n        \"runtime\": {\"type\": \"string\", \"enum\": [\"dotnet8\", \"node18\", \"python3.9\"], \"default\": \"dotnet8\"},\r\n        \"timeout\": {\"type\": \"integer\", \"default\": 30},\r\n        \"memory\": {\"type\": \"integer\", \"default\": 128}\r\n    }\r\n}", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(531), "{\r\n    \"functionName\": \"\",\r\n    \"runtime\": \"dotnet8\",\r\n    \"timeout\": 30,\r\n    \"memory\": 128\r\n}", "Serverless function service similar to AWS Lambda", "/icons/function.svg", true, "Lambda-like Functions", "Function", new DateTime(2025, 9, 3, 1, 48, 39, 424, DateTimeKind.Utc).AddTicks(532) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMetrics_ServiceInstanceId_MetricName_Timestamp",
                table: "ServiceMetrics",
                columns: new[] { "ServiceInstanceId", "MetricName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInstances_Name_TenantId",
                table: "ServiceInstances",
                columns: new[] { "Name", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInstances_TenantId",
                table: "ServiceInstances",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDefinitions_Name",
                table: "ServiceDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email_TenantId",
                table: "AspNetUsers",
                columns: new[] { "Email", "TenantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceInstances_ServiceDefinitions_ServiceDefinitionId",
                table: "ServiceInstances",
                column: "ServiceDefinitionId",
                principalTable: "ServiceDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
