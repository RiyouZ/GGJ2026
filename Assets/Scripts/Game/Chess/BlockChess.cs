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

		public BlockType blockType;

		public override void Initialize() 
		{
			// 网格坐标
			currentPos = GameScene.GetWorldCellPos(this.transform.position.x, this.transform.position.y);

			this.transform.position = GameScene.GetCellWorld(currentPos.x, currentPos.y);
		}

	}


}
