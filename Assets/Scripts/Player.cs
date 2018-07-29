using UnityEngine;
using System.Collections;


public class Player : MonoBehaviour
{   // This class encapsulates the objects controlled by the player. It is assumed
	// that each instance of this class has a child node for an animated model. The 
	// animated model is identified by the public modelName attribute and accessed by
	// instances of this class that search for a child with that name. It is also
	// assumed that each instance has a rigid body attached for detecting collisions.

	private const float runSpeedMultiplier = 2.0f;
	private const float smallDelta = 0.01f;
	private const float mediumDelta = 0.1f;

	private GameObject model;               // The model that represents player
	private Actions actions;                // Animation actions controller
	private MazeDirection lookDirection;    // Direction player should face if not moving
	private Vector3 targetPosition;         // Location of the next maze cell player is headed toward 
	private bool isRunning = false;         // True iff player is running
	private Rigidbody player_rigidbody;     // Rigid body used for collision detection

	// List of maze cells player will is scheduled visit (The current route to travel)
	private ArrayList route = new ArrayList (); 

	// Configurable attributes of player
	public float turnSmoothing = 15f;
	public float dampingTime = 0.1f;
	public string modelName = "SportyGirl";

	public MazeCell GetCurrentCell()
	{   // Returns the cell the player is currently standing upon. It may be null
		// If the player has not entered the game yet.
		if(0 < route.Count) {
			return (MazeCell)route[0];
		}
		return null;
	}

	public MazeCell GetDestinationCell()
	{   // Returns the last cell in the players current route. This is
		// the cell the player will end up standing upon if no other
		// cells are added to the route. It may be null if the player has
		// not entered the game yet.
		if(0 < route.Count) {
			return (MazeCell)route[route.Count - 1];
		}
		return null;
	}

	public void TeleportToCell (MazeCell cell)
	{   // Unconditionaly move player to stand upon cell (teleport)
		// This method clears any route the player may have had before this
		// method was called.
		if (GetCurrentCell() != null) {
			GetCurrentCell().OnPlayerExited ();
		}
		route.Clear(); // Clear any previously planned route
		route.Add(cell);
		transform.position = cell.transform.position;
		targetPosition = transform.position;
		GetCurrentCell().OnPlayerEntered ();
	}

	public void MoveToCell (MazeCell cell)
	{   // Add cell to route player will follow. This will cause the player
		// to eventually reach cell
		route.Add (cell);
	}

	public void MoveDirection (MazeDirection direction)
	{   // If it is possible for the player to move the specified direction
		// from the last cell in player's route, the cell in that direction from
		// the last cell in player's route is added to the route.
		MazeCell cellToMoveFrom = GetDestinationCell();

		MazeCellEdge edge = cellToMoveFrom.GetEdge (direction);
		//Debug.Log(string.Format("Move:{0} from {1} to {2}", direction, cellToMoveFrom.name, edge.otherCell.name));
		if (edge is MazePassage) {
			if (cellToMoveFrom == edge.otherCell) {
				MoveToCell (edge.cell);
			} else if (cellToMoveFrom == edge.cell) {
				MoveToCell (edge.otherCell);
			} else {
				Debug.LogError (string.Format ("Move:{0} from {1} to {2}", direction, cellToMoveFrom.name, edge.otherCell.name));
			}
		}
	}

	public void LookDirection (MazeDirection direction)
	{   // Set the direction the player model should face when not moving. The player
		// model always faces the direction of motion when moving.
		lookDirection = direction;
	}

	void Awake ()
	{ 
		model = transform.Find (modelName).gameObject;
		actions = model.GetComponent<Actions> ();
		targetPosition = transform.position;
		player_rigidbody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate ()
	{
		if (1 < route.Count) {
			// Player has not yet completed route. There is at least one more cell
			// to visit in the route.
			float deltaToNextCell = Mathf.Max (
				                        Mathf.Abs (targetPosition.x - transform.position.x),
				                        Mathf.Abs (targetPosition.z - transform.position.z));
			if (0.4f > deltaToNextCell) {
				// The player is close to the immediate destination so go ahead and process
				// the transition from the current cell to the next cell.
				if (GetCurrentCell() != null) {
					GetCurrentCell().OnPlayerExited ();
				}
				route.RemoveAt (0);
				GetCurrentCell().OnPlayerEntered ();
				targetPosition = GetCurrentCell().transform.position;

//					Debug.LogWarning (string.Format(
//						"Move To:{0} {1}", 
//						GetCurrentCell().name, targetPosition.y));
			}
		}

		float delta = Mathf.Max (Mathf.Abs (targetPosition.x - transform.position.x),
			              Mathf.Abs (targetPosition.z - transform.position.z));
		if (delta > smallDelta) {
			// Calculate direction of travel
			Vector3 directionToTargetPosition = 
				targetPosition - transform.position;

			// Face direction of travel
			Quaternion targetRotation = Quaternion.LookRotation (
				directionToTargetPosition, Vector3.up);
			Quaternion newRotation = Quaternion.Lerp (player_rigidbody.rotation,
				                         targetRotation, turnSmoothing * Time.deltaTime);
			player_rigidbody.MoveRotation (newRotation);

			if (2 < route.Count) {   // The planned destination is far so run there
				const string message = "Run";
				actions.SendMessage (message, SendMessageOptions.DontRequireReceiver);
				isRunning = true;
			} else {   // The planned destination is very near so slow down
				if (1 == route.Count && delta > 0.2f) {
					const string message = "Walk";
					actions.SendMessage (message, SendMessageOptions.DontRequireReceiver);
					isRunning = false;
				}
			}

			if (isRunning) {
				player_rigidbody.MovePosition (transform.position +
				transform.forward * Time.deltaTime * runSpeedMultiplier);
			} else {
				player_rigidbody.MovePosition (
					transform.position + transform.forward * Time.deltaTime);
			}
		} else {
			const string message = "Stay";
			actions.SendMessage (message, SendMessageOptions.DontRequireReceiver);
			float angle = Quaternion.Angle (transform.rotation, lookDirection.ToRotation ());
			if (Mathf.Abs (angle) > mediumDelta) {
				Rigidbody player_rigidbody = GetComponent<Rigidbody> ();
				Quaternion newRotation = Quaternion.Lerp (player_rigidbody.rotation,
					lookDirection.ToRotation (), turnSmoothing * Time.deltaTime);
				player_rigidbody.MoveRotation (newRotation);
			}
		}
	}

	private void Update ()
	{
		if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.UpArrow)) {
			MoveDirection (lookDirection);
			LookDirection (lookDirection);
		} else if (Input.GetKeyDown (KeyCode.D) || Input.GetKeyDown (KeyCode.RightArrow)) {
			MazeDirection direction = lookDirection.GetNextClockwise ();
			MoveDirection (direction);
			LookDirection (direction);
		} else if (Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.DownArrow)) {
			MazeDirection direction = lookDirection.GetOpposite ();
			MoveDirection (direction);
			LookDirection (direction);
		} else if (Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.LeftArrow)) {
			MazeDirection direction = lookDirection.GetNextCounterclockwise ();
			MoveDirection (direction);
			LookDirection (direction);
		} else if (Input.GetKeyDown (KeyCode.Q)) {
			LookDirection (lookDirection.GetNextCounterclockwise ());
		} else if (Input.GetKeyDown (KeyCode.E)) {
			LookDirection (lookDirection.GetNextClockwise ());
		}
	}
}