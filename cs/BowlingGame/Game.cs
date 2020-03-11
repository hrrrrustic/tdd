using System;
using System.Linq;
using System.Reflection;
using BowlingGame.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BowlingGame
{
    public class GameRound
    {
        public Boolean IsSpare { get; set; }
        public Boolean IsStrike { get; set; }
        public Int32 RollOne { get; set; }
        public Int32? ThirdRoll { get; set; }
        public Int32 CurrentRoll { get; set; } = 1;

        public Int32 RollSecond { get; set; }
    }

    public class Game
    {
        private const Int32 maxPins = 10;
        private const Int32 maxRounds = 9;

        private readonly GameRound[] rounds = new GameRound[maxRounds + 1];

        public Game()
        {
            for (Int32 i = 0; i < rounds.Length; i++) rounds[i] = new GameRound();
        }

        private Int32 CurrentRound { get; set; }

        public void Roll(Int32 pins)
        {
            if (pins < 0 || pins > maxPins)
                throw new ArgumentException();

            switch (rounds[CurrentRound].CurrentRoll)
            {
                case 1:
                    FirstRoll(pins);
                    break;
                case 2:
                    SecondRoll(pins);
                    break;
                case 3:
                    ThirdRoll(pins);
                    break;
            }
        }

        private void ThirdRoll(Int32 pins)
        {
            rounds[CurrentRound].ThirdRoll = pins;
        }

        private void FirstRoll(Int32 pins)
        {
            if (pins == maxPins)
            {
                GetStrike();
                return;
            }

            rounds[CurrentRound].RollOne = pins;
            rounds[CurrentRound].CurrentRoll++;
        }

        private void SecondRoll(Int32 pins)
        {
            if (rounds[CurrentRound].RollOne + pins == maxPins)
            {
                GetSpare(pins);
                return;
            }

            rounds[CurrentRound].RollSecond = pins;

            if (CurrentRound == maxRounds)
                rounds[CurrentRound].CurrentRoll++;
            else
                CurrentRound++;
        }

        private void GetSpare(Int32 pins)
        {
            rounds[CurrentRound].IsSpare = true;
            rounds[CurrentRound].RollSecond = pins;

            if (CurrentRound == maxRounds)
                rounds[CurrentRound].CurrentRoll++;
            else
                CurrentRound++;
        }

        private void GetStrike()
        {
            rounds[CurrentRound].IsStrike = true;
            rounds[CurrentRound].RollOne = maxPins;

            if (CurrentRound == maxRounds)
                rounds[CurrentRound].CurrentRoll++;
            else
                CurrentRound++;
        }

        public Int32 GetScore()
        {
            Int32 sum = 0;
            for (Int32 i = 0; i < rounds.Length; i++)
                if (rounds[i].IsStrike)
                    sum += maxPins + (i == maxRounds
                               ? rounds[i].RollSecond + (Int32) rounds[i].ThirdRoll
                               : NextRollsForStrike(i + 1));
                else if (rounds[i].IsSpare)
                    sum += maxPins + (i == maxRounds ? (Int32) rounds[i].ThirdRoll : rounds[i + 1].RollOne);
                else
                    sum += rounds[i].RollOne + rounds[i].RollSecond;

            return sum;
        }

        private Int32 NextRollsForStrike(Int32 roundIndex)
        {
            if (rounds[roundIndex].IsStrike)
                return maxPins + (roundIndex == maxRounds
                           ? rounds[roundIndex].RollSecond
                           : rounds[roundIndex + 1].RollOne);

            return rounds[roundIndex].RollOne + rounds[roundIndex].RollSecond;
        }
    }

    [TestFixture]
    public class Game_should : ReportingTest<Game_should>
    {
        private void SetCurrentRound(Game game, Int32 value)
        {
            game
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .First()
                .SetValue(game, value);
        }

        private GameRound GetRound(Game game, Int32 roundIndex)
        {
            return ((GameRound[]) game
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)[1]
                .GetValue(game))[roundIndex];
        }

        [Test]
        public void GameEnd_AndTryRollOneMoreTime_IndexOutOfRangeException()
        {
            Game game = new Game();
            SetCurrentRound(game, 10);
            Action act = () => game.Roll(1);
            act.ShouldThrow<IndexOutOfRangeException>();
        }

        [Test]
        public void GameWithAllSpares_ScoreIs150()
        {
            Game game = new Game();
            for (Int32 i = 0; i < 10; i++)
            {
                game.Roll(5);
                game.Roll(5);
            }

            game.Roll(5);

            game.GetScore().Should().Be(150);
        }

        [Test]
        public void GameWithAllStrikes_ScoreIs300()
        {
            Game game = new Game();

            for (Int32 i = 0; i < 12; i++) game.Roll(10);
            game.GetScore().Should().Be(300);
        }

        [Test]
        public void HaveZeroScore_BeforeAnyRolls()
        {
            new Game()
                .GetScore()
                .Should().Be(0);
        }

        [Test]
        public void LastRound_WithStrikeAndThird_RollIs5_ThirdRollScoreIs5()
        {
            Game game = new Game();
            SetCurrentRound(game, 9);
            game.Roll(1);
            game.Roll(9);
            game.Roll(5);
            GetRound(game, 9).ThirdRoll.Should().Be(5);
        }

        [Test]
        public void REFLECTION_IS_COOL()
        {
            1.Should().Be(1);
        }

        [Test]
        public void RollAllPinsInOneRoll_GameRoundIsStrike()
        {
            Game game = new Game();
            game.Roll(10);
            GetRound(game, 0)
                .IsStrike
                .Should()
                .Be(true);
        }

        [Test]
        public void RollAllPinsInTwoRolls_GameRoundIsSpare()
        {
            Game game = new Game();
            game.Roll(9);
            game.Roll(1);
            GetRound(game, 0).IsSpare.Should().Be(true);
        }

        [Test]
        public void RollMoreTenPins_ArgumentException()
        {
            Action act = () => new Game().Roll(11);
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void RollNegativePins_ArgumentException()
        {
            Action act = () => new Game().Roll(-1);
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void RoundWithoutStrikeOrSpare_With8And1_RollsSumIs9()
        {
            Game game = new Game();
            game.Roll(8);
            game.Roll(1);
            game.GetScore().Should().Be(9);
        }

        [Test]
        public void TestGame_ScoreIs133()
        {
            Game game = new Game();
            game.Roll(1);
            game.Roll(4);

            game.Roll(4);
            game.Roll(5);

            game.Roll(6);
            game.Roll(4);

            game.Roll(5);
            game.Roll(5);

            game.Roll(10);

            game.Roll(0);
            game.Roll(1);

            game.Roll(7);
            game.Roll(3);

            game.Roll(6);
            game.Roll(4);

            game.Roll(10);

            game.Roll(2);
            game.Roll(8);
            game.Roll(6);

            game.GetScore().Should().Be(133);
        }

        [Test]
        public void TwoCommonRolls_RoundIsNotSpareOrStrike()
        {
            Game game = new Game();
            game.Roll(8);
            game.Roll(1);
            var round = Tuple.Create(GetRound(game, 0).IsSpare, GetRound(game, 0).IsStrike);
            round.Should().Be(Tuple.Create(false, false));
        }
    }
}