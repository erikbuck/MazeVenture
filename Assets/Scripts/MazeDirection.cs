using UnityEngine;

public enum MazeDirection {
	North,
	East,
	South,
	West
}

public static class MazeDirections {

	public const int Count = 4;

	public static MazeDirection RandomValue {
		get {
			return (MazeDirection)Random.Range(0, Count);
		}
	}

	private static MazeDirection[] opposites = {
		MazeDirection.South,
		MazeDirection.West,
		MazeDirection.North,
		MazeDirection.East
	};

	public static MazeDirection GetOpposite (this MazeDirection direction) {
		return opposites[(int)direction];
	}

	public static MazeDirection GetNextClockwise (this MazeDirection direction) {
		return (MazeDirection)(((int)direction + 1) % Count);
	}

	public static MazeDirection GetNextCounterclockwise (this MazeDirection direction) {
		return (MazeDirection)(((int)direction + Count - 1) % Count);
	}
	
	private static IntVector2[] vector2s = {
		new IntVector2(0, 1),
		new IntVector2(1, 0),
		new IntVector2(0, -1),
		new IntVector2(-1, 0)
	};

	private static Vector3[] vector3s = {
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f)
	};

	public static IntVector2 ToIntVector2 (this MazeDirection direction) {
		return vector2s[(int)direction];
	}

	public static Vector3 ToVector3 (this MazeDirection direction) {
		return vector3s[(int)direction];
	}

	private static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

	public static Quaternion ToRotation (this MazeDirection direction) {
		return rotations[(int)direction];
	}
}