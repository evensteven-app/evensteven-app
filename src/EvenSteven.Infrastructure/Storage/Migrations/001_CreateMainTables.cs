using FluentMigrator;

namespace EvenSteven.Infrastructure.Storage.Migrations
{
    [Migration(1)]
    public class CreateMainTables : Migration
    {
        // TODO d.pichuzhkin: For PostgreSql migration
        public override void Down()
        {
            Delete.Table("Rooms");
            Delete.Table("Participants");
            Delete.Table("Expenses");
            Delete.Table("ExpenseEntries");

            //Delete.ForeignKey("FK_Rooms_Participants_RoomId");
            //Delete.ForeignKey("FK_Rooms_Expenses_RoomId");
            //Delete.ForeignKey("FK_Expenses_ExpenseEntries_ExpenseId");
            //Delete.ForeignKey("FK_Participants_ExpenseEntries_ParticipantId");
        }

        public override void Up()
        {
            Create.Table("Rooms")
                .WithColumn("Id").AsGuid().PrimaryKey().NotNullable().Unique()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("InviteCode").AsString().NotNullable().Unique()
                .WithColumn("EditKey").AsGuid().NotNullable().Unique()
                .WithColumn("PasswordHash").AsString().NotNullable()
                .WithColumn("Version").AsInt32().WithDefaultValue(1)
                .WithColumn("CreatedAt").AsDateTime().NotNullable();

            Create.Table("Participants")
                .WithColumn("Id").AsGuid().PrimaryKey().NotNullable().Unique()
                .WithColumn("RoomId").AsGuid().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("ParticipantKey").AsString().NotNullable();

            Create.Table("Expenses")
                .WithColumn("Id").AsGuid().PrimaryKey().NotNullable().Unique()
                .WithColumn("RoomId").AsGuid().NotNullable()
                .WithColumn("PayerId").AsGuid().NotNullable()
                .WithColumn("Amount").AsInt64().NotNullable()
                .WithColumn("Note").AsString().NotNullable()
                .WithColumn("IsReverted").AsBoolean().WithDefaultValue(false)
                .WithColumn("RevertedAt").AsDateTime().Nullable().WithDefaultValue(null)
                .WithColumn("CreatedAt").AsDateTime().NotNullable();

            Create.Table("ExpenseEntries")
                .WithColumn("Id").AsGuid().PrimaryKey().NotNullable().Unique()
                .WithColumn("ExpenseId").AsGuid().NotNullable()
                .WithColumn("ParticipantId").AsGuid().NotNullable()
                .WithColumn("Share").AsInt64().NotNullable();

            //Create.ForeignKey("FK_Rooms_Participants_RoomId")
            //    .FromTable("Participants").ForeignColumns("RoomId")
            //    .ToTable("Rooms").PrimaryColumn("Id")
            //    .OnDelete(System.Data.Rule.Cascade);

            //Create.ForeignKey("FK_Rooms_Expenses_RoomId")
            //    .FromTable("Expenses").ForeignColumns("RoomId")
            //    .ToTable("Rooms").PrimaryColumn("Id")
            //    .OnDelete(System.Data.Rule.Cascade);

            //Create.ForeignKey("FK_Expenses_ExpenseEntries_ExpenseId")
            //    .FromTable("ExpenseEntries").ForeignColumn("ExpenseId")
            //    .ToTable("Expenses").PrimaryColumn("Id")
            //    .OnDelete(System.Data.Rule.Cascade);

            //Create.ForeignKey("FK_Participants_EventEntries_ParticipantId")
            //    .FromTable("EventEntries").ForeignColumn("ParticipantId")
            //    .ToTable("Participants").PrimaryColumn("Id")
            //    .OnDelete(System.Data.Rule.Cascade);
        }
    }
}
