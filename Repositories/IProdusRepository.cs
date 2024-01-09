using LanguageExt;
using ProiectPSSC.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ProiectPSSC.Domain.Models.Comenzi;

namespace ProiectPSSC.Domain.Repositories
{
    public interface IProdusRepository
    {
        TryAsync<List<NumeProdus>> TryGetExistingProducts(IEnumerable<string> productsToCheck);
        TryAsync<Array> TryGetExistingStocks(IEnumerable<string> stocksToCheck);
        TryAsync<int> TryGetExistingPrices(IEnumerable<string> stocksToCheck);
    }
}
