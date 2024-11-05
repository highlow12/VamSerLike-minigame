using System.Collections;
using System.Collections.Generic;
using BackEnd;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using TMPro;

public class UserAuth : MonoBehaviour
{
    BackendManager backendManager;

    void Start()
    {
        backendManager = BackendManager.Instance;
    }

    void Update()
    {

    }

    public static bool CustomSignUp(ref PopUp.AuthInputForm inputForm)
    {
        string id = inputForm.id;
        string password = inputForm.password;
        string email = inputForm.email;
        bool result = true;
        Debug.Log("Requesting SignUp...");
        Debug.Log($"ID: {id}, Password: {password}");

        // id validation logic
        if (id.Length < 4)
        {
            Debug.LogError("ID must be at least 4 characters long");
            return false;
        }

        // password validation logic
        if (password.Length < 8)
        {
            Debug.LogError("Password must be at least 8 characters long");
            return false;
        }

        // email validation logic
        if (!Regex.IsMatch(email, @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])"))
        {
            Debug.LogError("Invalid Email Address");
            return false;
        }

        Backend.BMember.CustomSignUp(id, password, (callback) =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log($"SignUp Success: {callback.GetStatusCode()}\n{callback.GetMessage()}\n{callback}");
                Backend.BMember.UpdateCustomEmail(email, (callback) =>
                {
                    if (callback.IsSuccess())
                    {
                        Debug.Log($"Email Update Success: {callback.GetStatusCode()}\n{callback.GetMessage()}\n{callback}");
                    }
                    else
                    {
                        Debug.LogError($"Email Update Failed: {callback.GetStatusCode()}\n{callback.GetMessage()}\n{callback}");
                        result = false;
                    }
                });
            }
            else
            {
                Debug.LogError($"SignUp Failed: {callback.GetStatusCode()}\n{callback.GetMessage()}\n{callback}");
                result = false;
            }
        });
        if (!result)
        {
            return false;
        }



        return result;
    }

    public static bool CustomSignIn(ref PopUp.AuthInputForm inputForm)
    {
        string id = inputForm.id;
        string password = inputForm.password;
        bool result = true;
        Debug.Log("Requesting SignIn...");
        Debug.Log($"ID: {id}, Password: {password}");
        Backend.BMember.CustomLogin(id, password, (callback) =>
        {
            if (callback.IsSuccess())
            {
                Debug.Log($"SignIn Success: {callback.GetStatusCode()}\n{callback.GetMessage()}\n{callback}");
            }
            else
            {
                Debug.LogError($"SignIn Failed: {callback.GetStatusCode()}\n{callback.GetMessage()}\n{callback}");
                result = false;
            }
        });
        return result;

    }



}
