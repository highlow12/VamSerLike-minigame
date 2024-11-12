using UnityEngine;
using TMPro;
public class UserDataPopUp : MonoBehaviour
{
    [SerializeField] private TMP_Text panelText;
    private bool isLoaded = false;

    void Start()
    {

    }

    void Update()
    {
        if (BackendManager.Instance.isSignedIn && !isLoaded)
        {
            BackendManager.UserData userData = BackendManager.GetUserData();
            panelText.text = $"GamerId: {userData.gamerId}\nNickname: {userData.nickname}\nCountryCode: {userData.countryCode}\nEmailForFindPassword: {userData.emailForFindPassword}\nSubscriptionType: {userData.subscriptionType}\nFederationId: {userData.federationId}\nInDate: {userData.inDate}";
            isLoaded = true;
        }
    }
}
