using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class Player 
{
    private Team _team; 
    
    private string _name;

    public Team Team { get => _team; set => _team = value; }
    public string Name { get => _name; set => _name = value; }

}
