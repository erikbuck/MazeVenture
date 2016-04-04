using UnityEngine;

public class MazeCell : MonoBehaviour {

	public IntVector2 coordinates;
	public int altitude = 0;
	public MazeCellAccessory accessory = null;

	private MazeCellEdge[] edges = new MazeCellEdge[MazeDirections.Count];

	private int roomNumber;
	
	private int initializedEdgeCount;

	public bool IsFullyInitialized {
		get {
			return initializedEdgeCount == MazeDirections.Count;
		}
	}

	public int GetObstacleEdgeCount() {
		int result = 0;
		for(int i = 0; i < MazeDirections.Count; ++i) {
			if(edges[i].IsObstacle()) {
				result += 1;
			}
		}
		if(null != accessory && accessory.IsObstacle()) {
			result += 1;
		}

		return result;
	}

	public MazeDirection RandomUninitializedDirection {
		get {
			int skips = Random.Range(0, MazeDirections.Count - initializedEdgeCount);
			for (int i = 0; i < MazeDirections.Count; i++) {
				if (edges[i] == null) {
					if (skips == 0) {
						return (MazeDirection)i;
					}
					skips -= 1;
				}
			}
			throw new System.InvalidOperationException("MazeCell has no uninitialized directions left.");
		}
	}

	public int GetRoomNumber() {
		return roomNumber;
	}

	public void Initialize (int aRoomNumber) {
		roomNumber = aRoomNumber;
	}

	public MazeCellEdge GetEdge (MazeDirection direction) {
		return edges[(int)direction];
	}

	public void SetEdge (MazeDirection direction, MazeCellEdge edge) {
		if(null == edges[(int)direction]) {
			initializedEdgeCount += 1;
		}
		edges[(int)direction] = edge;
	}

	public void Show () {
		gameObject.SetActive(true);
		//if(null != accessory) {
		//	accessory.SetActive(true);
		//}
	}

	public void Hide () {
		//gameObject.SetActive(false);
	}

	public void OnPlayerEntered () {
		//room.Show();
		for (int i = 0; i < edges.Length; i++) {
			edges[i].OnPlayerEntered();
		}
		if(null != accessory) {
			accessory.OnPlayerEntered();
		}
	}
	
	public void OnPlayerExited () {
		//room.Hide();
		for (int i = 0; i < edges.Length; i++) {
			edges[i].OnPlayerExited();
		}
		if(null != accessory) {
			accessory.OnPlayerExited();
		}
	}
}