using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public abstract class Entity : MonoBehaviour
{
	private int _unitID;
	[SerializeField]
	private string name;
	[SerializeField]
	private float life;
	[SerializeField]
	private Team team;

	public GameObject _selectUI;
	public UIManager _uiManager;

	protected void Start()
	{
		_uiManager = GameObject.Find ("UIManager").GetComponent<UIManager>();
	}

	public void ShowStat()
	{
		Debug.Log (Name + " a actuellement " + Life + " hp");
	}

	public void TakeDommage(float dommage)
	{
		Life -= dommage;
		if(_uiManager._statShowActive)
		{
			_uiManager.UpdateStatUnit ();
		}
		if(Life <= 0)
		{
			Death();
		}
	}


	public virtual void IsSelected ()
	{
		_selectUI.GetComponent<SpriteRenderer> ().enabled = true;
		_uiManager.ShowStatUnit (name, Life.ToString(), this);
	}

	public virtual void IsDeselected ()
	{
		_selectUI.GetComponent<SpriteRenderer> ().enabled = false;
		_uiManager.HideStatUnit ();
	}

	public void Death ()
	{
		Destroy (this.gameObject);
	}

	public string Name { get => name; set => name = value; }
	public float Life { get => life; set => life = value; }
	public Team Team { get => team; set => team = value; }
	public int UnitID { get => _unitID; set => _unitID = value; }
}
