using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocFinder.Services.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateRemake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Owner = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Ext = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    SizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sha256 = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false, defaultValue: new byte[0])
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileId);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Owner = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Data",
                columns: table => new
                {
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataVersion = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", nullable: false),
                    DataBytes = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Data", x => x.FileId);
                    table.ForeignKey(
                        name: "FK_Data_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FileListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ListId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PinnedSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AddedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false),
                    FileId1 = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileListId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileListItems_FileLists_FileListId",
                        column: x => x.FileListId,
                        principalTable: "FileLists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FileListItems_FileLists_ListId",
                        column: x => x.ListId,
                        principalTable: "FileLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileListItems_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FileListItems_Files_FileId1",
                        column: x => x.FileId1,
                        principalTable: "Files",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Protocols",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AttachmentsListId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    OrganizationalUnit = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    AssetId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IssuedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LegalBasis = table.Column<string>(type: "TEXT", nullable: true),
                    ResponsiblePerson = table.Column<string>(type: "TEXT", nullable: false),
                    IssueDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Print = table.Column<bool>(type: "INTEGER", nullable: false),
                    ElectronicVersion = table.Column<bool>(type: "INTEGER", nullable: false),
                    Contract = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protocols", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Protocols_FileLists_AttachmentsListId",
                        column: x => x.AttachmentsListId,
                        principalTable: "FileLists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Protocols_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "FileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ListId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    PinnedVersion = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    PinnedFileSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AddedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false),
                    ProtocolListId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolListItems_ProtocolLists_ListId",
                        column: x => x.ListId,
                        principalTable: "ProtocolLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtocolListItems_ProtocolLists_ProtocolListId",
                        column: x => x.ProtocolListId,
                        principalTable: "ProtocolLists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProtocolListItems_Protocols_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "Protocols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileListItems_FileId",
                table: "FileListItems",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileListItems_FileId1",
                table: "FileListItems",
                column: "FileId1");

            migrationBuilder.CreateIndex(
                name: "IX_FileListItems_FileListId",
                table: "FileListItems",
                column: "FileListId");

            migrationBuilder.CreateIndex(
                name: "IX_FileListItems_ListId_FileId",
                table: "FileListItems",
                columns: new[] { "ListId", "FileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileListItems_ListId_Order",
                table: "FileListItems",
                columns: new[] { "ListId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_FilePath",
                table: "Files",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_Files_Name_Ext",
                table: "Files",
                columns: new[] { "Name", "Ext" });

            migrationBuilder.CreateIndex(
                name: "IX_Files_Sha256",
                table: "Files",
                column: "Sha256",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolListItems_ListId_Order",
                table: "ProtocolListItems",
                columns: new[] { "ListId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolListItems_ListId_ProtocolId",
                table: "ProtocolListItems",
                columns: new[] { "ListId", "ProtocolId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolListItems_ProtocolId",
                table: "ProtocolListItems",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolListItems_ProtocolListId",
                table: "ProtocolListItems",
                column: "ProtocolListId");

            migrationBuilder.CreateIndex(
                name: "IX_Protocols_AttachmentsListId",
                table: "Protocols",
                column: "AttachmentsListId");

            migrationBuilder.CreateIndex(
                name: "IX_Protocols_FileId",
                table: "Protocols",
                column: "FileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "Data");

            migrationBuilder.DropTable(
                name: "FileListItems");

            migrationBuilder.DropTable(
                name: "ProtocolListItems");

            migrationBuilder.DropTable(
                name: "ProtocolLists");

            migrationBuilder.DropTable(
                name: "Protocols");

            migrationBuilder.DropTable(
                name: "FileLists");

            migrationBuilder.DropTable(
                name: "Files");
        }
    }
}
