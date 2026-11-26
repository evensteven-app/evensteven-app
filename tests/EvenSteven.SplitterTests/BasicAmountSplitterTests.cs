using EvenSteven.Infrastructure;

namespace EvenSteven.SplitterTests
{
    public class BasicAmountSplitterTests
    {
        [Theory]
        [InlineData(3, 1, new long[] { 1, 0, 0 })]
        [InlineData(1, 1000, new long[] { 1000 })]
        [InlineData(2, 1000, new long[] { 500, 500 })]
        [InlineData(3, 1000, new long[] { 334, 333, 333 })]
        [InlineData(7, 5000, new long[] { 715, 715, 714, 714, 714, 714, 714 })]
        public void SplitAmount_ReturnValidSharesByParticipant(int participantsNumber, long amount, long[] expectedValues)
        {
            List<Guid> participants = [];
            for (int i = 0; i < participantsNumber; i++)
            {
                participants.Add(Guid.NewGuid());
            }

            var sharedAmount = BasicAmountSplitter.SplitAmount(participants, amount);

            Assert.Equal(participantsNumber, sharedAmount.Count);
            Assert.Equal(expectedValues, sharedAmount.Select(x => x.Value).ToArray());
        }

        [Fact]
        public void SplitAmount_ZeroAmountReturnZeroedDictionary()
        {
            List<Guid> participants = [];
            for (int i = 0; i < 2; i++)
            {
                participants.Add(Guid.NewGuid());
            }

            long[] expected = [0, 0];

            var sharedAmount = BasicAmountSplitter.SplitAmount(participants, 0);

            Assert.Equal(expected, sharedAmount.Select(x => x.Value));
        }

        [Fact]
        public void SplitAmount_EmptyUsersReturnEmptyDictionary()
        {
            var sharedAmount = BasicAmountSplitter.SplitAmount([], 100);

            Assert.Empty(sharedAmount);
        }

        [Fact]
        public void SplitAmount_NegativeAmountReturnEmptyDictionary()
        {
            var sharedAmount = BasicAmountSplitter.SplitAmount([Guid.NewGuid()], -100);

            Assert.Empty(sharedAmount);
        }

        [Theory]
        [InlineData(10, 7)]
        [InlineData(13, 999999)]
        [InlineData(100, 123456789)]
        public void SplitAmount_SumOfSharesEqualsAmount(int participantsNumber, long amount)
        {
            List<Guid> participants = [];
            for (int i = 0; i < participantsNumber; i++)
            {
                participants.Add(Guid.NewGuid());
            }

            var sharedAmount = BasicAmountSplitter.SplitAmount(participants, amount);

            Assert.Equal(participantsNumber, sharedAmount.Count);
            Assert.Equal(amount, sharedAmount.Values.Sum());
        }

        [Fact]
        public void SplitAmount_ReturnsAllParticipantsAsKeys()
        {
            List<Guid> participants = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

            var sharedAmount = BasicAmountSplitter.SplitAmount(participants, 100);

            Assert.Equal(participants.OrderBy(x => x), sharedAmount.Keys.OrderBy(x => x));
        }

        [Fact]
        public void SplitAmount_DuplicateParticipantsThrowException()
        {
            var participant = Guid.NewGuid();
            List<Guid> participants = [participant, participant];

            Assert.Throws<ArgumentException>(() => BasicAmountSplitter.SplitAmount(participants, 100));
        }
    }
}
