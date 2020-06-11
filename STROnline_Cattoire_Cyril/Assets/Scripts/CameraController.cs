using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Terrain terrain;

	public float panSpeed;
	public float terrainLimit;
	public float zoomMin;
	public float zoomMax;
	public float zoomSpeed;
	//public float rotationSpeed;
	//public float rotationAmount;

	private float _terrainSizeX;
	private float _terrainSizeZ;
	private float _panDetect = 15f;
	//private float _minHeight = 10f;
	//private float _maxHeight = 100f;

	//private Quaternion rotation;

    // Start is called before the first frame update
    void Start()
    {
		_terrainSizeX = terrain.terrainData.size.x;
		_terrainSizeZ = terrain.terrainData.size.z - terrainLimit;
		//rotation = Camera.main.transform.rotation;
	}

    // Update is called once per frame
    void Update()
    {
		MoveCamera ();
    }

	void MoveCamera ()
	{
		float moveX = Camera.main.transform.position.x;
		float moveY = Camera.main.transform.position.y;
		float moveZ = Camera.main.transform.position.z;

		float xPos = Input.mousePosition.x;
		float yPos = Input.mousePosition.y;

		if (Input.GetKey (KeyCode.Q) || xPos > 0 && xPos < _panDetect)
		{
			moveX -= panSpeed;
			if(moveX < 0)
			{
				moveX += panSpeed;
			}
		}
		else if (Input.GetKey (KeyCode.D)|| xPos < Screen.width && xPos > Screen.width - _panDetect)
		{
			moveX += panSpeed;
			if (moveX > _terrainSizeX)
			{
				moveX -= panSpeed;
			}
		}

		if (Input.GetKey (KeyCode.S)|| yPos > 0 && yPos < _panDetect)
		{
			moveZ -= panSpeed;
			if (moveZ < 0)
			{
				moveZ += panSpeed;
			}
		}
		else if (Input.GetKey (KeyCode.Z)|| yPos < Screen.height && yPos > Screen.height - _panDetect)
		{
			moveZ += panSpeed;
			if (moveZ > _terrainSizeZ)
			{
				moveZ -= panSpeed;
			}
		}

		if (Input.GetAxis ("Mouse ScrollWheel") < 0 || Input.GetKey (KeyCode.A))
		{
			moveY += zoomSpeed;
			if(Camera.main.transform.position.y > zoomMax)
			{
				moveY -= zoomSpeed;
			}
		}
		else if (Input.GetAxis ("Mouse ScrollWheel") > 0 || Input.GetKey (KeyCode.E))
		{
			moveY -= zoomSpeed;
			if (Camera.main.transform.position.y < zoomMin)
			{
				moveY += zoomSpeed;
			}
		}

		Vector3 newPos = new Vector3 (moveX, moveY, moveZ);
		Camera.main.transform.position = newPos;
	}

	//void RotateCamera()
	//{

	//}
}
