using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Classifier.Migrations
{
    /// <summary>
    /// Initial database migration that creates all required tables
    /// for document classification, including document classes,
    /// documents, embeddings, and class centroids.
    /// </summary>
    public partial class InitialClean : Migration
    {
        /// <summary>
        /// Applies the migration by creating database tables, relationships,
        /// constraints, and indexes required for the classification system.
        /// </summary>
        /// <param name="migrationBuilder">
        /// The builder used to define database schema changes.
        /// </param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Creates table to store document class categories
            migrationBuilder.CreateTable(
                name: "Team_404_DocumentClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team_404_DocumentClasses", x => x.Id);
                });

            // Creates table to store centroid vectors for each document class
            migrationBuilder.CreateTable(
                name: "Team_404_ClassCentroids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    Centroid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team_404_ClassCentroids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_404_ClassCentroids_Team_404_DocumentClasses_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Team_404_DocumentClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Creates table to store individual documents and their associated class
            migrationBuilder.CreateTable(
                name: "Team_404_Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team_404_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_404_Documents_Team_404_DocumentClasses_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Team_404_DocumentClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Creates table to store embedding vectors for each document
            migrationBuilder.CreateTable(
                name: "Team_404_DocumentEmbeddings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    Embedding = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team_404_DocumentEmbeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_404_DocumentEmbeddings_Team_404_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Team_404_Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Creates indexes to improve query performance and enforce constraints
            migrationBuilder.CreateIndex(
                name: "IX_Team_404_ClassCentroids_ClassId",
                table: "Team_404_ClassCentroids",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_404_DocumentClasses_Name",
                table: "Team_404_DocumentClasses",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_404_DocumentEmbeddings_DocumentId",
                table: "Team_404_DocumentEmbeddings",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_404_Documents_ClassId",
                table: "Team_404_Documents",
                column: "ClassId");
        }

        /// <summary>
        /// Reverts the migration by dropping all created tables
        /// in reverse order to maintain referential integrity.
        /// </summary>
        /// <param name="migrationBuilder">
        /// The builder used to define schema rollback operations.
        /// </param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drops centroid table
            migrationBuilder.DropTable(
                name: "Team_404_ClassCentroids");

            // Drops embeddings table
            migrationBuilder.DropTable(
                name: "Team_404_DocumentEmbeddings");

            // Drops documents table
            migrationBuilder.DropTable(
                name: "Team_404_Documents");

            // Drops document classes table
            migrationBuilder.DropTable(
                name: "Team_404_DocumentClasses");
        }
    }
}