﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MundoPrendarios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addfecha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Clientes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Clientes");
        }
    }
}
