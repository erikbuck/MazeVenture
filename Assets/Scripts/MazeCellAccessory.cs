using System;
using UnityEngine;


public class MazeCellAccessory : MonoBehaviour
{
	public MazeCell cell;

	public virtual bool IsPassable ()
	{
		return true;
	}

	public virtual bool IsObstacle ()
	{
		return true;
	}

	public virtual void OnPlayerEntered ()
	{
	}

	public virtual void OnPlayerExited ()
	{
	}
}


