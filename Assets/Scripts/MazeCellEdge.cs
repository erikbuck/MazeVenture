using UnityEngine;

public abstract class MazeCellEdge : MonoBehaviour {

	public MazeCell cell, otherCell;

	public MazeDirection direction;

	public virtual void Initialize (
		MazeCell cell, 
		MazeCell otherCell, 
		MazeDirection direction) 
	{
		this.cell = cell;
		this.otherCell = otherCell;
		this.direction = direction;
		cell.SetEdge(direction, this);
		if(null != otherCell)
		{
			if(cell == otherCell) {
				Debug.LogError("An edge has itself for a neighbor!");
			}
		    otherCell.SetEdge(direction.GetOpposite(), this);
		}
		transform.parent = cell.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = direction.ToRotation();
	}
	
	public virtual bool IsPassable() {
		return true;
	}

	public virtual bool IsObstacle() {
		return false;
	}
	
	public virtual void OnPlayerEntered () {}

	public virtual void OnPlayerExited () {}
}