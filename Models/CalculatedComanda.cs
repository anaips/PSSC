using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectPSSC.Domain.Models
{
    public record CalculatedComanda(Nume Nume, Adresa Adresa, NumeProdus NumeProdus, Cantitate Cantitate, Cantitate PretTotal)
    {
        public int ID_Comanda { get; set; }
        public bool IsUpdated { get; set; }
    }
}
