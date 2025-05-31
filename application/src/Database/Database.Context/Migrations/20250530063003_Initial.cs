using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Context.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "text", nullable: false),
                    registration_date = table.Column<DateOnly>(type: "date", nullable: false),
                    phone = table.Column<string>(type: "varchar(16)", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", nullable: false),
                    inn = table.Column<string>(type: "varchar(10)", nullable: false),
                    kpp = table.Column<string>(type: "varchar(9)", nullable: false),
                    ogrn = table.Column<string>(type: "varchar(13)", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company", x => x.id);
                    table.CheckConstraint("EmailCheck", "email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");
                    table.CheckConstraint("InnCheck", "inn ~ '^[0-9]{10}$'");
                    table.CheckConstraint("KppCheck", "kpp ~ '^[0-9]{9}$'");
                    table.CheckConstraint("OgrnChek", "ogrn ~ '^[0-9]{13}$'");
                    table.CheckConstraint("PhoneNumberCheck", "phone ~ '^\\+[0-9]{1,3}[0-9]{4,14}$'");
                    table.CheckConstraint("RegistrationDateCheck", "registration_date <= CURRENT_DATE");
                });

            migrationBuilder.CreateTable(
                name: "employee_base",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "varchar(16)", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    photo = table.Column<string>(type: "text", nullable: true),
                    duties = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employee_base", x => x.id);
                    table.CheckConstraint("BirthDateCheck", "birth_date < CURRENT_DATE");
                    table.CheckConstraint("EmailCheck", "email ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");
                    table.CheckConstraint("PhoneCheck", "phone ~ '^\\+[0-9]{1,3}[0-9]{4,14}$'");
                });

            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.id);
                    table.ForeignKey(
                        name: "FK_position_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_position_position_parent_id",
                        column: x => x.parent_id,
                        principalTable: "position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "text", nullable: false),
                    salary = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post", x => x.id);
                    table.CheckConstraint("salary_check", "salary > 0");
                    table.ForeignKey(
                        name: "FK_post_company_company_id",
                        column: x => x.company_id,
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "education",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution = table.Column<string>(type: "text", nullable: false),
                    education_level = table.Column<string>(type: "text", nullable: false),
                    study_field = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_education", x => x.id);
                    table.CheckConstraint("education_level_check", "education_level in ('Высшее (бакалавриат)', 'Высшее (магистратура)', 'Высшее (специалитет)', 'Среднее профессиональное (ПКР)', 'Среднее профессиональное (ПССЗ)','Программы переподготовки', 'Курсы повышения квалификации' )");
                    table.CheckConstraint("end_date_check", "end_date < CURRENT_DATE");
                    table.CheckConstraint("start_date_check", "start_date < CURRENT_DATE");
                    table.ForeignKey(
                        name: "FK_education_employee_base_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "position_history",
                columns: table => new
                {
                    position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position_history", x => new { x.position_id, x.employee_id });
                    table.CheckConstraint("CK_position_history_end_date", "end_date <= CURRENT_DATE");
                    table.CheckConstraint("CK_position_history_start_date", "start_date < CURRENT_DATE");
                    table.ForeignKey(
                        name: "FK_position_history_employee_base_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_position_history_position_position_id",
                        column: x => x.position_id,
                        principalTable: "position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "score_story",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    efficiency_score = table.Column<int>(type: "integer", nullable: false),
                    engagement_score = table.Column<int>(type: "integer", nullable: false),
                    competency_score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_score_story", x => x.id);
                    table.CheckConstraint("CK_ScoreStory_CompetencyScore", "competency_score > 0 AND competency_score < 6");
                    table.CheckConstraint("CK_ScoreStory_EfficiencyScore", "efficiency_score > 0 AND efficiency_score < 6");
                    table.CheckConstraint("CK_ScoreStory_EngagementScore", "engagement_score > 0 AND engagement_score < 6");
                    table.ForeignKey(
                        name: "FK_score_story_employee_base_author_id",
                        column: x => x.author_id,
                        principalTable: "employee_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_score_story_employee_base_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_score_story_position_position_id",
                        column: x => x.position_id,
                        principalTable: "position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "post_history",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    EmployeeDbId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostDbId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_history", x => new { x.post_id, x.employee_id });
                    table.CheckConstraint("CK_post_history_end_date", "end_date <= CURRENT_DATE");
                    table.CheckConstraint("CK_post_history_start_date", "start_date < CURRENT_DATE");
                    table.ForeignKey(
                        name: "FK_post_history_employee_base_EmployeeDbId",
                        column: x => x.EmployeeDbId,
                        principalTable: "employee_base",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_post_history_employee_base_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee_base",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_post_history_post_PostDbId",
                        column: x => x.PostDbId,
                        principalTable: "post",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_post_history_post_post_id",
                        column: x => x.post_id,
                        principalTable: "post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_company_email",
                table: "company",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_inn",
                table: "company",
                column: "inn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_kpp",
                table: "company",
                column: "kpp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_ogrn",
                table: "company",
                column: "ogrn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_phone",
                table: "company",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_company_title",
                table: "company",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_education_employee_id",
                table: "education",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_employee_base_email",
                table: "employee_base",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_base_phone",
                table: "employee_base",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employee_base_photo",
                table: "employee_base",
                column: "photo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_position_company_id",
                table: "position",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_position_parent_id",
                table: "position",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_position_history_employee_id",
                table: "position_history",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_company_id",
                table: "post",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_history_employee_id",
                table: "post_history",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_post_history_EmployeeDbId",
                table: "post_history",
                column: "EmployeeDbId");

            migrationBuilder.CreateIndex(
                name: "IX_post_history_PostDbId",
                table: "post_history",
                column: "PostDbId");

            migrationBuilder.CreateIndex(
                name: "IX_score_story_author_id",
                table: "score_story",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_score_story_employee_id",
                table: "score_story",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "IX_score_story_position_id",
                table: "score_story",
                column: "position_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "education");

            migrationBuilder.DropTable(
                name: "position_history");

            migrationBuilder.DropTable(
                name: "post_history");

            migrationBuilder.DropTable(
                name: "score_story");

            migrationBuilder.DropTable(
                name: "post");

            migrationBuilder.DropTable(
                name: "employee_base");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropTable(
                name: "company");
        }
    }
}
