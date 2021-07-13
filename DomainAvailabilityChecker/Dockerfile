FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY /DomainAvailabilityChecker.sln .
COPY /DomainAvailabilityChecker/*.csproj DomainAvailabilityChecker/

RUN dotnet restore

# Copy csproj and restore as distinct layers
COPY /DomainAvailabilityChecker.sln .
COPY /DomainAvailabilityChecker/domain-names.txt .
COPY /DomainAvailabilityChecker/. DomainAvailabilityChecker/

RUN dotnet publish -c Release -o out

RUN ls /app/DomainAvailabilityChecker


# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:5.0

# environment variables
ENV Environment Production
ENV credentials__staging__apiKey 6a08de3894e5168dd65a0eea18325d2b
ENV credentials__staging__apiSecret C0O84+MLWsHa2J5F8wLrHz92sWQY3FeoCyAr3WPVc1L0EBY5dr3XYZQa7CwPImzFcaIvbwZEksYW1zq+cKihUg==
ENV credentials__staging__passPhrase SandBoxPassphrase
ENV credentials__staging__isSSandBox true
ENV credentials__production__apiKey bfa63e6ca2fb61bf1679e6b6ccbe4e8c
ENV credentials__production__apiSecret f+uzuZgO8H+Bla45wgWr0z5PPBtmYDAUotR+wANS7x2AQJUHR1bkMhElWYhEud7xjWmookDWxvPVLAk6wXDcag==
ENV credentials__production__passPhrase LesTateNELARBiThImpT
ENV credentials__production__isSSandBox true

WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "DomainAvailabilityChecker.dll"]