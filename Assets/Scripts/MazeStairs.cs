using System;

public class MazeStairs : MazeCellAccessory
{
	public MazeStairs ()
	{
	}

	public override bool IsPassable() {
		return true;
	}

	public override bool IsObstacle() {
		return true;
	}

	public override void OnPlayerEntered () {
		//StopCoroutine("UpdateDoorAngle");
		//targetAngle = 90f;
		//StartCoroutine("UpdateDoorAngle");
	}

	public override void OnPlayerExited () {
		//StopCoroutine("UpdateDoorAngle");
		//targetAngle = 0f;
		//StartCoroutine("UpdateDoorAngle");
	}
}

