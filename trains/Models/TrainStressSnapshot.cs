namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class TrainStressSnapshot
    {
        public TrainStressSnapshot(float maxTensionKips, float maxCompressionKips, int worstCouplerIndex)
        {
            MaxTensionKips = maxTensionKips;
            MaxCompressionKips = maxCompressionKips;
            WorstCouplerIndex = worstCouplerIndex;
        }

        public float MaxTensionKips { get; }

        public float MaxCompressionKips { get; }

        public int WorstCouplerIndex { get; }
    }
}
