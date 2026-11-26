using System.Data;
using FluentMigrator;

namespace EvenSteven.Infrastructure.Storage.Migrations
{
    [Migration(1)]
    public class CreatingMainTables : Migration
    {
        public override void Up()
        {
            Create.Table("Rooms")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("InviteCode").AsString().NotNullable().Unique()
                .WithColumn("EditKey").AsGuid().NotNullable().Unique()
                .WithColumn("PasswordHash").AsString().Nullable()
                .WithColumn("Version").AsInt64().NotNullable().WithDefaultValue(1)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime)
                .WithColumn("EditedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime);

            Create.Table("Participants")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("RoomId").AsGuid().NotNullable().Indexed()
                    .ForeignKey("FK_Participants_Rooms_RoomId", "Rooms", "Id").OnDelete(Rule.Cascade)
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("ParticipantKey").AsString().NotNullable()
                .WithColumn("IsDeleted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime);

            Create.Table("Expenses")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("RoomId").AsGuid().NotNullable().Indexed()
                    .ForeignKey("FK_Expenses_Rooms_RoomId", "Rooms", "Id").OnDelete(Rule.Cascade)
                .WithColumn("PayerId").AsGuid().NotNullable().Indexed()
                    .ForeignKey("FK_Expenses_Participants_PayerId", "Participants", "Id").OnDelete(Rule.None)
                .WithColumn("Amount").AsInt64().NotNullable()
                .WithColumn("Note").AsString().Nullable()
                .WithColumn("IsReverted").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("RevertedBy").AsGuid().Nullable().Indexed()
                    .ForeignKey("FK_Expenses_Participants_RevertedBy", "Participants", "Id").OnDelete(Rule.None)
                .WithColumn("RevertedAt").AsDateTime().Nullable().WithDefaultValue(null)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentUTCDateTime);

            Create.Table("ExpenseEntries")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("ExpenseId").AsGuid().NotNullable()
                    .ForeignKey("FK_ExpenseEntries_Expenses_ExpenseId", "Expenses", "Id").OnDelete(Rule.Cascade)
                .WithColumn("ParticipantId").AsGuid().NotNullable()
                    .ForeignKey("FK_ExpenseEntries_Participants_ParticipantId", "Participants", "Id").OnDelete(Rule.None)
                .WithColumn("Share").AsInt64().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("ExpenseEntries");
            Delete.Table("Expenses");
            Delete.Table("Participants");
            Delete.Table("Rooms");
        }
    }
}
