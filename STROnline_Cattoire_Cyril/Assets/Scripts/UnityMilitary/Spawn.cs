using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject APCPrefabRed;
    public GameObject APCPrefabBlue;
    public GameObject APCPrefabGreen;

    public UnitManager _unitManager;

    [SerializeField]
    private Team _team;


	// Start is called before the first frame update
	void Start ()
	{
		GameObject tank = Instantiate (APCPrefabRed, new Vector3 (500, 0, 500), Quaternion.identity) as GameObject;

        GameObject tank01 = Instantiate (APCPrefabBlue, new Vector3 (550, 0, 550), Quaternion.identity) as GameObject;

        _unitManager.units.Add (0,tank);
        tank.GetComponent<Tank>().UnitID = 0;
        _unitManager.units.Add (1,tank01);
        tank.GetComponent<Tank> ().UnitID = 1;
        Debug.Log (_unitManager.units.Count);
    }

}
