using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamenU2LP.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateForDbTransactional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.EnsureSchema(
                name: "security");

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    refresh_token = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    refresh_token_expire = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles_claims",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_roles_claims_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account_behavior_types",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_behavior_types", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_behavior_types_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_behavior_types_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entries",
                schema: "dbo",
                columns: table => new
                {
                    entry_number = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entries", x => x.entry_number);
                    table.ForeignKey(
                        name: "FK_entries_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entries_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users_claims",
                schema: "security",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_claims_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users_logins",
                schema: "security",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_logins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_users_logins_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users_roles",
                schema: "security",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_users_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "security",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_users_roles_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users_tokens",
                schema: "security",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_tokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_users_tokens_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chart_accounts",
                schema: "dbo",
                columns: table => new
                {
                    account_number = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    behavior_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    allows_movement = table.Column<bool>(type: "bit", nullable: false),
                    parent_account_number = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    is_disabled = table.Column<bool>(type: "bit", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chart_accounts", x => x.account_number);
                    table.ForeignKey(
                        name: "FK_chart_accounts_account_behavior_types_behavior_type_id",
                        column: x => x.behavior_type_id,
                        principalSchema: "dbo",
                        principalTable: "account_behavior_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chart_accounts_chart_accounts_parent_account_number",
                        column: x => x.parent_account_number,
                        principalSchema: "dbo",
                        principalTable: "chart_accounts",
                        principalColumn: "account_number",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chart_accounts_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chart_accounts_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "account_balances",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    month = table.Column<int>(type: "int", nullable: false),
                    account_number = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_balances", x => x.id);
                    table.ForeignKey(
                        name: "FK_account_balances_chart_accounts_account_number",
                        column: x => x.account_number,
                        principalSchema: "dbo",
                        principalTable: "chart_accounts",
                        principalColumn: "account_number",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_balances_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_account_balances_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entry_details",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entry_number = table.Column<int>(type: "int", nullable: false),
                    account_number = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    entry_position = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    updated_date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entry_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_entry_details_chart_accounts_account_number",
                        column: x => x.account_number,
                        principalSchema: "dbo",
                        principalTable: "chart_accounts",
                        principalColumn: "account_number",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entry_details_entries_entry_number",
                        column: x => x.entry_number,
                        principalSchema: "dbo",
                        principalTable: "entries",
                        principalColumn: "entry_number",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entry_details_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entry_details_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "security",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_balances_account_number",
                schema: "dbo",
                table: "account_balances",
                column: "account_number");

            migrationBuilder.CreateIndex(
                name: "IX_account_balances_created_by",
                schema: "dbo",
                table: "account_balances",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_account_balances_updated_by",
                schema: "dbo",
                table: "account_balances",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_account_behavior_types_created_by",
                schema: "dbo",
                table: "account_behavior_types",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_account_behavior_types_updated_by",
                schema: "dbo",
                table: "account_behavior_types",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_chart_accounts_behavior_type_id",
                schema: "dbo",
                table: "chart_accounts",
                column: "behavior_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_chart_accounts_created_by",
                schema: "dbo",
                table: "chart_accounts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_chart_accounts_parent_account_number",
                schema: "dbo",
                table: "chart_accounts",
                column: "parent_account_number");

            migrationBuilder.CreateIndex(
                name: "IX_chart_accounts_updated_by",
                schema: "dbo",
                table: "chart_accounts",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_entries_created_by",
                schema: "dbo",
                table: "entries",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_entries_updated_by",
                schema: "dbo",
                table: "entries",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_entry_details_account_number",
                schema: "dbo",
                table: "entry_details",
                column: "account_number");

            migrationBuilder.CreateIndex(
                name: "IX_entry_details_created_by",
                schema: "dbo",
                table: "entry_details",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_entry_details_entry_number",
                schema: "dbo",
                table: "entry_details",
                column: "entry_number");

            migrationBuilder.CreateIndex(
                name: "IX_entry_details_updated_by",
                schema: "dbo",
                table: "entry_details",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "security",
                table: "roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_roles_claims_RoleId",
                schema: "security",
                table: "roles_claims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "security",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "security",
                table: "users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_claims_UserId",
                schema: "security",
                table: "users_claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_logins_UserId",
                schema: "security",
                table: "users_logins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_roles_RoleId",
                schema: "security",
                table: "users_roles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_balances",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "entry_details",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "roles_claims",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users_claims",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users_logins",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users_roles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "users_tokens",
                schema: "security");

            migrationBuilder.DropTable(
                name: "chart_accounts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "entries",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "security");

            migrationBuilder.DropTable(
                name: "account_behavior_types",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "users",
                schema: "security");
        }
    }
}
