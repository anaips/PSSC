using LanguageExt;
using ProiectPSSC.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProiectPSSC.Domain.Models.Comenzi;

namespace ProiectPSSC.Domain.Repositories
{
    public interface IUtilizatoRepository
    {
        TryAsync<List<Nume>> TryGetExistingUsers(IEnumerable<string> usersToCheck);
    }
}
