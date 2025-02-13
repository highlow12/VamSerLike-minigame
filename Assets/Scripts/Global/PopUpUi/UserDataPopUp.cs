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
        if (BackendDataManager.Instance.isSignedIn && !isLoaded)
        {
            BackendDataManager.UserData userData = BackendDataManager.GetUserData();
            panelText.text = $"GamerId: {userData.gamerId}\nNickname: {userData.nickname}\nCountryCode: {userData.countryCode}\nEmailForFindPassword: {userData.emailForFindPassword}\nSubscriptionType: {userData.subscriptionType}\nFederationId: {userData.federationId}\nInDate: {userData.inDate}";
            isLoaded = true;
        }
    }
}
