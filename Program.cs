using App.API;
using App.Auth;

var app = Startup.Build(args);

app.MapGet("/",()=>new G9.Models.Rodiklis()).ExcludeFromDescription();


app.MapGet("/auth/login",Auth.Login).Swagger("", "Vartotojas peradresuojamas į VIISP prisijungimą").Produces(302);
app.MapGet("/auth/logout",Auth.Logout).Swagger("", "Vartotojo atsijungimas nuo sistemos.").Produces(302);

app.MapGet("/auth/login/evartai",Auth.Evartai).ExcludeFromDescription();

app.MapGet("/api/user",Auth.Get).Swagger(
	"","Informacija apie prisijungusį vartotoją"
).Response<G9.Models.Vartotojas>(200,401).AddEndpointFilter(Require.Login);


// Teisių delegavimas
app.MapGet("/api/deleg",Delegavimas.Get).Swagger(
	"","Gauti visas prisijungusio asmens administruojamų GVTS sąrašą."
).Response<G9.Models.Delegavimas>(200,422,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/deleg/{gvts}",Delegavimas.Set).Swagger(	
	"","Pridėti deleguojamą asmenį"
).Produces(204).Errors(422,401,403,404).AddEndpointFilter(Require.Role);

app.MapDelete("/api/deleg/{gvts}",Delegavimas.Del).Swagger(	
	"","Trinti deleguojamą asmenį"
).Produces(204).Errors(422,401,403,404).AddEndpointFilter(Require.Role);


app.MapGet("/api/deklar/{gvts}/{metai}",Deklaravimas.Get).Swagger(	
	"","Gaunamas deklaruojamų metų objektas"
).Response<G9.Models.Deklaracija>(200,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/deklar/{gvts}/{metai}",Deklaravimas.Set).Swagger(	
 	"","Daklaracijos duomenų įvedimas"
).Response<G9.Models.Deklaracija>(200,422,401,403,404).AddEndpointFilter(Require.Role);


app.MapGet("/api/deklaruoti/{deklaracija}",Deklaravimas.Valid).Swagger(	
 	"","Tikrinti ar galima delkaruoti"
).Response<G9.Models.Deklaravimas>(200,422,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/deklaruoti/{deklaracija}",Deklaravimas.Submit).Swagger(	
 	"","Pateikti deklaraciją"
).Response<G9.Models.Deklaravimas>(200,422,401,403,404).AddEndpointFilter(Require.Role);

app.MapPut("/api/deklaruoti/{deklaracija}",Deklaravimas.Update).Swagger(	
 	"","Atnaujinti delkaracijos neatitikimus"
).Response<G9.Models.Deklaravimas>(200,422,401,403,404).AddEndpointFilter(Require.Role);



app.MapGet("/api/veiklos",Veiklos.Get).Swagger(	
	"","Gaunamos prisijungusio vartotojo veiklos ir deklaruojami metai"
).Response<G9.Models.Veiklos>(200,401,403).AddEndpointFilter(Require.Role);



app.MapGet("/api/rodikliai",Rodikliai.List).Swagger(	
	"","Gaunamas pilnas rodiklių sąrašas"
).Response<G9.Models.RodikliuSarasas>(200).AddEndpointFilter(Require.Login);




app.MapGet("/api/reiksmes/{deklaracija}",Reiksmes.Get).Swagger(	
	"","Gauti deklaruojamų rodiklių reikšmes"
).Response<G9.Models.ArrayModelA<G9.Models.RodiklioReiksme>>(200,401,403,404).AddEndpointFilter(Require.Role);

app.MapPost("/api/reiksmes/{deklaracija}",Reiksmes.Set).Swagger(	
 	"", "Įrašomos rodiklių reikšmės"
).Produces(204).Errors(422,401,403,404).AddEndpointFilter(Require.Role);

app.MapDelete("/api/reiksmes/{deklaracija}",Reiksmes.Del).Swagger(	
 	"", "Pašalinti rodiklių reikšmes deklaracijoje"
).Produces(204).Errors(422,401,403,404).AddEndpointFilter(Require.Role);


app.Run();
