using LanguageExt;
using ProiectPSSC.Domain.Models;
using static LanguageExt.Prelude;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ProiectPSSC.Domain.Models.Comenzi;

namespace ProiectPSSC.Domain.Workflows
{
    public class LivrareWorkflow
    {
        public async Task<Either<IComenzi, PublishedOrders>> ExecuteWorkflowAsync(PublishedOrders publishedOrders)
        {
            string numeproduse = "";
            var nume = publishedOrders.CommandList.Select(order => order.Nume);
            var numeProduse = publishedOrders.CommandList.Where(order => order.Nume == nume.ElementAt(0)).Select(order => order.NumeProdus);

            for (int i = 0; i < numeProduse.Length(); i++)
            {
                numeproduse = numeproduse + numeProduse.ElementAt(i).ToString() + " ";
            }
            var Adresa = publishedOrders.CommandList.Select(order => order.Adresa);
            var cantitate = CalculateTotalQuantity(publishedOrders, nume).ToString();
            var suma = CalculateTotalAmount(publishedOrders, nume).ToString();
            string[] file = { "Nume: " + nume.ElementAt(0).ToString(), "Produse:" + numeproduse.ToString(), "Adresa: " + Adresa.ElementAt(0).ToString(), "Cantitate: " + cantitate, "Total Plata: " + suma, "Data emitere:" + publishedOrders.PublishedDate.ToShortDateString(), "Durata Livrare: maxim 48 de ore" };
            await File.WriteAllLinesAsync(fullPath, file);

            return publishedOrders.Match<Either<IComenzi, PublishedOrders>>(
                whenUnvalidatedOrders: unvalidatedOrders => Left(unvalidatedOrders as IComenzi),
                whenCalculatedOrders: calculatedOrders => Left(calculatedOrders as IComenzi),
                whenInvalidOrders: invalidOrders => Left(invalidOrders as IComenzi),
                whenFailedOrders: failedOrders => Left(failedOrders as IComenzi),
                whenValidatedOrders: validatedOrders => Left(validatedOrders as IComenzi),
                whenPublishedOrders: publishedOrders => Right(publishedOrders)
            );
        }
        private int CalculateTotalAmount(PublishedOrders publishedOrders, IEnumerable<Nume> nume)
        {

            int totalAmount = publishedOrders.CommandList.Where(order => order.Nume == nume.ElementAt(0)).Sum(order => order.PretTotal.Value);

            return totalAmount;
        }
        private int CalculateTotalQuantity(PublishedOrders publishedOrders, IEnumerable<Nume> nume)
        {

            int totalQuantity = publishedOrders.CommandList.Where(order => order.Nume == nume.ElementAt(0)).Sum(order => order.Cantitate.Value);

            return totalQuantity;
        }
        string fullPath = @"C:\Users\sympl\Desktop\Livrare.txt";
    }
}
