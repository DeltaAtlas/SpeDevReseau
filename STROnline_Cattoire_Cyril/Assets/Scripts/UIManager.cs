using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    public GameObject _backgroundStat;
    public GameObject _mainMenu;
    public Text _nameUI;
    public Text _countPlayer;
    public Text _lifeUI;
    public GameObject _buttonUI;
    public bool _statShowActive;
    public Entity _entity;

    void Start()
	{
        Network.GetInstance ().PlayedCountChange += i => ChangeCountPlayer (i);

        Network.GetInstance ().StartGame += () => StartRealyGame ();
    }

    void StartRealyGame()
	{
        _mainMenu.SetActive (false);
    }

    void ChangeCountPlayer(int count)
	{
        _countPlayer.text = count.ToString() + " / 2";

    }

    public void ShowStatUnit(string name, string life, Entity entitySelected)
	{
        _entity = entitySelected;
        _backgroundStat.SetActive (true);
        _nameUI.text = "Name : " + name;
        _lifeUI.text = "Life : " + life;
        _statShowActive = true;
    }

    public void UpdateStatUnit()
	{
        _lifeUI.text = "Life : " + _entity.Life;
    }

    public void ShowButtonBuild ()
    {
        _buttonUI.SetActive (true);
    }

    public void HideStatUnit ()
    {
        _backgroundStat.SetActive (false);
        _statShowActive = false;
    }

    public void HideButtonBuild ()
    {
        _buttonUI.SetActive (false);
    }
}
