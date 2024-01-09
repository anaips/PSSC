using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.ClassInstances;
using Microsoft.EntityFrameworkCore;
using ProiectPSSC.Data.Models;
using ProiectPSSC.Domain.Models;
using ProiectPSSC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProiectPSSC.Domain.Models.Comenzi;
using System.Numerics;

namespace ProiectPSSC.Data.Repositories
{
    public class ProdusRepository: IProdusRepository
    {
        private readonly ComandaContext comandaContext;
  

        public ProdusRepository(ComandaContext comandaContext)
        {
            this.comandaContext = comandaContext;
        }

        public TryAsync<List<NumeProdus>> TryGetExistingProducts(IEnumerable<string> productsToCheck) => async () =>
        {
            var products = await comandaContext.Products
                                              .Where(product => productsToCheck.Contains(product.NumeProdus))
                                              .AsNoTracking()
                                              .ToListAsync();
            return products.Select(product => new NumeProdus(product.NumeProdus))
                           .ToList();
        };

        public TryAsync<Array> TryGetExistingStocks(IEnumerable<string> productsToCheck) => async () =>
        {

            var products = await comandaContext.Products
                                             .Where(product => productsToCheck.Contains(product.NumeProdus))
                                             .AsNoTracking()
                                             .ToListAsync();
            return products.Select(product => product.Stoc)
                           .ToArray();
        };


    }
}
