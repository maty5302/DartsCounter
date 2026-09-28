using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain
{
	public class Tournament
	{
		public List<Match> Matches { get; set; } = new();
		public List<PlayerDto> Players { get; set; } = new();
		public List<List<Match>> AllMatches { get; set; } = new();
		public int Round { get; private set; }

		// Backward compatibility aliases
		public List<Match> matches
		{
			get => Matches;
			set => Matches = value;
		}

		public List<PlayerDto> players
		{
			get => Players;
			set => Players = value;
		}

		public List<List<Match>> allmatches
		{
			get => AllMatches;
			set => AllMatches = value;
		}

		public bool IsFinished => Matches.Count == 1 && Matches[0].WinnerId != 0;

		public PlayerDto? Winner
		{
			get
			{
				if (!IsFinished) return null;
				return Players.FirstOrDefault(p => p.Id == Matches[0].WinnerId);
			}
		}

		public Tournament(List<PlayerDto> players)
		{
			if (players == null || players.Count < 2)
			{
				throw new ArgumentException("Tournament requires at least 2 players.", nameof(players));
			}

			Players = new List<PlayerDto>(players);
			Matches = new List<Match>();
			AllMatches = new List<List<Match>>();
			GenerateInitialMatches();
		}

		private void GenerateInitialMatches()
		{
			Round = 1;
			int matchesPerRound = Players.Count / 2;

			for (int i = 0; i < matchesPerRound; i++)
			{
				Matches.Add(new Match(Convert.ToInt32(Players[i].Id), Convert.ToInt32(Players[Players.Count - 1 - i].Id), Round));
			}

			AllMatches.Add(Matches);
		}

		public Match? GetNextMatch()
		{			
			return Matches.Find(x => x.WinnerId == 0);
		}

		public Match? getNextMatch() => GetNextMatch();

		public bool GenerateNextRound()
		{
			// Do not advance if the current round has unfinished matches
			if (Matches.Any(m => m.WinnerId == 0))
			{
				return false;
			}

			var winners = new List<PlayerDto>();
			foreach (var match in Matches)
			{
				var player = Players.Find(x => x.Id == match.WinnerId);
				if (player != null)
				{
					winners.Add(player);
				}
			}

			// If only 1 winner remains, the tournament has concluded
			if (winners.Count < 2)
			{
				return false;
			}

			Matches = new List<Match>();
			Round++;
			int matchesPerRound = winners.Count / 2;

			for (int i = 0; i < matchesPerRound; i++)
			{
				Matches.Add(new Match(Convert.ToInt32(winners[i * 2].Id), Convert.ToInt32(winners[i * 2 + 1].Id), Round));
			}

			AllMatches.Add(Matches);
			return true;
		}

		public void generateNextRound() => GenerateNextRound();
	}
}
