using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace ProiectPSSC.Domain.Models
{
    public record Nume
    {
       
        public string? Value { get; }
        public Nume(string value)
        {   
            Value = value;
        }

        public override string? ToString()
        {
            return Value;
        }
        private static bool IsValid(string stringValue) => stringValue.All(char.IsLetter);
        public static Option<Nume> TryParse(string stringValue)
        {
            if (IsValid(stringValue))
            {
                return Some<Nume>(new(stringValue));
            }
            else
            {
                return None;
            }
        }
    }
}
