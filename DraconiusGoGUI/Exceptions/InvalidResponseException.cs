#region using directives

using System;

#endregion

namespace DraconiusGoGUI.Exceptions
{
    [Serializable]
    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {
        }

        public InvalidResponseException(string message)
            : base(message)
        {
        }
    }
}