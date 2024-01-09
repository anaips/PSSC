using System.ComponentModel.DataAnnotations;

namespace ProiectPSSC.Models
{
    public class Input
    {
        [Required]
        public string Nume { get; set; }

        [Required]
        public string NumeProdus { get; set; }

        [Required]
        public string Adresa { get; set; }

        [Required]
        [Range(1,int.MaxValue)]
        public int Cantitate { get; set; }
    }
}
