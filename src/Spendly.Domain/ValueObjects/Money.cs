using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spendly.Domain.ValueObjects
{
    public sealed class Money
    {
        public decimal Value { get; }

        // Constructor privado para evitar instanciación incorrecta
        private Money(decimal value)
        {
            if (value <= 0)
                throw new ArgumentException("Money value must be greater than zero.", nameof(value));

            Value = value;
        }

        // Método de fábrica estático (patrón Factory Method)
        public static Money FromDecimal(decimal value)
        {
            return new Money(value);
        }

        // Sobrescribir Equals y GetHashCode para que se compare por valor
        public override bool Equals(object? obj)
        {
            if (obj is Money other)
                return Value == other.Value;

            return false;
        }

        public override int GetHashCode() => Value.GetHashCode();

        // Operadores para sumar/restar dinero
        public static Money operator +(Money a, Money b) => FromDecimal(a.Value + b.Value);
        public static Money operator -(Money a, Money b)
        {
            var result = a.Value - b.Value;
            if (result <= 0)
                throw new InvalidOperationException("Resulting money must be greater than zero.");
            return FromDecimal(result);
        }

        public override string ToString() => Value.ToString("C"); // formato moneda
    }

}
