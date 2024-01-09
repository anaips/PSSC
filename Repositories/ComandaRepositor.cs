using static LanguageExt.Prelude;
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
using LanguageExt;
using static ProiectPSSC.Domain.Models.Facturi;

namespace ProiectPSSC.Data.Repositories
{
    public class ComandaRepositor : IComandaRepository
    {
        private readonly ComandaContext dbContext;
        // private readonly ComandaContext dbContext2;

        public ComandaRepositor(ComandaContext dbContext)
        {
            this.dbContext = dbContext;

        }

        public TryAsync<List<CalculatedComanda>> TryPayOrder(string nume) => async () => (await (
                          from g in dbContext.Commands
                          join f in dbContext.Products on g.ID_Produs equals f.ID_Produs
                          join s in dbContext.Users on g.ID_Utilizator equals s.ID_Utilizator
                          select new { s.Nume, s.Adresa, f.NumeProdus, g.Cantitate, g.PretTotal, g.ID_Comanda })
                          .AsNoTracking()
                          .ToListAsync())
                          .Where(result => result.Nume == nume)
                          .Select(result => new CalculatedComanda(
                                                    Nume: new(result.Nume),
                                                    Adresa: new(result.Adresa),
                                                    NumeProdus: new(result.NumeProdus),
                                                    Cantitate: new(result.Cantitate),
                                                    PretTotal: new(result.PretTotal))


                          {
                              ID_Comanda = result.ID_Comanda
                          })
                          .ToList();
        public TryAsync<List<CalculatedComanda>> TryGetExistingOrders() => async () => (await (
                          from g in dbContext.Commands
                          join f in dbContext.Products on g.ID_Produs equals f.ID_Produs
                          join s in dbContext.Users on g.ID_Utilizator equals s.ID_Utilizator
                          select new { s.Nume, s.Adresa, f.NumeProdus, g.Cantitate, g.PretTotal, g.ID_Comanda })
                          .AsNoTracking()
                          .ToListAsync())
                          .Select(result => new CalculatedComanda(
                                                    Nume: new(result.Nume),
                                                    Adresa: new(result.Adresa),
                                                    NumeProdus: new(result.NumeProdus),
                                                    Cantitate: new(result.Cantitate),
                                                    PretTotal: new(result.PretTotal))

                          {
                              ID_Comanda = result.ID_Comanda
                          })
                          .ToList();

        public TryAsync<Unit> TrySaveOrders(PublishedOrders orders) => async () =>
        {
            var users = (await dbContext.Users.ToListAsync()).ToLookup(user => user.Nume);
            var products = (await dbContext.Products.ToListAsync()).ToLookup(produs => produs.NumeProdus);

            var newOrders = orders.CommandList
                                    .Where(g => g.IsUpdated && g.ID_Comanda == 0)
                                    .Select(g => new ComandaDto()
                                    {
                                        ID_Comanda = g.ID_Comanda,
                                        ID_Utilizator = users[g.Nume.Value].Single().ID_Utilizator,
                                        ID_Produs = products[g.NumeProdus.Value].Single().ID_Produs,
                                        Cantitate = g.Cantitate.Value,
                                        DataComanda = DateTime.Today,
                                        PretTotal = products[g.NumeProdus.Value].Single().Pret * g.Cantitate.Value,

                                    });
            var updatedProducts = orders.CommandList.Where(m => m.IsUpdated)
                                    .Select(m => new ProdusDto()
                                    {
                                        ID_Produs = products[m.NumeProdus.Value].Single().ID_Produs,
                                        NumeProdus = products[m.NumeProdus.Value].Single().NumeProdus,
                                        Stoc = products[m.NumeProdus.Value].Single().Stoc - m.Cantitate.Value,
                                        Pret = products[m.NumeProdus.Value].Single().Pret
                                    });
            var updatedOrders = orders.CommandList.Where(g => g.IsUpdated && g.ID_Comanda > 0)
                                    .Select(g => new ComandaDto()
                                    {
                                        ID_Comanda = g.ID_Comanda,
                                        ID_Utilizator = users[g.Nume.Value].Single().ID_Utilizator,
                                        ID_Produs = products[g.NumeProdus.Value].Single().ID_Produs,
                                        Cantitate = g.Cantitate.Value,
                                        DataComanda = DateTime.Today,
                                        PretTotal = products[g.NumeProdus.Value].Single().Pret * g.Cantitate.Value,
                                    });

            dbContext.AddRange(newOrders);
            foreach (var entity in updatedOrders)
            {
                dbContext.Entry(entity).State = EntityState.Modified;

            }

            foreach (var entity in updatedProducts)
            {
                dbContext.Entry(entity).State = EntityState.Modified;

            }
            await dbContext.SaveChangesAsync();

            return unit;
        };
        public TryAsync<Unit> TrySavePay(PublishedPay order) => async () =>
        {
            var users = (await dbContext.Users.ToListAsync()).ToLookup(user => user.Nume);
            var products = (await dbContext.Products.ToListAsync()).ToLookup(produs => produs.NumeProdus);
            var orders = (await dbContext.Commands.ToListAsync()).ToLookup(order => order.ID_Comanda);

            var newOrders = order.CommandList
                                     .Where(m => m.IsUpdated && m.ID_Comanda == 0)
                                     .Select(m => new ComandaDto()
                                     {
                                         ID_Comanda = orders[m.ID_Comanda].Single().ID_Comanda,
                                         ID_Utilizator = orders[m.ID_Comanda].Single().ID_Utilizator,
                                         ID_Produs = orders[m.ID_Comanda].Single().ID_Produs,
                                         Cantitate = orders[m.ID_Comanda].Single().Cantitate,
                                         DataComanda = orders[m.ID_Comanda].Single().DataComanda,
                                         PretTotal = orders[m.ID_Comanda].Single().PretTotal,

                                     });

            var updatedOrders = order.CommandList
                                     .Where(m => m.IsUpdated && m.ID_Comanda > 0)
                                     .Select(m => new ComandaDto()
                                     {
                                         ID_Comanda = orders[m.ID_Comanda].Single().ID_Comanda,
                                         ID_Utilizator = orders[m.ID_Comanda].Single().ID_Utilizator,
                                         ID_Produs = orders[m.ID_Comanda].Single().ID_Produs,
                                         Cantitate = orders[m.ID_Comanda].Single().Cantitate,
                                         DataComanda = orders[m.ID_Comanda].Single().DataComanda,
                                         PretTotal = orders[m.ID_Comanda].Single().PretTotal,

                                     });
           // dbContext.Remove(newOrders);
            foreach (var entity in updatedOrders)
            {
                dbContext.Remove(entity).State = EntityState.Modified;

            }

         
            await dbContext.SaveChangesAsync();

            return unit;
        };
        }
        
}
