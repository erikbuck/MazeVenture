using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour
{

	public IntVector2 size;

	public MazeCell cellPrefab;

	public MazeCell cellTreePrefab;

	public float generationStepDelay;

	public MazePassage passagePrefab;
	public MazeStairs stairsPrefab;
	public MazeDoor doorPrefab;

	public GameObject columnPrefab;
	public GameObject railingColumnPrefab;

	[Range (0f, 1f)]
	public float doorProbability;

	public MazeWall[] railingPrefabs;
	public MazeWall[] wallPrefabs;

	private MazeCell[,] cells;

	public IntVector2 RandomCoordinates {
		get {
			return new IntVector2 (Random.Range (0, size.x), Random.Range (0, size.z));
		}
	}

	public IntVector2 CenterCoordinates {
		get {
			return new IntVector2 (size.x / 2, size.z / 2);
		}
	}

	public bool ContainsCoordinates (IntVector2 coordinate)
	{
		return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
	}

	public MazeCell GetCell (IntVector2 coordinates)
	{
		return cells [coordinates.x, coordinates.z];
	}

	public IEnumerator Generate ()
	{
		WaitForSeconds delay = new WaitForSeconds (generationStepDelay);
		cells = new MazeCell[size.x, size.z];
		List<MazeCell> activeCells = new List<MazeCell> ();
		DoFirstGenerationStep (activeCells);

		while (activeCells.Count > 0) {
			if (0 < generationStepDelay) {
				yield return delay;
			}
			DoNextGenerationStep (activeCells);
		}
	}

	private void DoFirstGenerationStep (List<MazeCell> activeCells)
	{
		MazeCell newCell = CreateCell (RandomCoordinates, 0, cellPrefab);
		newCell.Initialize (0);
		activeCells.Add (newCell);
	}

	private void DoNextGenerationStep (
		List<MazeCell> activeCells)
	{
		int currentIndex = activeCells.Count - 1;
		Debug.Assert(currentIndex >= 0);

		MazeCell currentCell = activeCells [currentIndex];
		if (currentCell.IsFullyInitialized) {
			activeCells.RemoveAt (currentIndex);
			return;
		}

		MazeDirection direction = currentCell.RandomUninitializedDirection;
		IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2 ();
		if (ContainsCoordinates (coordinates)) {
			MazeCell neighbor = GetCell (coordinates);
			if (neighbor == null) {
				if (null != cellTreePrefab && 0 == Random.Range (0, 77)) {
					neighbor = CreateCell (coordinates, 0, cellTreePrefab);
					activeCells.Add (neighbor);
				} else {
					neighbor = CreateCell (coordinates, 0, cellPrefab);
					activeCells.Add (neighbor);
					CreatePassage (activeCells, currentCell, neighbor, direction);
				}
			} else if (currentCell.GetRoomNumber () == neighbor.GetRoomNumber () &&
			    doorProbability > 0 && 
				currentCell.altitude == neighbor.altitude) {
				CreatePassageInSameRoom (currentCell, neighbor, direction);
			} else {
				CreateWall (currentCell, neighbor, direction);
			}
		} else {
			CreateWall (currentCell, null, direction);
		}
	}

	private MazeCell CreateCell (
		IntVector2 coordinates,
		int anAltitude,
		MazeCell aCellPrefab)
	{
		MazeCell newCell = Instantiate (aCellPrefab) as MazeCell;
		cells [coordinates.x, coordinates.z] = newCell;
		newCell.coordinates = coordinates;
		newCell.altitude = anAltitude;
		newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
		newCell.transform.parent = transform;
		newCell.transform.localPosition = new Vector3 (
			coordinates.x - size.x * 0.5f + 0.5f, 
			newCell.altitude * 1.078f, 
			coordinates.z - size.z * 0.5f + 0.5f);

		return newCell;
	}

	private void CreatePassageInSameRoom (
		MazeCell cell, 
		MazeCell otherCell, 
		MazeDirection direction)
	{
		MazePassage prefab = passagePrefab;

		MazePassage passage = Instantiate (prefab) as MazePassage;
		passage.Initialize (cell, otherCell, direction);
	}

	private bool CanCreateStairs(
		MazeCell cell, 
		MazeCell otherCell, 
		MazeDirection direction)
	{   // To create stairs, the strairs must have a altitude 0 low landing cell in the same 
		// room as the stairs, and there must not be an existing cell at the location of the 
		// future high landing cell.
		bool result = false;

		if( null != stairsPrefab && 0 == cell.altitude) {
			IntVector2 lowLandingCoordinates = cell.coordinates + 
				direction.GetOpposite().ToIntVector2 ();
			IntVector2 highLandingCoordinates = otherCell.coordinates + 
				direction.ToIntVector2 ();
			
			if(ContainsCoordinates (lowLandingCoordinates) &&
				ContainsCoordinates(highLandingCoordinates)) {

				MazeCell lowLandingCell = GetCell (lowLandingCoordinates);
				if(null != lowLandingCell && 
					cell.GetRoomNumber() == lowLandingCell.GetRoomNumber() &&
					null == GetCell (highLandingCoordinates)) {
						result = true;
				}
			}
		}
		return result;
	}


	private void CreatePassage (
		List<MazeCell> activeCells,
		MazeCell cell, 
		MazeCell otherCell, 
		MazeDirection direction)
	{   // Create one of the possible passable edges from one a cell in one
		// room to a cell in another.
		MazePassage prefab = passagePrefab; // generic passage

		if (cell.altitude == 0 && 
			Random.value < doorProbability) {
			// Possibly make a door from one room to the next
			prefab = doorPrefab;

			if (Random.value < doorProbability &&
								CanCreateStairs(cell, otherCell, direction) ) {
				// Instead of a door, let's try to make stairs. Stiars occupy
				// both cell and other cell and enforce constraints on
				// a low landing cell at the base of the stairs and a
				// high landing cell at the top of the stairs.
				prefab = passagePrefab; // Not a door after all!

				cell.accessory = Instantiate (stairsPrefab) as MazeCellAccessory;
				cell.accessory.cell = cell;
				cell.accessory.transform.parent = cell.transform;
				cell.accessory.transform.localPosition = Vector3.zero;
				cell.accessory.transform.localRotation = direction.ToRotation();

				otherCell.Initialize (cell.GetRoomNumber ());  

				IntVector2 highLandingCoordinates = otherCell.coordinates + 
					direction.ToIntVector2 ();
				MazeCell highLandingCell = CreateCell (
					highLandingCoordinates, 1, cellPrefab);
				activeCells.Add (highLandingCell);
				highLandingCell.Initialize (otherCell.GetRoomNumber () + 1);

				MazePassage landingPassage = Instantiate (prefab) as MazePassage;
				landingPassage.Initialize (otherCell, highLandingCell, direction);

				GameObject newColumn = Instantiate (columnPrefab) as GameObject;
				newColumn.transform.parent = otherCell.transform;
				newColumn.transform.localPosition = Vector3.zero;
				newColumn.transform.localRotation = direction.ToRotation ();

				if(null != railingColumnPrefab) {
				    GameObject newRailingColumn = Instantiate (railingColumnPrefab) as GameObject;
					newRailingColumn.transform.parent = highLandingCell.transform;
					newRailingColumn.transform.localPosition = Vector3.zero;
					newRailingColumn.transform.localRotation = direction.ToRotation ();
				}

			} else {
				// Doors alsways change room numbers
				otherCell.Initialize (cell.GetRoomNumber () + 1);
			}

		} else {
			// This passage is neither door nor stair
			otherCell.Initialize (cell.GetRoomNumber ());
			otherCell.altitude = cell.altitude;
			otherCell.transform.localPosition = new Vector3(otherCell.transform.localPosition.x,
				otherCell.transform.localPosition.y + otherCell.altitude * 1.078f,
				otherCell.transform.localPosition.z);
		} 

		MazePassage passage = Instantiate (prefab) as MazePassage;
		passage.Initialize (cell, otherCell, direction);
	}

	private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction)
	{
		MazeWall wall = Instantiate (wallPrefabs [Random.Range (0, wallPrefabs.Length)]) as MazeWall;
		wall.Initialize (cell, otherCell, direction);
		GameObject newColumn = Instantiate (columnPrefab) as GameObject;
		newColumn.transform.parent = cell.transform;
		newColumn.transform.localPosition = Vector3.zero;
		newColumn.transform.localRotation = direction.ToRotation ();

		if(cell.altitude > 0) {
			newColumn.transform.localPosition = new Vector3(
				newColumn.transform.localPosition.x,
				//-cell.transform.localPosition.y,
				-cell.altitude * 1.078f,
				newColumn.transform.localPosition.z);
			wall.transform.localPosition = new Vector3(
				wall.transform.localPosition.x,
				//-cell.transform.localPosition.y,
				-cell.altitude * 1.078f,
				wall.transform.localPosition.z);

			MazeWall railing = Instantiate (
				railingPrefabs [Random.Range (0, railingPrefabs.Length)]) as MazeWall;
			railing.Initialize (cell, otherCell, direction);
		}
	}

}