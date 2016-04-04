using UnityEngine;

public class MazeWall : MazeCellEdge {

	public Transform wall;

	public override void Initialize (
		MazeCell cell, 
		MazeCell otherCell, 
		MazeDirection direction) 
	{
		base.Initialize (cell, otherCell, direction);
		//wall.GetComponent<Renderer>().material = cell.room.settings.wallMaterial;
	}
		
	public override bool IsPassable() {
		return false;
	}
	
	public override bool IsObstacle() {
		return true;
	}
}