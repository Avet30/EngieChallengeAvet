namespace EngieChallenge.CORE.Domain.Exceptions
{
    public class PlannedOutputCalculationException : Exception
    {
        public PlannedOutputCalculationException()
        {
        }

        public PlannedOutputCalculationException(string message)
            : base(message)
        {
        }

        public PlannedOutputCalculationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
