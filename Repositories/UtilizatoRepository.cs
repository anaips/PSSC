using LanguageExt;
using Microsoft.EntityFrameworkCore;
using ProiectPSSC.Domain.Models;
using ProiectPSSC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectPSSC.Data.Repositories
{
    public class UtilizatoRepository: IUtilizatoRepository
    {
        private readonly ComandaContext comandaContext;

        public UtilizatoRepository(ComandaContext comandaContext)
        {
            this.comandaContext = comandaContext;
        }

        public TryAsync<List<Nume>> TryGetExistingUsers(IEnumerable<string> usersToCheck) => async () =>
        {
            var users = await comandaContext.Users
                                              .Where(user => usersToCheck.Contains(user.Nume))
                                              .AsNoTracking()
                                              .ToListAsync();
            return users.Select(user => new Nume(user.Nume))
                           .ToList();
        };
    }
}
