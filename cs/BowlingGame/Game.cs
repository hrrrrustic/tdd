using System;
using System.Xml.Linq;
using BowlingGame.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BowlingGame
{
    public class GameRound
    {
        public bool IsSpare { get; set; }
        public bool IsStrike { get; set; }
        public int RollOne { get; set; }
        public int RollSecond { get; set; }

    }
    public class Game
    {
        public Game()
        {
            for (int i = 0; i < rounds.Length; i++)
            {
                rounds[i] = new GameRound();
            }
        }
        private readonly Int32 maxPins = 10;
        public int Score { get; set; }
        public int CurrentRound { get; set; }
        
        public readonly GameRound[] rounds = new GameRound[10];

        public void Roll(int pins)
        {
            if(pins < 0 || pins > 10)
                throw new ArgumentException();

            if (pins == 10)
                rounds[CurrentRound].IsStrike = true;
        }

        public int GetScore()
        {
            return Score;
        }
    }

    [TestFixture]
    public class Game_should : ReportingTest<Game_should>
    {
        [Test]
        public void HaveZeroScore_BeforeAnyRolls()
        {
            new Game()
                .GetScore()
                .Should().Be(0);
        }
        [Test]
        public void RollNegative_ArgumentException()
        {
            Action act = () => new Game().Roll(-1);
            act.ShouldThrow<ArgumentException>();
        }
        [Test]
        public void RollMoreTenPins_ArgumentException()
        {
            Action act = () => new Game().Roll(11);
            act.ShouldThrow<ArgumentException>();
        }
        [Test]
        public void RollAllPinsInOneRoll_GameRoundIsStrike()
        {
            var game = new Game();
            game.Roll(10);
            game.rounds[0].IsStrike.Should().Be(true);
        }

    }
}
