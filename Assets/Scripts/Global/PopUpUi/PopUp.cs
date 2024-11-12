using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopUp : MonoBehaviour
{
    public enum PopUpType
    {
        Auth,
        Confirm,
        Error,
        Info,
        Warning
    }

    public enum PopUpState
    {
        Open,
        Close
    }

    public struct AuthInputForm
    {
        public string id;
        public string password;
        public string passwordConfirm;
        public string email;
    }

    public PopUpType popUpType;
    public PopUpState popUpState;

    public virtual void OnClickButton()
    {
        Debug.Log("Button Clicked");
    }

    public virtual void OnClickSubmit()
    {
        Debug.Log("Submit Button Clicked");
    }

    public virtual void OnClickClose()
    {
        Debug.Log("Close Button Clicked");
    }

    public virtual void OnClickConfirm()
    {
        Debug.Log("Confirm Button Clicked");
    }

}
