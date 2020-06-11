using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Entity
{


	public override void IsSelected ()
	{
		base.IsSelected ();
		_uiManager.ShowButtonBuild ();
	}

	public override void IsDeselected ()
	{
		base.IsDeselected ();
		_uiManager.HideButtonBuild ();
	}
}
