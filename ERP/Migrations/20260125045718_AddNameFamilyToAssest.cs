using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class AddNameFamilyToAssest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
         

            migrationBuilder.CreateTable(
                name: "AssestCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssestCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssestCategories_AssestCategories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AssestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssestUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Family = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssestUserTypes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssestUsers", x => x.Id);
                });

   
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_AssestCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AssestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

       
            migrationBuilder.CreateTable(
                name: "AssetProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PropertyValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastOwnerUserId = table.Column<int>(type: "int", nullable: true),
                    LastAssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetProperties_AssestCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "AssestCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetProperties_AssestUsers_LastOwnerUserId",
                        column: x => x.LastOwnerUserId,
                        principalTable: "AssestUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetProperties_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    AssetPropertyId = table.Column<int>(type: "int", nullable: true),
                    FromUserId = table.Column<int>(type: "int", nullable: false),
                    ToUserId = table.Column<int>(type: "int", nullable: false),
                    AssignDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AssestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetHistories_AssestUsers_AssestId",
                        column: x => x.AssestId,
                        principalTable: "AssestUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetHistories_AssestUsers_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "AssestUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetHistories_AssestUsers_ToUserId",
                        column: x => x.ToUserId,
                        principalTable: "AssestUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetHistories_AssetProperties_AssetPropertyId",
                        column: x => x.AssetPropertyId,
                        principalTable: "AssetProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetHistories_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

         
            migrationBuilder.CreateIndex(
                name: "IX_AssestCategories_ParentId",
                table: "AssestCategories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistories_AssestId",
                table: "AssetHistories",
                column: "AssestId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistories_AssetId",
                table: "AssetHistories",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistories_AssetPropertyId",
                table: "AssetHistories",
                column: "AssetPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistories_FromUserId",
                table: "AssetHistories",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetHistories_ToUserId",
                table: "AssetHistories",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetProperties_AssetId",
                table: "AssetProperties",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetProperties_CategoryId",
                table: "AssetProperties",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetProperties_LastOwnerUserId",
                table: "AssetProperties",
                column: "LastOwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetCode",
                table: "Assets",
                column: "AssetCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CategoryId",
                table: "Assets",
                column: "CategoryId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
       
            migrationBuilder.DropTable(
                name: "AssetHistories");

         

            migrationBuilder.DropTable(
                name: "AssetProperties");

           
            migrationBuilder.DropTable(
                name: "AssestUsers");

            migrationBuilder.DropTable(
                name: "Assets");

      
            migrationBuilder.DropTable(
                name: "AssestCategories");
        }
    }
}
