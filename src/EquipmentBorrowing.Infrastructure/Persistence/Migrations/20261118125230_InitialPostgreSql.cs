using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EquipmentBorrowing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    NormalizedIdentifier = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(regexp_replace(\"Identifier\", '^[[:space:]]+|[[:space:]]+$', '', 'g'))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.CheckConstraint("CK_Equipment_Description_NotBlank", "\"Description\" IS NULL OR \"Description\" ~ '[^[:space:]]'");
                    table.CheckConstraint("CK_Equipment_Identifier_NotBlank", "\"NormalizedIdentifier\" <> ''");
                    table.CheckConstraint("CK_Equipment_Name_NotBlank", "\"Name\" ~ '[^[:space:]]'");
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:IdentitySequenceOptions", "'100', '1', '', '', 'False', '1'")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentNumber = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Contact = table.Column<string>(type: "text", nullable: true),
                    IsAllowedToBorrow = table.Column<bool>(type: "boolean", nullable: false),
                    NormalizedStudentNumber = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(regexp_replace(\"StudentNumber\", '^[[:space:]]+|[[:space:]]+$', '', 'g'))", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.CheckConstraint("CK_Students_Contact_NotBlank", "\"Contact\" IS NULL OR \"Contact\" ~ '[^[:space:]]'");
                    table.CheckConstraint("CK_Students_Name_NotBlank", "\"Name\" ~ '[^[:space:]]'");
                    table.CheckConstraint("CK_Students_Number_NotBlank", "\"NormalizedStudentNumber\" <> ''");
                });

            migrationBuilder.CreateTable(
                name: "Borrowings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    DateBorrowed = table.Column<DateTime>(type: "date", nullable: false),
                    ExpectedReturnDate = table.Column<DateTime>(type: "date", nullable: false),
                    ActualReturnDate = table.Column<DateTime>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrowings", x => x.Id);
                    table.CheckConstraint("CK_Borrowings_ExpectedAfterCheckout", "\"ExpectedReturnDate\" > \"DateBorrowed\"");
                    table.CheckConstraint("CK_Borrowings_ReturnStatus", "(\"Status\" = 'Active' AND \"ActualReturnDate\" IS NULL) OR\n(\"Status\" = 'Returned' AND \"ActualReturnDate\" IS NOT NULL AND \"ActualReturnDate\" >= \"DateBorrowed\")");
                    table.ForeignKey(
                        name: "FK_Borrowings_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Borrowings_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Equipment",
                columns: new[] { "Id", "Description", "Identifier", "IsAvailable", "Name" },
                values: new object[,]
                {
                    { 1, null, "EQ-001", true, "Laptop" },
                    { 2, null, "EQ-002", true, "Projector" },
                    { 3, null, "EQ-003", true, "Microscope" },
                    { 4, null, "EQ-004", true, "Tablet" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "Contact", "IsAllowedToBorrow", "Name", "StudentNumber" },
                values: new object[,]
                {
                    { 1, null, true, "Juan Dela Cruz", "STU-001" },
                    { 2, null, true, "Maria Santos", "STU-002" },
                    { 3, null, false, "Pedro Reyes", "STU-003" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_ActiveEquipment",
                table: "Borrowings",
                column: "EquipmentId",
                unique: true,
                filter: "\"Status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_EquipmentId",
                table: "Borrowings",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_StudentId",
                table: "Borrowings",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_NormalizedIdentifier",
                table: "Equipment",
                column: "NormalizedIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_NormalizedStudentNumber",
                table: "Students",
                column: "NormalizedStudentNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Borrowings");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
