#https://stackoverflow.com/questions/62065739/asp-net-core-web-api-client-does-not-trust-self-signed-certificate-used-by-the-i
#https://stackoverflow.com/questions/61997345/how-to-allow-https-connections-from-both-localhost-and-container-towards-an-asp



dotnet dev-certs https --clean

dotnet dev-certs https -ep C:\Temp\Net_Kurs1\Dotnetmicroservices_Kurs1\https\aspnetapp.pfx -p my-password-for-the-Cert+Start.is!not!so.complex! --trust
Kopiere nach C:\Users\thomas.staub\.aspnet\https



dotnet dev-certs https --clean --import /https/aspnetapp.pfx -p my-password-for-the-Cert+Start.is!not!so.complex!
