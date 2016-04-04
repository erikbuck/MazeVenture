using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeDoor : MazePassage
{
	
	public Transform hinge;

	public float doorAngleStepDelay;

	private float 
		targetAngle = 0, 
		currentAngle = 0;

	public IEnumerator UpdateDoorAngle ()
	{
		if (null != hinge) {
			WaitForSeconds delay = new WaitForSeconds (doorAngleStepDelay);

			while (0.1 < Mathf.Abs (targetAngle - currentAngle)) {
				currentAngle += (targetAngle - currentAngle) * 0.35f;
				//Debug.Log(string.Format("currentAngle:{0}", localCurrentAngle));
			
				hinge.localRotation = 
				Quaternion.Euler (0f, currentAngle, 0f);

				if (0 < doorAngleStepDelay) {
					yield return delay;
				}
			}

			currentAngle = targetAngle;
		}
	}

	public override void Initialize (
		MazeCell primary, 
		MazeCell other, 
		MazeDirection direction)
	{
		base.Initialize (primary, other, direction);

		//Debug.Log(string.Format("MazeDoor.Initialize: from {0} to {1}", primary.name, otherCell.name));

//		for (int i = 0; i < transform.childCount; i++) {
//			Transform child = transform.GetChild(i);
//			if (child != hinge) {
//				//child.GetComponent<Renderer>().material = cell.room.settings.wallMaterial;
//			}
//		}
	}

	public override bool IsPassable ()
	{
		return (currentAngle > 70);
	}

	public override bool IsObstacle ()
	{
		return true;
	}

	public override void OnPlayerEntered ()
	{
		StopCoroutine ("UpdateDoorAngle");
		targetAngle = 90f;
		StartCoroutine ("UpdateDoorAngle");
	}

	public override void OnPlayerExited ()
	{
		StopCoroutine ("UpdateDoorAngle");
		targetAngle = 0f;
		StartCoroutine ("UpdateDoorAngle");
	}
}