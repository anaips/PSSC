using LanguageExt;
using ProiectPSSC.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProiectPSSC.Domain.Models.Comenzi;
using static LanguageExt.Prelude;
using CSharp.Choices;

namespace ProiectPSSC.Domain.Operations
{
    public static class ComandaOperations
    {
        public static Task<IComenzi> ValidateOrders(Func<Nume, Option<Nume>> checkUserExists, Func<Cantitate, NumeProdus, Option<Cantitate>> checkStock, Func<NumeProdus, Option<NumeProdus>> checkProductExists, UnvalidatedOrders Orders,int price) =>
            Orders.CommandList
                      .Select(ValidateOrder(checkUserExists,checkStock,checkProductExists,price))
                      .Aggregate(CreateEmptyValatedOrdersList().ToAsync(), ReduceValidOrders)
                      .MatchAsync(
                            Right: validatedOrders => new ValidatedOrders(validatedOrders),
                            LeftAsync: errorMessage => Task.FromResult((IComenzi)new InvalidOrders(Orders.CommandList, errorMessage))
                      );

        private static Func<UnvalidatedComanda, EitherAsync<string, ValidatedComanda>> ValidateOrder(Func<Nume, Option<Nume>> checkUserExists, Func<Cantitate, NumeProdus, Option<Cantitate>> checkStock, Func<NumeProdus, Option<NumeProdus>> checkProductExists,int price) =>
            unvalidatedOrder => ValidateOrder(checkUserExists,checkStock, checkProductExists, unvalidatedOrder,price);

        private static EitherAsync<string, ValidatedComanda> ValidateOrder(Func<Nume, Option<Nume>> checkUserExists, Func<Cantitate, NumeProdus, Option<Cantitate>> checkStock, Func<NumeProdus, Option<NumeProdus>> checkProductExists, UnvalidatedComanda unvalidatedOrder,int price) =>
            from cantitate in Cantitate.TryParseCantitate(unvalidatedOrder.Cantitate)
                                   .ToEitherAsync($"Invalid Cantitate ({unvalidatedOrder.Nume}, {unvalidatedOrder.Cantitate})")
            from nume in Nume.TryParse(unvalidatedOrder.Nume)
                                   .ToEitherAsync($"Invalid Name ({unvalidatedOrder.Nume}, {unvalidatedOrder.Nume})")
            from numeProdus in NumeProdus.TryParse(unvalidatedOrder.NumeProdus)
                                   .ToEitherAsync($"Invalid NumeProdus ({unvalidatedOrder.NumeProdus})")
            from adresa in Adresa.TryParse(unvalidatedOrder.Adresa)
                               .ToEitherAsync($"Invalid Adresa ({unvalidatedOrder.Adresa})")
            from pret in Cantitate.TryParseCantitate(price)
                           .ToEitherAsync($"Invalid PretTotal ({unvalidatedOrder.Cantitate})")
            from userExists in checkUserExists(nume)
                                   .ToEitherAsync($"User {nume.Value} does not exist.")
            from productExists in checkProductExists(numeProdus)
                               .ToEitherAsync($"Product {numeProdus.Value} does not exist.")
            from cantitateValid in checkStock(cantitate,numeProdus)
                           .ToEitherAsync($"Stoc insuficient.")
            select new ValidatedComanda(nume, adresa, numeProdus,cantitate, pret*cantitate);

        private static Either<string, List<ValidatedComanda>> CreateEmptyValatedOrdersList() =>
            Right(new List<ValidatedComanda>());

        private static EitherAsync<string, List<ValidatedComanda>> ReduceValidOrders(EitherAsync<string, List<ValidatedComanda>> acc, EitherAsync<string, ValidatedComanda> next) =>
            from list in acc
            from nextOrder in next
            select list.AppendValidOrder(nextOrder);

        private static List<ValidatedComanda> AppendValidOrder(this List<ValidatedComanda> list, ValidatedComanda validOrder)
        {
            list.Add(validOrder);
            return list;
        }

        public static IComenzi CalculateFinalOrders(IComenzi Orders) => Orders.Match(
            whenUnvalidatedOrders: unvalidaTedOrder => unvalidaTedOrder,
            whenInvalidOrders: invalidOrder => invalidOrder,
            whenFailedOrders: failedOrder => failedOrder,
            whenCalculatedOrders: calculatedOrder => calculatedOrder,
            whenPublishedOrders: publishedOrder => publishedOrder,
            whenValidatedOrders: CalculateFinalOrder
        );

        private static IComenzi CalculateFinalOrder(ValidatedOrders validOrders) =>
            new CalculatedOrders(validOrders.CommandList
                                                    .Select(CalculateUserFinalOrder)
                                                    .ToList()
                                                    .AsReadOnly());

        private static CalculatedComanda CalculateUserFinalOrder(ValidatedComanda validOrder) =>
            new CalculatedComanda(validOrder.Nume,
                                      validOrder.Adresa,
                                      validOrder.NumeProdus,
                                      validOrder.Cantitate,
                                      validOrder.PretTotal);

        public static IComenzi MergeOrders(IComenzi Orders, IEnumerable<CalculatedComanda> existingOrders) => Orders.Match(
            whenUnvalidatedOrders: unvalidaTedOrder => unvalidaTedOrder,
            whenInvalidOrders: invalidOrder => invalidOrder,
            whenFailedOrders: failedOrder => failedOrder,
            whenValidatedOrders: validatedOrder => validatedOrder,
            whenPublishedOrders: publishedOrder => publishedOrder,
            whenCalculatedOrders: calculatedExam => MergeOrders(calculatedExam.CommandList, existingOrders));

        private static CalculatedOrders MergeOrders(IEnumerable<CalculatedComanda> newList, IEnumerable<CalculatedComanda> existingList)
        {
            var updatedAndNewOrders = newList.Select(order => order with { ID_Comanda = existingList.FirstOrDefault(g => g.Nume == order.Nume && g.NumeProdus == order.NumeProdus)?.ID_Comanda ?? 0, IsUpdated = true });
            var oldOrders = existingList.Where(order => !newList.Any(g => g.Nume == order.Nume && g.NumeProdus==order.NumeProdus));
            var allOrders = updatedAndNewOrders.Union(oldOrders)
                                               .ToList()
                                               .AsReadOnly();
            return new CalculatedOrders(allOrders);
        }

        public static IComenzi PublishOrders(IComenzi Orders) => Orders.Match(
            whenUnvalidatedOrders: unvalidaTedOrder => unvalidaTedOrder,
            whenInvalidOrders: invalidOrder => invalidOrder,
            whenFailedOrders: failedOrder => failedOrder,
            whenValidatedOrders: validatedOrder => validatedOrder,
            whenPublishedOrders: publishedOrder => publishedOrder,
            whenCalculatedOrders: GenerateExport);

        private static IComenzi GenerateExport(CalculatedOrders calculatedOrder) =>
            new PublishedOrders(calculatedOrder.CommandList,
                                    calculatedOrder.CommandList.Aggregate(new StringBuilder(), CreateCsvLine).ToString(),
                                    DateTime.Now);

        private static StringBuilder CreateCsvLine(StringBuilder export, CalculatedComanda order) =>
            export.AppendLine($"{order.Nume.Value}, {order.Adresa.Value}, {order.NumeProdus.Value}, {order.Cantitate}, {order.PretTotal}");
    }
}
