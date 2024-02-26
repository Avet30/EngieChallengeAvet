using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngieChallenge.CORE.Exceptions
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
