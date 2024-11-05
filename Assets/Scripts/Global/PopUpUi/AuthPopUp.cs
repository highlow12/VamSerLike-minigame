using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;


public class AuthPopUp : PopUp
{
    public enum AuthPopUpType
    {
        Login,
        SignUp
    }

    public AuthPopUp()
    {
        popUpType = PopUpType.Auth;
        popUpState = PopUpState.Open;
    }

    public AuthPopUpType authPopUpType = AuthPopUpType.Login;

    // Components
    public TMP_InputField idInput;
    public TMP_InputField passwordInput;
    public TMP_InputField emailInput;
    public Button submitButton;
    public Button changeButton;
    public GameObject signInUi;
    public GameObject signUpUi;
    public AuthInputForm inputForm = new();

    void Start()
    {
        if (idInput == null || passwordInput == null || submitButton == null)
        {
            Debug.LogError("Components are not assigned");
        }
        else
        {
            submitButton.onClick.AddListener(() => OnClickSubmit());
            changeButton.onClick.AddListener(() => OnClickButton());
        }
    }


    public void OnIdInput()
    {
        inputForm.id = idInput.text;
    }

    public void OnPasswordInput()
    {
        inputForm.password = passwordInput.text;
    }

    public void OnEmailInput()
    {
        inputForm.email = emailInput.text;
    }

    public override void OnClickButton()
    {
        if (signInUi.activeSelf)
        {
            signInUi.SetActive(false);
            signUpUi.SetActive(true);
            authPopUpType = AuthPopUpType.SignUp;

            submitButton.onClick.RemoveAllListeners();
            submitButton = Array.Find(signUpUi.GetComponentsInChildren<Button>(), x => x.name == "SubmitButton");
            submitButton.onClick.AddListener(() => OnClickSubmit());

            changeButton.onClick.RemoveAllListeners();
            changeButton = Array.Find(signUpUi.GetComponentsInChildren<Button>(), x => x.name == "ChangeButton");
            changeButton.onClick.AddListener(() => OnClickButton());

            idInput = Array.Find(signUpUi.GetComponentsInChildren<TMP_InputField>(), x => x.name == "IdInput");
            passwordInput = Array.Find(signUpUi.GetComponentsInChildren<TMP_InputField>(), x => x.name == "PwInput");
            emailInput = Array.Find(signUpUi.GetComponentsInChildren<TMP_InputField>(), x => x.name == "EmailInput");

        }
        else
        {
            signInUi.SetActive(true);
            signUpUi.SetActive(false);
            authPopUpType = AuthPopUpType.Login;

            submitButton.onClick.RemoveAllListeners();
            submitButton = Array.Find(signInUi.GetComponentsInChildren<Button>(), x => x.name == "SubmitButton");
            submitButton.onClick.AddListener(() => OnClickSubmit());

            changeButton.onClick.RemoveAllListeners();
            changeButton = Array.Find(signInUi.GetComponentsInChildren<Button>(), x => x.name == "ChangeButton");
            changeButton.onClick.AddListener(() => OnClickButton());

            idInput = Array.Find(signInUi.GetComponentsInChildren<TMP_InputField>(), x => x.name == "IdInput");
            passwordInput = Array.Find(signInUi.GetComponentsInChildren<TMP_InputField>(), x => x.name == "PwInput");
        }

    }

    public override void OnClickSubmit()
    {
        if (authPopUpType == AuthPopUpType.SignUp)
        {
            UserAuth.CustomSignUp(ref inputForm);
        }
        else
        {
            UserAuth.CustomSignIn(ref inputForm);
        }
    }

    public override void OnClickClose()
    {
        Debug.Log("Close Button Clicked");
    }

    public override void OnClickConfirm()
    {
        Debug.Log("Confirm Button Clicked");
    }

    public void Test()
    {
        var res = UserAuth.CustomSignUp(ref inputForm);
        Debug.Log(res);
    }

}
