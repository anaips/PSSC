using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharp.Choices;

namespace ProiectPSSC.Domain.Models
{
    [AsChoice]
    public static partial class Comenzi
    {
        public interface IComenzi { }

        public record UnvalidatedOrders : IComenzi
        {
            public UnvalidatedOrders(IReadOnlyCollection<UnvalidatedComanda> commandList)
            {
                CommandList = commandList;
            }

            public IReadOnlyCollection<UnvalidatedComanda> CommandList { get; }
        }

        public record InvalidOrders : IComenzi
        {
            internal InvalidOrders(IReadOnlyCollection<UnvalidatedComanda> commandList, string reason)
            {
                CommandList = commandList;
                Reason = reason;
            }

            public IReadOnlyCollection<UnvalidatedComanda> CommandList { get; }
            public string Reason { get; }
        }

        public record FailedOrders : IComenzi
        {
            internal FailedOrders(IReadOnlyCollection<UnvalidatedComanda> commandList, Exception exception)
            {
                CommandList = commandList;
                Exception = exception;
            }

            public IReadOnlyCollection<UnvalidatedComanda> CommandList { get; }
            public Exception Exception { get; }
        }

        public record ValidatedOrders : IComenzi
        {
            internal ValidatedOrders(IReadOnlyCollection<ValidatedComanda> commandList)
            {
                CommandList = commandList;
            }

            public IReadOnlyCollection<ValidatedComanda> CommandList { get; }
        }

        public record CalculatedOrders : IComenzi
        {
            internal CalculatedOrders(IReadOnlyCollection<CalculatedComanda> commandList)
            {
                CommandList = commandList;
            }

            public IReadOnlyCollection<CalculatedComanda> CommandList { get; }
        }

        public record PublishedOrders : IComenzi
        {
            internal PublishedOrders(IReadOnlyCollection<CalculatedComanda> commandList, string csv, DateTime publishedDate)
            {
                CommandList = commandList;
                PublishedDate = publishedDate;
                Csv = csv;
            }

            public IReadOnlyCollection<CalculatedComanda> CommandList { get; }
            public DateTime PublishedDate { get; }
            public string Csv { get; }
        }
    }
}
