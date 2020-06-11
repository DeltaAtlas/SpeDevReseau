using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIOClient;
using System;
using System.Collections.Specialized;

public class Network : MonoBehaviour
{
    [SerializeField]
    private SystemPlayer _systemPlayer;

    public event Action<int> PlayedCountChange;
    public event Action StartGame;

    [SerializeField]
    private UnitManager _unitManager;

    private static Network _instance;
    private Client PlayerIoClient;
    private Connection _connection;
    private List<Message> _msgList = new List<Message> ();
    private Dictionary<string, Player> _playersData = new Dictionary<string, Player> ();

    private int _numberPlayers;

    private string _localUserID;
    public Player LocalPlayer { get => _playersData[_localUserID]; } 

	public Dictionary<string, Player> PlayersData { get => _playersData; set => _playersData = value; }

	void Awake()
	{
        if(_instance != null)
		{
            DestroyImmediate (gameObject);
            return;
		}
        _instance = this;
	}

    public static Network GetInstance()
	{
        return _instance;
	}

    void Start ()
    {
        Application.runInBackground = true;

        // Create a random userid 
        System.Random random = new System.Random ();
        _localUserID = "Général" + random.Next (0, 10000);

        Debug.Log ("Starting");

        PlayerIO.Authenticate (
     "the-world-war-game-itxmadhafeczufiu9pb1ga", // Copier/Coller le gameId présent sur le backoffice du jeu
     "public", // Voir les types de connexion, par défaut on laisse en "public" (pas safe)
     new Dictionary<string, string>  // arguments passés à la connexion (userId, mdp, etc...)
     {
        {"userId", _localUserID},
     },
     null, // Segments dans lequel placer le joueur pour PlayerInsight (analytics)
     delegate (Client client)
     {
        // Ce qu'il se passe quand la connexion est établie avec succès
        Debug.Log ("Connexion à Player.io établie pour le client : " + client);

        // conseil : garder le paramètre "Client" en variable de classe, on va s'en servir souvent
        PlayerIoClient = client;

         // Commenter cette ligne pour un serveur en production !
         PlayerIoClient.Multiplayer.DevelopmentServer = new ServerEndpoint ("localhost", 8184);

         ConnectToRoom ();
     },
     delegate (PlayerIOError error)
     {
         Debug.Log ("Erreur à la connexion avec Player.io: " + error.ToString ());
     }
 );

    }



    void ConnectToRoom ()
    {
        PlayerIoClient.Multiplayer.CreateJoinRoom (
        "MyRoom", // nom/id de la partie (pas l'id du jeu), pour le test on lui assigne un nom par défaut
        "TheWorldWarGame", // type de room, doit correspondre au GameCode du serveur !
        true, // la salle est elle visible par l'appel à "ListRooms" ?
        null, // opt: les données à passer à la room pour renseigner "ListRooms", par exemple le nom de la map choisie...
        null, // opt :les données du joueur qui vient d'entrer
        delegate (Connection connection)
        {
            Debug.Log ("Vous avez rejoint une partie.");

        // comme pour le client, stocker cet objet car on va l'utiliser
        _connection = connection;
        // on ajoute une écoute sur l'événement "OnMessage" qui recevra les messages du serveur
        _connection.OnMessage += HandleMessage;
        },
        delegate (PlayerIOError error)
        {
            Debug.Log ("Erreur à la connexion à une partie : " + error.Message + " (" + error.ErrorCode + ")");
        });
    }


    // méthode déléguée
    private void HandleMessage (object sender, Message e)
    {
        // on ajoute simplement le message à la liste
        _msgList.Add (e);
    }

    void FixedUpdate ()
    {


        if(Input.GetKeyDown(KeyCode.P))
		{
            Debug.Log ("il rentre au moins");
            Disconnect ();
            return;
		}

        // traitement de la file de message
        foreach (Message m in _msgList)
        {
            // on traite chaque type de message séparément
            switch (m.Type)
            {
                case "PlayerJoined":
                    PlayerJoined (m);
                    break;

				case "PlayerLeft":
					PlayerLeft (m);
					break;

                case "CreateBuild":
                    CreateBuild (m);
                    break;

                case "StartGame":
                    StartRealyGame (m);
                    break;

                case "MoveUnity":
                    MoveUnity (m);
                    break;
                    //case "Move":
                    //    Move (m);
                    //    break;

                    //case "Chat":
                    //    ChatText (m.GetString (0), m.GetString (1));
                    //    break;
            }
        }

        // une fois tous les messages de la file traités, on la vide pour en attendre de nouveaux
        _msgList.Clear ();
    }

    private void StartRealyGame(Message m)
	{
        StartGame?.Invoke ();
    }

    private void MoveUnity(Message m)
	{
        var UnityId = m.GetInt (0);
        var PointX = m.GetFloat (1);
        var PointY = m.GetFloat (2);
        var PointZ = m.GetFloat (3);
        var State = m.GetInt (4);
        Vector3 destination = new Vector3 (PointX, PointY, PointZ);
        Debug.Log (_unitManager.units[UnityId]);
        Tank currentTank = _unitManager.units[UnityId].GetComponent<Tank> ();
        currentTank.SetEnnemy (null);
        currentTank.SetDestination (destination);
        currentTank._state = (EntityStates)State;
    }

    private void PlayerJoined (Message m)
    {
        Debug.Log ("PlayerJoined");
        // on recupère les données (dans le bon ordre !) du message
        var userId = m.GetString (0);
        var colorIndex = m.GetInt (1);

        // on créé nos données joueurs
        var playerData = new Player
		{
			Name = userId,
			Team = (Team)colorIndex,
		};
        //// qu'on stock
        _playersData.Add (userId, playerData);

        PlayedCountChange?.Invoke (_playersData.Count);

	}

    private void PlayerLeft (Message m)
    {
        Debug.Log ("PlayerLeft");
        var userId = m.GetString (0);
        PlayersData.Remove (userId);

        PlayedCountChange?.Invoke (_playersData.Count);
        // TODO : Retirer toutes les entités du joueur
    }


    private void CreateBuild(Message m)
	{

        Debug.Log ("CreateBuild");
        var buildId = m.GetInt (0);
        var buildPosX = m.GetFloat (1);
        var buildPosY = m.GetFloat (2);
        var buildPosZ = m.GetFloat (3);
        var buildTeam = m.GetInt (4);
        Vector3 buildPos = new Vector3 (buildPosX, buildPosY, buildPosZ);
        _systemPlayer.CreateBuild (buildId, buildPos, buildTeam);

    }


    private void MaxPlayer ()
    {
        Debug.Log ("MaxPlayer");
        _connection.Send ("MaxPlayer");
    }

    //// exemple de méthode envoyant un message "Bomb" avec deux coordonnées float en paramètre
    //public void SendBomb (float x, float y)
    //{
    //    _connection.Send ("Bomb", x, y);
    //}

    // déconnexion (à appeler dans un OnApplicationQuit par exemple)

    public void EntityMove(Entity tank, Vector3 point, EntityStates state)
	{
        _connection.Send ("MoveUnity", tank.UnitID, point.x, point.y, point.z, (int)state);
    }

    public void SendCreateUnit()
	{
        _connection.Send ("CreateUnit", (int)LocalPlayer.Team);
	}

    public void Disconnect ()
    {
        _connection.Disconnect ();
    }


}
