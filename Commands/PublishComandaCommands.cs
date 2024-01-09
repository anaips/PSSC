using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProiectPSSC.Domain.Models;

namespace ProiectPSSC.Domain.Commands
{
    public record PublishComandaCommands
    {
        public PublishComandaCommands(IReadOnlyCollection<UnvalidatedComanda> inputComanda)
        {
            InputComanda = inputComanda;
        }
        public IReadOnlyCollection<UnvalidatedComanda> InputComanda { get; }
    }

}
