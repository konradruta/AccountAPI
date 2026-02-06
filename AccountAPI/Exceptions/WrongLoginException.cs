namespace AccountAPI.Exceptions
{
    public class WrongLoginException : Exception
    {
        public WrongLoginException(string message) : base(message)
        {
            
        }
    }
}
