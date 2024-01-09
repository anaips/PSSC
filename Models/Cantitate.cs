using static LanguageExt.Prelude;
using LanguageExt;
//using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProiectPSSC.Domain.Exceptions;
//using static LanguageExt.Compositions<A>;

namespace ProiectPSSC.Domain.Models
{
    public record Cantitate
    {
        public int Value { get; }
       
        public Cantitate(int value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new InvalidCantitateException($"{value:0.##} is an invalid cantitate value.");
            }
        }
        public override string ToString()
        {
            return $"{Value:0.##}";
        }
        public static Cantitate operator *(Cantitate a, Cantitate b) => new Cantitate(a.Value * b.Value);

        public static Option<Cantitate> TryParseCantitate(int numericCantitate)
        {
            if (IsValid(numericCantitate))
            {
                return Some<Cantitate>(new(numericCantitate));
            }
            else
            {
                return Option<Cantitate>.None;
            }
        }
       
        public static Option<Cantitate> TryParseCantitate(string cantitateString)
        {
            if (int.TryParse(cantitateString, out int numericCantitate) && IsValid(numericCantitate))
            {
                return Some<Cantitate>(new(numericCantitate)) ;
            }
            else
            {
                return None;
            }
        }
        private static bool IsValid(int numericCantitate) => numericCantitate > 0;

       //public static Cantitate Total(Cantitate a, ProdusDto b) => new Grade((a.Value + b.Value) / 2);
    }
}
