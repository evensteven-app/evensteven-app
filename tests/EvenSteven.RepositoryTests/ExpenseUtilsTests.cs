using EvenSteven.Infrastructure.Storage.Utils;

namespace EvenSteven.RepositoryTests
{
    public class ExpenseUtilsTests
    {
        [Theory]
        [InlineData(1, 1000, new long[] { 1000 })]
        [InlineData(2, 1000, new long[] { 500, 500 })]
        [InlineData(3, 1000, new long[] { 334, 333, 333 })]
        [InlineData(7, 5000, new long[] { 715, 715, 714, 714, 714, 714, 714 })]
        public void SplitAmount_ReturnValidShareByParticipant(int participantsNumber, long amount, long[] expectedValues)
        {
            List<Guid> participantIds = [];
            for (int i = 0; i < participantsNumber; i++)
            {
                participantIds.Add(new Guid($"00000000-0000-0000-0000-{i:D12}"));
            }

            var distribution = ExpenseUtils.SplitAmount(participantIds, amount);

            Assert.Equal(expectedValues, distribution.Select(d => d.Value).ToArray());
        }

        [Fact]
        public void SplitAmount_NoParticipants_ReturnEmptyDictionary()
        {
            var distribution = ExpenseUtils.SplitAmount([], 1000);

            Assert.Empty(distribution);
        }

        [Fact]
        public void SplitAmount_ZeroAmount_AllSharesAreZero()
        {
            List<Guid> participantIds = [
                new Guid("00000000-0000-0000-0000-000000000001"),
                new Guid("00000000-0000-0000-0000-000000000002")
            ];

            var distribution = ExpenseUtils.SplitAmount(participantIds, 0);

            Assert.Equal(2, distribution.Count);
            Assert.All(distribution, d => Assert.Equal(0, d.Value));
        }

        [Fact]
        public void SplitAmount_AmountLessThanParticipantCount_DistributesRemainderToOne()
        {
            List<Guid> participantIds = [
                new Guid("00000000-0000-0000-0000-000000000001"),
                new Guid("00000000-0000-0000-0000-000000000002"),
                new Guid("00000000-0000-0000-0000-000000000003")
            ];

            var distribution = ExpenseUtils.SplitAmount(participantIds, 2);

            Assert.Equal(3, distribution.Count);
            Assert.Equal(2, distribution.Values.Sum());
            Assert.Equal(2, distribution.Values.Count(v => v == 1));
            Assert.Single(distribution.Values, v => v == 0);
        }

        [Fact]
        public void SplitAmount_NegativeAmount_ReturnEmptyDictionary()
        {
            List<Guid> participantIds = [
                new Guid("00000000-0000-0000-0000-000000000001"),
                new Guid("00000000-0000-0000-0000-000000000002"),
                new Guid("00000000-0000-0000-0000-000000000003")
            ];

            var distribution = ExpenseUtils.SplitAmount(participantIds, -1000);

            Assert.Empty(distribution);
        }

        [Fact]
        public void SplitAmount_MaxLongAmount_NoOverflow()
        {
            List<Guid> participantIds = [
                new Guid("00000000-0000-0000-0000-000000000001"),
                new Guid("00000000-0000-0000-0000-000000000002")
            ];

            var distribution = ExpenseUtils.SplitAmount(participantIds, long.MaxValue);

            Assert.Equal(long.MaxValue, distribution.Values.Sum());
        }

        [Fact]
        public void SplitAmount_DuplicateParticipantIds_ThrowsArgumentException()
        {
            var id = Guid.NewGuid();
            List<Guid> participantIds = [id, id];

            Assert.Throws<ArgumentException>(() => ExpenseUtils.SplitAmount(participantIds, 1000));
        }

        [Fact]
        public void SplitAmount_DoesNotMutateInputList()
        {
            List<Guid> participantIds = [
                new Guid("00000000-0000-0000-0000-000000000003"),
                new Guid("00000000-0000-0000-0000-000000000001"),
                new Guid("00000000-0000-0000-0000-000000000002")
            ];
            var originalOrder = participantIds.ToList();

            ExpenseUtils.SplitAmount(participantIds, 1000);

            Assert.Equal(originalOrder, participantIds);
        }
    }
}
