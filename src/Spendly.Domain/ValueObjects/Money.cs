namespace Spendly.Domain.ValueObjects
{
    public sealed class Money
    {
        public decimal Value { get; }
        public string Currency { get; }

        // EF Core constructor (value only — currency comes from separate column)
        private Money(decimal value) : this(value, "USD") { }

        // Full constructor
        private Money(decimal value, string currency)
        {
            if (value <= 0)
                throw new ArgumentException("Money value must be greater than zero.", nameof(value));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency code is required.", nameof(currency));

            Value = value;
            Currency = currency.ToUpperInvariant();
        }

        // Métodos de fábrica estáticos
        public static Money FromDecimal(decimal value) => new(value, "USD");

        public static Money Create(decimal value, string currency) => new(value, currency);

        // Sobrescribir Equals y GetHashCode para que se compare por valor
        public override bool Equals(object? obj)
        {
            if (obj is Money other)
                return Value == other.Value && Currency == other.Currency;

            return false;
        }

        public override int GetHashCode() => HashCode.Combine(Value, Currency);

        // Operadores para sumar/restar dinero (misma moneda)
        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException($"Cannot add {a.Currency} and {b.Currency}. Convert first.");
            return new Money(a.Value + b.Value, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException($"Cannot subtract {a.Currency} and {b.Currency}. Convert first.");
            var result = a.Value - b.Value;
            if (result <= 0)
                throw new InvalidOperationException("Resulting money must be greater than zero.");
            return new Money(result, a.Currency);
        }

        public override string ToString() => $"{Value:N2} {Currency}";
    }
}
