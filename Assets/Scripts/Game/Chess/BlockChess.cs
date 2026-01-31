using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Game.GameChess
{
	public class BlockChess : Chess
	{
		public override Faction Faction => Faction.Neutral;

		public enum BlockType
		{
			Rock,
			Tree,
			Water
		}

	}


}
