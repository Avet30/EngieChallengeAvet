using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngieChallenge.CORE.Domain
{
    public class PlanScenario
    {
        public List<PlannedOutput>? PlannedOutputs { get; set; }
        public decimal TotalCost { get; set; }
        public decimal RemainingLoad { get; set; }
    }
}

