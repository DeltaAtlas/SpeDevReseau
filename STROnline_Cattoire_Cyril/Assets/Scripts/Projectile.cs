using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float power;
    public Team _team;

    void OnTriggerEnter (Collider other)
    {
        if(other.tag == "Entity" && other.GetComponent<Entity>().Team != _team)
		{
            other.GetComponent<Entity> ().TakeDommage (power);
		}
    }
}
