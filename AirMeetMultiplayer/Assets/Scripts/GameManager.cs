using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
public class GameManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI connectedText;
    private NetworkVariable<int> peopleConnected = new NetworkVariable<int>();
    public LeftJoystick leftJoystick;
    public RightJoystick rightJoystick;


    public int PeopleConnected
    {
        get { return peopleConnected.Value; }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                peopleConnected.Value++;
                connectedText.text = $"People Connected: {peopleConnected.Value}";
            }
        };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (NetworkManager.Singleton.IsServer)
            {
                peopleConnected.Value--;
                connectedText.text = $"People Connected: {peopleConnected.Value}";
            }
        };

    }



    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }


    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
