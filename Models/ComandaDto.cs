using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectPSSC.Data.Models
{
    public class ComandaDto
    {
        public int ID_Comanda { get; set; }
        public int ID_Utilizator { get; set; }
        
        public DateTime DataComanda { get; set; }
        public int PretTotal { get; set; }
        public int Cantitate { get; set; }
        public int ID_Produs { get; set; }
    }
}
