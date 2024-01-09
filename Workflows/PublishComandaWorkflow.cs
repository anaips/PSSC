using LanguageExt;
using Microsoft.Extensions.Logging;
using ProiectPSSC.Domain.Commands;
using ProiectPSSC.Domain.Models;
using ProiectPSSC.Domain.Operations;
using ProiectPSSC.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using static ProiectPSSC.Domain.Models.Comenzi;
using static ProiectPSSC.Domain.WorkflowEvents.ComandaPublishedEvent;

namespace ProiectPSSC.Domain.Workflows
{
    public class PublishComandaWorkflow
    {
        private readonly IUtilizatoRepository usersRepository;
        private readonly IProdusRepository productsRepository;
        private readonly IComandaRepository commandsRepository;
        private readonly ILogger<PublishComandaWorkflow> logger;
        private readonly FacturareWorkflowv2 facturareWorkflowv2;
        private readonly LivrareWorkflow livrareWorkflow;

        public PublishComandaWorkflow(FacturareWorkflowv2 facturareWorkflowv2, LivrareWorkflow livrareWorkflow, IUtilizatoRepository usersRepository, IComandaRepository ordersRepository, IProdusRepository productsRepository,ILogger<PublishComandaWorkflow> logger)
        {
            this.facturareWorkflowv2 = facturareWorkflowv2;
            this.livrareWorkflow = livrareWorkflow;
            this.usersRepository = usersRepository;
            this.commandsRepository = ordersRepository;
            this.productsRepository = productsRepository;
            this.logger = logger;
        }

        public async Task<IComandaPublishedEvent> ExecuteAsync(PublishComandaCommands command)
        {
            UnvalidatedOrders unvalidatedOrders = new UnvalidatedOrders(command.InputComanda);

            var result = from users in usersRepository.TryGetExistingUsers(unvalidatedOrders.CommandList.Select(order => order.Nume))
                                          .ToEither(ex => new FailedOrders(unvalidatedOrders.CommandList, ex) as IComenzi)
                         from products in productsRepository.TryGetExistingProducts(unvalidatedOrders.CommandList.Select(order => order.NumeProdus))
                         .ToEither(ex => new FailedOrders(unvalidatedOrders.CommandList, ex) as IComenzi)
                         from stocks in productsRepository.TryGetExistingStocks(unvalidatedOrders.CommandList.Select(order => order.NumeProdus))
                         .ToEither(ex => new FailedOrders(unvalidatedOrders.CommandList, ex) as IComenzi)
                         from price in productsRepository.TryGetExistingPrices(unvalidatedOrders.CommandList.Select(order => order.NumeProdus))
                         .ToEither(ex => new FailedOrders(unvalidatedOrders.CommandList, ex) as IComenzi)
                         from existingOrders in commandsRepository.TryGetExistingOrders()
                                          .ToEither(ex => new FailedOrders(unvalidatedOrders.CommandList, ex) as IComenzi)
                         let checkUserExists = (Func<Nume, Option<Nume>>)(user => CheckUserExists(users, user))
                         let checkProductExists = (Func<NumeProdus, Option<NumeProdus>>)(product => CheckProductExists(products, product))
                         let checkStock = (Func<Cantitate,NumeProdus, Option<Cantitate>>)((stock, numeProdus) => CheckStock(products,stocks, stock, numeProdus))
                         
                         //let checkStock = (Func<Cantitate, Option<NumeProdus>>)(product => CheckStoc(products, product))
                         from publishedOrders in ExecuteWorkflowAsync(unvalidatedOrders, existingOrders,price, checkUserExists,checkStock,checkProductExists).ToAsync()
                         from _ in commandsRepository.TrySaveOrders(publishedOrders)
                                          .ToEither(ex => new FailedOrders(unvalidatedOrders.CommandList, ex) as IComenzi)
                         select publishedOrders;
            
            return await result.Match(
                    Left: orders => GenerateFailedEvent(orders) as IComandaPublishedEvent,
                    Right:publishedOrders => {facturareWorkflowv2.ExecuteWorkflowAsync(publishedOrders).ToAsync();livrareWorkflow.ExecuteWorkflowAsync(publishedOrders).ToAsync(); return new ComandaPublishScucceededEvent(publishedOrders.Csv, publishedOrders.PublishedDate); }
            );
        }

        private async Task<Either<IComenzi, PublishedOrders>> ExecuteWorkflowAsync(UnvalidatedOrders unvalidatedOrders,
                                                                                          IEnumerable<CalculatedComanda> existingOrders,int price,
                                                                                          Func<Nume, Option<Nume>> checkUserExists, Func<Cantitate, NumeProdus, Option<Cantitate>> checkStock, Func<NumeProdus, Option<NumeProdus>> checkProductExists)
        {

            IComenzi orders = await ComandaOperations.ValidateOrders(checkUserExists,checkStock, checkProductExists, unvalidatedOrders,price);
            orders = ComandaOperations.CalculateFinalOrders(orders);
            orders = ComandaOperations.MergeOrders(orders, existingOrders);
            orders = ComandaOperations.PublishOrders(orders);
            //Console.WriteLine(orders);
            return orders.Match<Either<IComenzi, PublishedOrders>>(
                whenUnvalidatedOrders: unvalidatedOrders => Left(unvalidatedOrders as IComenzi),
                whenCalculatedOrders: calculatedOrders => Left(calculatedOrders as IComenzi),
                whenInvalidOrders: invalidOrders => Left(invalidOrders as IComenzi),
                whenFailedOrders: failedOrders => Left(failedOrders as IComenzi),
                whenValidatedOrders: validatedOrders => Left(validatedOrders as IComenzi),
                whenPublishedOrders: publishedOrders => Right(publishedOrders)
            );
        }

        private Option<Nume> CheckUserExists(IEnumerable<Nume> users, Nume nume)
        {
            if (users.Any(s => s == nume))
            {
                return Some(nume);
            }
            else
            {
                return None;
            }
        }
        private Option<NumeProdus> CheckProductExists(IEnumerable<NumeProdus> products, NumeProdus numeProdus)
        {
            if (products.Any(s => s == numeProdus))
            {
                return Some(numeProdus);
            }
            else
            {
                return None;
            }
        }
        private Option<Cantitate> CheckStock(IEnumerable<NumeProdus> products, Array stocks, Cantitate cantitate, NumeProdus numeProdus)
        {
            int ok = 0;
            int cantitateint = cantitate.Value;
            for (int i = 0; i <= stocks.Length - 1; i++)
            {             
                if (int.Parse(stocks.GetValue(i).ToString()) < cantitateint)
                {
                   var indexprodus = i;
                   if (products.ElementAt(indexprodus) == numeProdus)
                   ok = 1;
                }               
            }
            if (ok == 0)
            {
                return Some(cantitate);
            }
            else
            {
                return None;
            }
        }
        
        private ComandaPublishFaildEvent GenerateFailedEvent(IComenzi Orders) =>
        Orders.Match<ComandaPublishFaildEvent>(
                whenUnvalidatedOrders: unValidatedOrders => new($"Invalid state {nameof(unValidatedOrders)}"),
                whenInvalidOrders: InvalidOrders => new(InvalidOrders.Reason),
                whenValidatedOrders: ValidatedOrders => new($"Invalid state {nameof(ValidatedOrders)}"),
                whenFailedOrders: FailedOrders =>
                {
                    logger.LogError(FailedOrders.Exception, FailedOrders.Exception.Message);
                    return new(FailedOrders.Exception.Message);
                },
                whenCalculatedOrders: CalculatedOrders => new($"Invalid state {nameof(CalculatedOrders)}"),
                whenPublishedOrders: PublishedOrders => new($"Invalid state {nameof(PublishedOrders)}"));
    }
}
