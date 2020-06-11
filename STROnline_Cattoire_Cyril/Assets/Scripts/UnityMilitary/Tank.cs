using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tank : Entity
{
    
    public GameObject _projectile;
    public GameObject _canon;
    public GameObject _canonTarget;
    public GameObject _rocketSpawn;
    [SerializeField]
    private int reloadMax;
    [SerializeField]
    private int checkTimeMax;
    [SerializeField]
    private float _radiusCheckEnnemy;
    [SerializeField]
    private LayerMask _entity;
    [SerializeField]
    private float _angularSpeed;
    [SerializeField]
    private float _power;
    [SerializeField]
    private float _distance;
    [SerializeField]
    private float _range;
    public EntityStates _state;

    [SerializeField]
    private Entity _currentEnnemy;

    private Vector3 _currentDestination;
    private float _distanceMinEnnemy;
    private int reload;
    private int checkTime;
    private NavMeshAgent _navMeshAgent;

    public Renderer renderer;

    void Start()
	{
        base.Start ();
        _navMeshAgent = GetComponent<NavMeshAgent> ();

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
		{
            ShowStat ();
		}
        if (Input.GetKeyDown (KeyCode.G))
        {
            TakeDommage (10f);
        }
        if (Input.GetKeyDown (KeyCode.Space))
        {
            Shoot ();
        }

        switch (_state)
        {
            case EntityStates.Neutral:
                Neutral ();
                break;

            case EntityStates.Attack:
                Attack ();
                break;

            case EntityStates.Move:
                MoveTo ();
                break;

        }
    }

    void Neutral ()
    {
        if (checkTime > 0)
        {
            checkTime--;
        }
        else
        {
            CheckEnnemy ();
            checkTime = checkTimeMax;
        }
    }

    void CheckEnnemy()
	{
        _distanceMinEnnemy = _radiusCheckEnnemy;
        Collider[] hitColliders = Physics.OverlapSphere (transform.position, _radiusCheckEnnemy, _entity);
        for(int i = 0; i < hitColliders.Length; i++)
		{
            if (hitColliders[i].GetComponent<Entity> ().Team != Team)
            {
                CheckEnnemyDistance (hitColliders[i].gameObject.transform);
            }
        }

        if (_distanceMinEnnemy != _radiusCheckEnnemy)
        {
            _state = EntityStates.Attack;
        }
    }

    void CheckEnnemyDistance(Transform ennemy)
	{
        float currentDistance = Vector3.Distance (ennemy.position, transform.position);
        if (currentDistance < _distanceMinEnnemy)
		{
            SetEnnemy (ennemy.gameObject.GetComponent<Entity> ());
            _distanceMinEnnemy = currentDistance;
        }
	}


    public void SetEnnemy(Entity entity)
	{
        _currentEnnemy = entity;
	}

    public void Attack ()
    {
        if (_currentEnnemy != null)
        {
            if (Vector3.Distance (_currentEnnemy.transform.position, transform.position) > _distance)
            {
                _navMeshAgent.SetDestination (_currentEnnemy.transform.position);
            }
            else
            {
                _navMeshAgent.SetDestination (transform.position);
            }

            if (Vector3.Distance (_currentEnnemy.transform.position, transform.position) < _range)
            {
                _canonTarget.transform.LookAt (new Vector3 (_currentEnnemy.transform.position.x, _canonTarget.transform.position.y, _currentEnnemy.transform.position.z));
                Quaternion targetRotation = Quaternion.Slerp (_canon.transform.rotation, _canonTarget.transform.rotation, Time.deltaTime * _angularSpeed);
                _canon.transform.rotation = targetRotation;
                Debug.DrawRay (_canon.transform.position, _canon.transform.forward * 200, Color.red, 1f);
                RaycastHit hit;
                if (Physics.Raycast (_canon.transform.position, _canon.transform.forward, out hit, _range, _entity))
                {
                    if (hit.collider.GetComponent<Entity> ().Team != Team)
                    {
                        if (reload > 0)
                        {
                            reload--;
                        }
                        else
                        {
                            Shoot ();
                            reload = reloadMax;
                        }
                    }
                }
                else
                {
                    reload = 0;
                }

            }
            else
            {
                _canon.transform.rotation = Quaternion.Slerp (_canon.transform.rotation, transform.rotation, Time.deltaTime * _angularSpeed);

            }
        }
		else
		{
            _state = EntityStates.Neutral;

        }

    }
    
    public void Shoot ()
    {
        GameObject Bullet = Instantiate (_projectile, _rocketSpawn.transform.position, _rocketSpawn.transform.rotation) as GameObject;
        Bullet.GetComponent<Rigidbody> ().velocity = _rocketSpawn.transform.forward * 60;
        Projectile projectile = Bullet.GetComponent<Projectile> ();
        projectile.power = _power;
        projectile._team = Team;
    }

    public void SetDestination (Vector3 destination)
	{
        _currentDestination = destination;
    }

    public void MoveTo()
	{
        _navMeshAgent.SetDestination (_currentDestination);
        _canon.transform.rotation = Quaternion.Slerp (_canon.transform.rotation, transform.rotation, Time.deltaTime * _angularSpeed);
        if (Vector3.Distance(_currentDestination, transform.position) < 1)
		{
            _state = EntityStates.Neutral;
        }
    }

}
