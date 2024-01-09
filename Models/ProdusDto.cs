using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectPSSC.Data.Models
{
    public class ProdusDto
    {
        public int ID_Produs { get; set; }
        public string? NumeProdus { get; set; }
        public int Pret { get; set; }
        public int Stoc { get; set; }
    }
}
