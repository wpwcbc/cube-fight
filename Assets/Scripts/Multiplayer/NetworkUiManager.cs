using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUiManager : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hostButton.onClick.AddListener(onHostButtonClicked);
        clientButton.onClick.AddListener(onClientButtonClicked);
    }

    private void onHostButtonClicked()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void onClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
    }
}
