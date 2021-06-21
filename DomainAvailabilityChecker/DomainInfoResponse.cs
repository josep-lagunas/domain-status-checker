namespace DomainAvailabilityChecker
{
    internal class DomainInfoResponse
    {
        public string Domain { get; }
        public CheckStatus Status { get; }
        public decimal FirstDomainPrice { get; }
        public decimal RealDomainPrice { get; }

        public string Currency { get; }

        private DomainInfoResponse(string domain, CheckStatus status, decimal firstDomainPrice,
            decimal realDomainPrice, string currency)
        {
            Domain = domain;
            Status = status;
            FirstDomainPrice = firstDomainPrice;
            RealDomainPrice = realDomainPrice;
            Currency = currency;
        }

        public static DomainInfoResponse Create(string domain, CheckStatus status,
            decimal firstDomainPrice, decimal realDomainPrice, string currency)
        {
            return new(domain, status, firstDomainPrice, realDomainPrice,
                currency);
        }
    }
}