using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace ProiectPSSC.Domain.Models
{
    public record NumeProdus
    {
        public string? Value { get; }
        public  NumeProdus(string value)
        {

            Value = value;
        }

        public override string? ToString()
        {
            return Value;
        }
        private static bool IsValid(string stringValue) => true;
        public static Option<NumeProdus> TryParse(string stringValue)
        {
            if (IsValid(stringValue))
            {
                return Some<NumeProdus>(new(stringValue));
            }
            else
            {
                return None;
            }
        }
    }
}
