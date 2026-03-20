using System;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class RepairWorkEstimate
    {
        private readonly Func<float, float> _estimateConditionDelta;

        public RepairWorkEstimate(float repairWorkUnitsAvailable, Func<float, float> estimateConditionDelta)
        {
            RepairWorkUnitsAvailable = repairWorkUnitsAvailable;
            _estimateConditionDelta = estimateConditionDelta ?? throw new ArgumentNullException(nameof(estimateConditionDelta));
        }

        public float RepairWorkUnitsAvailable { get; }

        public float EstimateConditionDelta(float currentCondition)
        {
            return _estimateConditionDelta(currentCondition);
        }
    }
}
