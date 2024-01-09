using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectPSSC.Domain.Models
{
    public record ValidatedComanda(Nume Nume, Adresa Adresa, NumeProdus NumeProdus, Cantitate Cantitate,Cantitate PretTotal);
}
