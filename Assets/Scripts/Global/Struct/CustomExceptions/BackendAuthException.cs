using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackendAuthException : MonoBehaviour
{
    public struct ExceptionStruct : IException
    {
        string message;
        string code;
        public void HandleException()
        {

        }
        public string GetMessage()
        {
            return message;
        }
        public string GetCode()
        {
            return code;
        }
    }
}
