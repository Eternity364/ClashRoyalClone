using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour {


    [SerializeField] private Button authenticateButton;
    [SerializeField] private GameObject waitingText;


    private void Awake() {
        authenticateButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.Authenticate(EditPlayerName.Instance.GetPlayerName(), () =>
            {
                Hide();
            });
            authenticateButton.gameObject.SetActive(false);
            waitingText.SetActive(true);
        });
    }

    private void Hide() {
        gameObject.SetActive(false);
    }

}