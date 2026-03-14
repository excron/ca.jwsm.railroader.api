using System;

namespace Ca.Jwsm.Railroader.Api.Orders.Models
{
    public readonly struct OrderCorrelationId : IEquatable<OrderCorrelationId>
    {
        public OrderCorrelationId(Guid value)
        {
            Value = value;
        }

        public Guid Value { get; }

        public static OrderCorrelationId NewId()
        {
            return new OrderCorrelationId(Guid.NewGuid());
        }

        public bool Equals(OrderCorrelationId other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is OrderCorrelationId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString("D");
        }
    }
}
