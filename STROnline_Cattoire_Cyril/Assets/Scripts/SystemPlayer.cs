using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SystemPlayer : MonoBehaviour
{

	public static SystemPlayer instance;

	public RectTransform _rect;
	[SerializeField]
	private GameObject _building;

	[SerializeField]
	private List<GameObject> _currentEntity = new List<GameObject> ();
	private Player _player;
	private Vector2 _startDragPos;


	public UnitManager _unitManager;
	public GameObject selectedEntity;
	public bool mouseDragging;

	public void CreateBuild(int buildId, Vector3 buildPos, int buildTeam)
	{
		GameObject build = Instantiate (_building, buildPos, Quaternion.identity) as GameObject;
		Entity buildEntity = build.GetComponent<Entity> ();
		buildEntity.Team = (Team)buildTeam;
		buildEntity.UnitID = buildId;
	}

	public enum SelectFSM
	{
		clickOrDrag,
		clickSelect,
		clickDeselect
	}
	public SelectFSM selectFSM;


	void Update ()
	{
		SelectUnitsFSM ();
	}

	void SelectUnitsFSM ()
	{
		switch (selectFSM)
		{
			case SelectFSM.clickOrDrag:
				ClickOrDrag ();
				break;
			case SelectFSM.clickSelect:
				SelectSingleUnit ();
				break;
			case SelectFSM.clickDeselect:
				DeselectAll ();
				break;
		}
	}

	Player GetPlayer()
	{
		if(_player == null)
			_player = Network.GetInstance ().LocalPlayer;

		return _player;
	}

	void ClickOrDrag ()
	{
		if (!mouseDragging)
			_rect.gameObject.SetActive (false);

		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (EventSystem.current.currentSelectedGameObject != null)
			return;


		if (Physics.Raycast (ray, out hit, Mathf.Infinity))
		{
			if (Input.GetMouseButtonDown (0) && !Input.GetKey (KeyCode.LeftControl))
			{
				var _endDragPos = hit.point;
				_startDragPos = Input.mousePosition;
				if (hit.collider.gameObject.tag == "Entity" && hit.collider.GetComponent<Entity> ().Team == GetPlayer().Team)
				{
					selectedEntity = hit.collider.gameObject;
					selectFSM = SelectFSM.clickSelect;
				}
				else if (hit.collider.gameObject.tag == "UI")
				{
				}
				else if (hit.collider.gameObject.tag == "Terrain")
				{
					selectFSM = SelectFSM.clickDeselect;
				}
			}
			else if (Input.GetMouseButtonDown (0) && Input.GetKey (KeyCode.LeftControl) && hit.collider.gameObject.tag != "UI")
			{
				if (hit.collider.gameObject.tag == "Entity" && !_currentEntity.Contains (hit.collider.gameObject) && hit.collider.GetComponent<Tank> ()?.Team == GetPlayer ().Team)
					AddToCurrentlySelectedUnits (hit.collider.gameObject);
				else if (hit.collider.gameObject.tag == "Entity" && _currentEntity.Contains (hit.collider.gameObject) && hit.collider.GetComponent<Tank> ()?.Team == GetPlayer ().Team)
					RemoveFromCurrentlySelectedUnits (hit.collider.gameObject);
			}
			else if (Input.GetMouseButton (0) && !Input.GetKey (KeyCode.LeftControl))
			{
				if (UserDraggingByPosition (_startDragPos, Input.mousePosition))
				{
					mouseDragging = true;
					DrawDragBox ();
					SelectUnitsInDrag ();
				}
			}
			else if (Input.GetMouseButtonUp (0) && !Input.GetKey (KeyCode.LeftControl))
			{
				mouseDragging = false;
			}

			if (Input.GetMouseButtonDown (1) && _currentEntity.Count > 0)
			{
				if (hit.collider.gameObject.tag == "Entity" && hit.collider.GetComponent<Entity> ().Team != GetPlayer ().Team)
				{
					for (int i = 0; i < _currentEntity.Count; i++)
					{
						Tank _currentTank = _currentEntity[i].GetComponent<Tank> ();
						_currentTank.SetEnnemy (hit.collider.GetComponent<Entity> ());
						_currentTank._state = EntityStates.Attack;
					}
				}
				else if (hit.collider.gameObject.tag == "Terrain")
				{
					for (int i = 0; i < _currentEntity.Count; i++)
					{
						Tank _currentTank = _currentEntity[i].GetComponent<Tank> ();
						Network.GetInstance ().EntityMove (_currentTank, hit.point, EntityStates.Move);
					}
				}
			}
		}
	}

	private void SelectSingleUnit ()
	{
		if (selectedEntity != null)
		{
			if (_currentEntity.Count > 0)
			{
				for (int i = 0; i < _currentEntity.Count; i++)
				{
					_currentEntity[i].GetComponent<Entity> ().IsDeselected ();
					_currentEntity.Remove (_currentEntity[i]);
				}
			}
			else if (_currentEntity.Count == 0)
			{
				AddToCurrentlySelectedUnits (selectedEntity);
				selectFSM = SelectFSM.clickOrDrag;
			}
		}
		else
		{
			Debug.Log ("Whaaat?");
		}
	}

	void DrawDragBox ()
	{
		if (!_rect.gameObject.activeInHierarchy)
		{
			_rect.gameObject.SetActive (true);
		}

		var _endDragPos = Input.mousePosition;

		float sizeX = _endDragPos.x - _startDragPos.x;
		float sizeY = _endDragPos.y - _startDragPos.y;

		_rect.sizeDelta = new Vector2 (Mathf.Abs (sizeX), Mathf.Abs (sizeY));
		_rect.anchoredPosition = _startDragPos + new Vector2 (sizeX / 2, sizeY / 2);
	}

	private bool UserDraggingByPosition (Vector2 dragStartPoint, Vector2 newPoint)
	{
		if ((newPoint.x > dragStartPoint.x || newPoint.x < dragStartPoint.x) || (newPoint.y > dragStartPoint.y || newPoint.y < dragStartPoint.y))
		return true;
		else
			return false;
	}

	private void SelectUnitsInDrag ()
	{
		for (int i = 0; i < _unitManager.units.Count; i++)
		{
			if (_unitManager.units[i].GetComponent<Tank> ().renderer.isVisible && _unitManager.units[i].GetComponent<Tank> ()?.Team == GetPlayer ().Team)
			{
				Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint (_unitManager.units[i].transform.position);

				if (unitScreenPosition.x < _rect.offsetMax.x && unitScreenPosition.y > _rect.offsetMin.y && unitScreenPosition.x > _rect.offsetMin.x && unitScreenPosition.y < _rect.offsetMax.y)
				{
					AddToCurrentlySelectedUnits (_unitManager.units[i]);
				}
				else
				{
					RemoveFromCurrentlySelectedUnits (_unitManager.units[i]);
				}
			}
		}
	}

	private void AddToCurrentlySelectedUnits (GameObject unitToAdd)
	{
		if (!_currentEntity.Contains (unitToAdd))
		{
			_currentEntity.Add (unitToAdd);
			unitToAdd.GetComponent<Entity> ().IsSelected ();
		}
	}

	private void RemoveFromCurrentlySelectedUnits (GameObject unitToRemove)
	{
		if (_currentEntity.Count > 0)
		{
			unitToRemove.GetComponent<Entity> ().IsDeselected();
			_currentEntity.Remove (unitToRemove);
		}
	}

	private void DeselectAll ()
	{
		if (_currentEntity.Count > 0)
		{
			for (int i = 0; i < _currentEntity.Count; i++)
			{
				_currentEntity[i].GetComponent<Entity> ().IsDeselected ();
				_currentEntity.Remove (_currentEntity[i]);
			}
		}
		else if (_currentEntity.Count == 0)
		{
			selectFSM = SelectFSM.clickOrDrag;
		}
	}
}
