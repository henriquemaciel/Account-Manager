using System;

namespace DraconiusGoGUI.Exceptions
{
    [Serializable]
    public class AuthConfigException : Exception
    {
        public AuthConfigException(string message) : base(message)
        {

        }
    }
}
