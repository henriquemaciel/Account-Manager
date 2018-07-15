using System;

namespace DraconiusGoGUI.Exceptions
{
    [Serializable]
    public class AccountNotVerifiedException : Exception
    {
        public AccountNotVerifiedException() : base("Account is not verified")
        {
            
        }
    }
}