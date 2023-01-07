using Microsoft.EntityFrameworkCore.Migrations;

namespace AuthenticationLinkdien.Migrations
{
    public partial class simpleChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefToken_AspNetUsers_UserId1",
                table: "RefToken");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "RefToken",
                newName: "UserRefId");

            migrationBuilder.RenameIndex(
                name: "IX_RefToken_UserId1",
                table: "RefToken",
                newName: "IX_RefToken_UserRefId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RefToken",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_RefToken_AspNetUsers_UserRefId",
                table: "RefToken",
                column: "UserRefId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefToken_AspNetUsers_UserRefId",
                table: "RefToken");

            migrationBuilder.RenameColumn(
                name: "UserRefId",
                table: "RefToken",
                newName: "UserId1");

            migrationBuilder.RenameIndex(
                name: "IX_RefToken_UserRefId",
                table: "RefToken",
                newName: "IX_RefToken_UserId1");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "RefToken",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RefToken_AspNetUsers_UserId1",
                table: "RefToken",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
