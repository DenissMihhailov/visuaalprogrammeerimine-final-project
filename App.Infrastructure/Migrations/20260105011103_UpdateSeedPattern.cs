using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace App.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedPattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patterns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    StepsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Bpm = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    ResourcePath = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KitSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KitId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    SoundId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitSlots_Kits_KitId",
                        column: x => x.KitId,
                        principalTable: "Kits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatternSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PatternId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    StepIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOn = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatternSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatternSteps_Patterns_PatternId",
                        column: x => x.PatternId,
                        principalTable: "Patterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Patterns",
                columns: new[] { "Id", "Bpm", "Code", "CreatedAt", "Name", "Status", "StepsCount" },
                values: new object[] { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 128, "HOUSE-001", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "House Basic", 1, 16 });

            migrationBuilder.InsertData(
                table: "PatternSteps",
                columns: new[] { "Id", "IsOn", "PatternId", "Role", "StepIndex" },
                values: new object[,]
                {
                    { new Guid("0008cf7e-9f09-4eed-959c-400c2d71a447"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 6 },
                    { new Guid("04115c0e-87a7-4d4f-a579-d99e742ed10d"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 4 },
                    { new Guid("0415e9cf-cdb8-48c1-abac-498cdc243569"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 10 },
                    { new Guid("0bc7e79d-ac0d-47ce-80f7-aef9a7fef186"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 11 },
                    { new Guid("0ca8a81f-bcaf-4c5c-a81c-b6f1b5e6fbc3"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 7 },
                    { new Guid("0e69658f-c577-4b38-bd92-e0605270b819"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 9 },
                    { new Guid("0efc06fc-cab7-4ad6-ac8d-ec94acb10381"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 5 },
                    { new Guid("127bcda9-5901-49e9-89fd-fb59f2195ec0"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 12 },
                    { new Guid("140631e1-18b0-4478-bb18-d0466a779e7f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 14 },
                    { new Guid("142de131-308f-44da-bc01-a89215ff4d9a"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 2 },
                    { new Guid("17ef45e9-545f-4cc1-9710-f4370197749b"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 5 },
                    { new Guid("1da8f695-7246-4c1c-9d85-f6252c42ad07"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 11 },
                    { new Guid("1e8f527d-94d4-4185-91ba-eb3ca08cf07f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 13 },
                    { new Guid("1f2002ee-00ab-4ac1-8b89-da447bef6d05"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 10 },
                    { new Guid("206af8e9-72d0-44bc-aa2d-b37cd232e4f0"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 12 },
                    { new Guid("22cdc8c6-b0e3-45a1-a3ff-204e59fc9361"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 9 },
                    { new Guid("245d53cf-28e8-418c-b7e0-bcad19264458"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 9 },
                    { new Guid("32a433b2-0f56-4ded-9b5e-c0dc3b69e82b"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 13 },
                    { new Guid("33239278-a727-4120-920a-40695c70c384"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 9 },
                    { new Guid("3386304e-fffe-4091-8149-43b2f27b0caf"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 10 },
                    { new Guid("338a0906-9abb-4f00-9e81-c4c3df8b9638"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 2 },
                    { new Guid("342c405b-7db7-4a21-ba85-706948803897"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 8 },
                    { new Guid("37fac3ae-bece-4b9f-a0c4-9f590c1e03b5"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 6 },
                    { new Guid("39a4d16e-3c73-4f44-8d3d-3c59e5b10e56"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 4 },
                    { new Guid("39a7e2f0-0bbc-4178-a64c-2fa02f6d018a"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 1 },
                    { new Guid("3dd75e64-e793-481c-98b8-bf4f9d80b8b7"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 11 },
                    { new Guid("3e8321f3-d145-4ed1-a758-247c75df37b1"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 7 },
                    { new Guid("43a8f992-f275-4f8f-81aa-37050ce0163e"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 13 },
                    { new Guid("446655bf-3a60-40e1-b145-2f6392169c5d"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 0 },
                    { new Guid("448c526c-1653-4596-a897-bb258319ab9e"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 6 },
                    { new Guid("4784b061-4eba-4b8e-901b-a71dc23f5162"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 9 },
                    { new Guid("4b22a9f8-5146-443e-8fe5-a6f145b7986c"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 10 },
                    { new Guid("4c1e47fe-83f8-4319-abc9-e89d5e3ef646"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 5 },
                    { new Guid("4d6dcef0-a2a4-4ecf-9153-59927cb94d6e"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 2 },
                    { new Guid("4dcf6309-ebea-4b9b-abd4-9ee10f61c651"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 13 },
                    { new Guid("533921c5-7cb5-46cd-a18c-4bf4bbb2f036"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 3 },
                    { new Guid("54ce0fcd-9bd2-47a9-add0-163e6ef5b186"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 15 },
                    { new Guid("55aea621-fc13-4799-972f-fa32e42f9eae"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 4 },
                    { new Guid("5c87093b-d8a6-40d8-893c-5a89535bf739"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 0 },
                    { new Guid("603c311e-5ebf-46ed-81f5-fa9f8f634349"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 1 },
                    { new Guid("6558cbef-87a7-4f9e-b1f8-0e929f46b1cc"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 7 },
                    { new Guid("65f51162-66f3-4ee7-a8af-cbb81e5ccbb3"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 11 },
                    { new Guid("68018382-48f1-4f63-84a8-a68daad218c5"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 7 },
                    { new Guid("6951aba8-3b3d-4788-b1f9-1d02a3ad0891"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 13 },
                    { new Guid("6b5aab0b-a704-4dda-a573-4e0c0c4eee6a"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 14 },
                    { new Guid("6d0f4d59-05cb-46bf-bc47-18a95243a3e5"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 0 },
                    { new Guid("70aed7aa-0570-4228-bebf-7cab69e7099f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 15 },
                    { new Guid("739fd4f5-fca2-4f6e-bc7e-892247f1676b"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 10 },
                    { new Guid("75a11afe-1d86-4023-803e-47956b139172"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 13 },
                    { new Guid("79553905-ccf0-4021-9b2b-f66f41cb573e"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 12 },
                    { new Guid("7d7d2e15-1d68-49ab-a4b6-a812e6d294e5"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 0 },
                    { new Guid("7eb7ab3d-ab73-4a24-a49f-a85809ecdb7f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 4 },
                    { new Guid("7ff9c9b0-9621-4c24-aa2c-c390f8974c7f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 11 },
                    { new Guid("80d9991a-8431-46f1-acf6-12a192e44593"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 5 },
                    { new Guid("87af418d-a97d-4252-a1ad-2547b7987799"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 14 },
                    { new Guid("884d3734-ea45-411e-9c00-1f96a9a4e2d2"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 12 },
                    { new Guid("8b387f6b-9c28-47d3-98da-dacd2c6cd642"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 9 },
                    { new Guid("8b7d3b53-bea0-41f6-814a-d49bf5cc67ff"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 0 },
                    { new Guid("8dab70c4-6e99-4c94-8bca-9c581cd7fad0"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 3 },
                    { new Guid("8e873f48-5f4e-4258-9035-6b15c5c88a2c"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 11 },
                    { new Guid("933b6b4b-c129-454f-95ed-5492b9044d0f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 3 },
                    { new Guid("96940867-15bc-478d-b881-2b6e40b861ec"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 8 },
                    { new Guid("99184f6d-4f90-4a83-9e17-f5d6ca2552a1"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 4 },
                    { new Guid("a278d6ac-4f44-4a00-861f-74aec3bd07ff"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 3 },
                    { new Guid("a48d4c4c-255f-4b77-9b78-fe8b0bf12c5c"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 7 },
                    { new Guid("a52c3e03-a4ce-4b5a-b4fe-9083e6d6ee51"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 8 },
                    { new Guid("a6fb1405-caec-41a4-abe8-814934683899"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 15 },
                    { new Guid("a7dc15e1-4c34-4763-915e-fddefa8cef9f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 14 },
                    { new Guid("ab833f98-29dc-4e69-a408-0a0cde68142e"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 5 },
                    { new Guid("b03eee11-3abc-4366-8296-534eaa6ef103"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 1 },
                    { new Guid("b04eb833-3519-4f01-a60d-25a8dda5d1f0"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 8 },
                    { new Guid("b06ebec6-97d3-4562-a462-11cdf31a00a0"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 15 },
                    { new Guid("b4139727-89c8-4695-87e4-9b75c7926143"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 6 },
                    { new Guid("b4d3ad9c-7a8e-42af-bb0b-d2192fbf51f8"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 8 },
                    { new Guid("b589f79a-8e3c-486f-b307-58d752473d91"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 8 },
                    { new Guid("b90640cf-45ea-48d0-bce8-c7ce6b3ff295"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 3 },
                    { new Guid("bad41383-a2ba-4046-9804-6e936baa763f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 6 },
                    { new Guid("c585f04c-3ae0-467f-be64-da33d4c699ce"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 0 },
                    { new Guid("c69183a5-420f-4a6f-a053-cb0c7b4bc202"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 1 },
                    { new Guid("c826a358-3e02-4afe-85f0-013513e0a548"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 14 },
                    { new Guid("caa47db5-6318-4334-9f3d-a522660a08c7"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 5 },
                    { new Guid("cef22a74-046c-48ec-a88f-a24012aa3be7"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 1 },
                    { new Guid("cf5fb09b-b52c-48a5-9c7c-b22e43e67aed"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 1 },
                    { new Guid("d49be055-c0f2-4052-b26a-77acb8ae3f1f"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 15 },
                    { new Guid("d97ccf87-823f-452f-bcfd-f05470337802"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 12 },
                    { new Guid("db2bc39a-ff85-4e77-bdfa-58003b881da1"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 0, 6 },
                    { new Guid("db3cbff1-9d9a-4212-ac41-392b19fbbc9b"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 3, 2 },
                    { new Guid("dc4347f5-8ce2-42c0-91d7-f07e287eab6d"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 7 },
                    { new Guid("dcd9b37d-b67c-4582-8bc7-30524da6822d"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 15 },
                    { new Guid("e0858707-a345-45b6-b0fa-74b4b189fc2a"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 4 },
                    { new Guid("e15986c8-d86b-47e6-96f3-32d583944ee6"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 2, 14 },
                    { new Guid("e7e2a393-2b08-4231-b79e-f398f26d18eb"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 2 },
                    { new Guid("f7658912-02ea-4a21-9038-3ec50a878808"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5, 3 },
                    { new Guid("fa0bd08a-cb90-4f89-8291-270510b399e4"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 10 },
                    { new Guid("fa8a45b1-4f75-4e22-b36b-84ea72dbcaa6"), false, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 4, 2 },
                    { new Guid("fcc24de2-d47b-4b09-8c30-83a5725c6447"), true, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 1, 12 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kits_Name",
                table: "Kits",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitSlots_KitId_Role",
                table: "KitSlots",
                columns: new[] { "KitId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patterns_Code",
                table: "Patterns",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatternSteps_PatternId_Role_StepIndex",
                table: "PatternSteps",
                columns: new[] { "PatternId", "Role", "StepIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KitSlots");

            migrationBuilder.DropTable(
                name: "PatternSteps");

            migrationBuilder.DropTable(
                name: "Sounds");

            migrationBuilder.DropTable(
                name: "Kits");

            migrationBuilder.DropTable(
                name: "Patterns");
        }
    }
}
