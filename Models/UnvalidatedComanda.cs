using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectPSSC.Domain.Models
{
    public record UnvalidatedComanda(string Nume, string Adresa, string NumeProdus, int Cantitate);
}
