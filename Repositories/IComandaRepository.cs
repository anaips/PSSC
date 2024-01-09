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
    public interface IComandaRepository
    {
        TryAsync<List<CalculatedComanda>> TryGetExistingOrders();
        TryAsync<List<CalculatedComanda>> TryPayOrder(string nume);
        TryAsync<Unit> TrySaveOrders(PublishedOrders orders);
    }
}
