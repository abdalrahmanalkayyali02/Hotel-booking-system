using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HBS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Languages",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Backfill only recognized language names; never assign a shared placeholder code.
            migrationBuilder.Sql("""
                UPDATE `Languages`
                SET `Code` = CASE
                    WHEN `LanguageName` = 'English' THEN 'en'
                    WHEN `LanguageName` IN ('العربية', 'Arabic') THEN 'ar'
                END
                WHERE `LanguageName` IN ('English', 'العربية', 'Arabic');
                """);

            // Strict conversion rejects unmapped rows instead of converting NULL to an empty string.
            // Use SQL directly: EF's nullable-to-required operation can replace NULL with a default.
            migrationBuilder.Sql("""
                SET @hbs_language_code_sql_mode = @@SESSION.sql_mode;
                SET SESSION sql_mode = CONCAT_WS(',', NULLIF(@@SESSION.sql_mode, ''), 'STRICT_ALL_TABLES');
                ALTER TABLE `Languages` MODIFY COLUMN `Code` varchar(10) CHARACTER SET utf8mb4 NOT NULL;
                SET SESSION sql_mode = @hbs_language_code_sql_mode;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Languages_Code",
                table: "Languages",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Languages_Code",
                table: "Languages");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Languages");
        }
    }
}
