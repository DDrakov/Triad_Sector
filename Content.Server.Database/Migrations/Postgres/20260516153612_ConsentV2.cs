using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class ConsentV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bank_balance",
                table: "profile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "company",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "height",
                table: "profile",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "width",
                table: "profile",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "mono_coins",
                table: "preference",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "short_name",
                table: "admin_rank",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_balance",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "company",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "height",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "width",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "mono_coins",
                table: "preference");

            migrationBuilder.DropColumn(
                name: "short_name",
                table: "admin_rank");
        }
    }
}
