using System;
using Xunit;
using Domain;

namespace Domain.Tests
{
    public class PlayerMatchStatisticsTests
    {
        [Fact]
        public void NewInstance_ShouldBeEmptyAndHaveZeroAverage()
        {
            var stats = new PlayerMatchStatistics();

            Assert.True(stats.IsEmpty);
            Assert.Equal(0.0, stats.CurrentAverage);
        }

        [Fact]
        public void AddThrow_ShouldUpdateAverageAndIsEmpty()
        {
            var stats = new PlayerMatchStatistics();

            stats.AddThrow("60", 60);

            Assert.False(stats.IsEmpty);
            Assert.Equal(60.0, stats.CurrentAverage);

            stats.AddThrow("20", 20);

            Assert.Equal(40.0, stats.CurrentAverage);
        }

        [Fact]
        public void UndoLastThrow_OnEmpty_ShouldReturnNull()
        {
            var stats = new PlayerMatchStatistics();

            var result = stats.UndoLastThrow();

            Assert.Null(result);
        }

        [Fact]
        public void UndoLastThrow_ShouldReturnLastItemAndRestorePreviousState()
        {
            var stats = new PlayerMatchStatistics();
            stats.AddThrow("60", 60);
            stats.AddThrow("20", 20);

            var result = stats.UndoLastThrow();

            Assert.Equal("20", result); 
            Assert.Equal(60.0, stats.CurrentAverage);
            Assert.False(stats.IsEmpty);
        }

        [Fact]
        public void UndoLastThrow_UntilEmpty_ShouldResetState()
        {
            var stats = new PlayerMatchStatistics();
            stats.AddThrow("50", 50);

            stats.UndoLastThrow();

            Assert.True(stats.IsEmpty);
            Assert.Equal(0.0, stats.CurrentAverage);
        }

        [Fact]
        public void Clear_ShouldResetAllStatistics()
        {
            var stats = new PlayerMatchStatistics();
            stats.AddThrow("60", 60);
            stats.AddThrow("57", 57);

            stats.Clear();

            Assert.True(stats.IsEmpty);
            Assert.Equal(0.0, stats.CurrentAverage);
            
            var result = stats.UndoLastThrow();
            Assert.Null(result);
        }

        [Fact]
        public void UndoLastThrow_NormalThrowMatchingPreviousCheckout_ShouldNotRemoveCheckout()
        {
            var stats = new PlayerMatchStatistics();
            stats.AddThrow("40", 40, isCheckout: true);
            Assert.Equal(1, stats.Wins);
            Assert.Equal(40, stats.HighestOut);

            // Add normal throw with same score value
            stats.AddThrow("40", 40, isCheckout: false);
            Assert.Equal(1, stats.Wins);
            Assert.Equal(40, stats.HighestOut);

            // Undo the normal throw
            var undone = stats.UndoLastThrow();
            Assert.Equal("40", undone);
            Assert.Equal(1, stats.Wins);
            Assert.Equal(40, stats.HighestOut);

            // Undo the checkout
            var undoneCheckout = stats.UndoLastThrow();
            Assert.Equal("40", undoneCheckout);
            Assert.Equal(0, stats.Wins);
            Assert.Equal(0, stats.HighestOut);
        }
    }
}