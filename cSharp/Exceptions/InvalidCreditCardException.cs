namespace CamundaTraining.Exceptions
{
    public class InvalidCreditCardException : Exception
    {
        public InvalidCreditCardException(string message) : base(message)
        {
        }
    }
}