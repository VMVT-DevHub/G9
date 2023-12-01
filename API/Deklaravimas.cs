using System.Data;
using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql.Replication.TestDecoding;

namespace App.API;


/// <summary>Veiklų API</summary>
public static class Veiklos {
	/// <summary>Veiklų statinės reikšmės</summary>
	public static CachedLookup VeiklosVal { get; } = new CachedLookup("Veiklos", ("Stebesenos","lkp_stebesenos"),("Statusas","lkp_statusas"));

	/// <summary>Gauti vartotojo veiklas</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		writer.WriteStartObject();
		var rls = ctx.GetUser()?.Roles?.ToArray();
		var gvts = new DBParams(("@gvts", rls));
		writer.WritePropertyName("JA");
		await DBExtensions.PrintArray("SELECT DISTINCT \"ID\",\"Title\",\"Addr\" FROM public.v_gvts_ja WHERE gvts = ANY(@gvts);", gvts, writer, ct);
		writer.WritePropertyName("GVTS");
		await DBExtensions.PrintArray("SELECT * FROM public.v_gvts WHERE \"ID\" = ANY(@gvts);", gvts, writer, ct);
		writer.WritePropertyName("Deklaracijos");
		await DBExtensions.PrintArray("SELECT * FROM public.v_deklar WHERE \"GVTS\" = ANY(@gvts)", gvts, writer, ct, VeiklosVal);
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
	}
}

/// <summary>Rodiklių API</summary>
 public static class Rodikliai {
	/// <summary>Rodiklių statinės reikšmės</summary>
	public static CachedLookup RodikliaiVal { get; } = new CachedLookup("Rodikliai", ("Grupe","lkp_rodikliu_grupes"),("Daznumas","lkp_daznumas"));

	/// <summary>Dažnumo statinės reikšmės</summary>
	public static CachedLookup DaznumasVal { get; } = new CachedLookup("Daznumas", ("Daznumas","lkp_daznumas"),("Laikas","lkp_daznumo_laikas"));

	/// <summary>Gauti visus G9 rodiklius</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <returns></returns>
 	public static async Task List(HttpContext ctx,CancellationToken ct){ 		
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		writer.WriteStartObject();
		var rls = ctx.GetUser()?.Roles?.ToArray();
		writer.WritePropertyName("Rodikliai");
		//TODO: Cache this
		await DBExtensions.PrintArray("SELECT * FROM public.v_rodikliai;", null, writer, ct, RodikliaiVal);
		writer.WritePropertyName("Daznumas");
		await DBExtensions.PrintArray("SELECT * FROM public.v_daznumas;", null, writer, ct,  DaznumasVal);

		writer.WritePropertyName("Stebesenos");
		writer.WriteStartArray();
		foreach(var i in Veiklos.VeiklosVal["Stebesenos"]){
			if(int.TryParse(i.Key, out var key)){
				writer.WriteStartObject();
				writer.WriteNumber("ID", key);
				writer.WriteString("Pavadinimas", i.Value);
				writer.WritePropertyName("Rodikliai");
				await DBExtensions.PrintList("SELECT stb_rodiklis FROM public.stebesenos WHERE stb_stebesenos=@key ORDER BY stb_rodiklis;",new(("@key",key)),writer,ct);
				writer.WriteEndObject();
			}
		}
		writer.WriteEndArray();
		writer.WriteEndObject();
		await writer.FlushAsync(ct);
 	}

}


/// <summary>Reikšmių API</summary>
public static class Reiksmes{
	/// <summary>Gauti suvestas rodiklių reikšmes</summary>
	/// <param name="ctx"></param>
	/// <param name="ct"></param>
	/// <param name="deklaracija">Geriamo vandens tiekimo sistems</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, long deklaracija,CancellationToken ct){
		var gvts = await new DBExec("SELECT dkl_gvts FROM deklaravimas WHERE dkl_id=@id;", "@id", deklaracija).ExecuteScalar<long>(ct);
		if(gvts>0){
			if(ctx.GetUser()?.Roles?.Contains(gvts) == true) await PrintRodikl(ctx,deklaracija,ct);
			else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
	}
	
	private static async Task PrintRodikl(HttpContext ctx, long deklaracija, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
		using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
		await DBExtensions.PrintArray("SELECT * FROM public.v_reiksmes where \"Deklaracija\"=@id;", new(("@ID",deklaracija)), writer, ct);
		await writer.FlushAsync(ct);
		return;
	}
	/// <summary>Įrašyti rodiklio reikšmę</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">Rodiklio reikšmių masyvas</param>
	/// <param name="ct"></param>
	/// <returns></returns>
 	public static async Task Set(HttpContext ctx, long deklaracija, List<RodiklisSet> data ,CancellationToken ct){
		using var db = new DBExec("SELECT dkl_gvts, dkl_status, dkl_metai FROM deklaravimas WHERE dkl_id=@id;","@id",deklaracija);
		using var rdr = await db.GetReader(ct);
		if(rdr.Read()){
			var usr = ctx.GetUser();
			if(usr?.ID is not null && usr.Roles?.Contains(rdr.GetInt64(0)) == true){			
				if(rdr.GetInt32(1)==3) Error.E422(ctx,true,$"Negalima keisti jau deklaruotų duomenų");
				else {
					var flush = Config.GetInt("DBBatch","Reiksmes",100);
					var cnt = flush;
					var metai = rdr.GetInt32(2);
					var rod = new List<int>();
					var dte = new List<DateOnly>();
					var val = new List<double>();
					foreach(var i in data){
						if(i.Data.Year!=metai) { Error.E422(ctx,true,"Įrašo data neatitinka deklaruojamų metų"); return; }
						rod.Add(i.Rodiklis); dte.Add(i.Data); val.Add(i.Reiksme);
						if(cnt--<1){ await WriteReiksmes(deklaracija,usr.ID,rod,dte,val,ct); cnt=flush;  }
					}
					await WriteReiksmes(deklaracija,usr.ID,rod,dte,val,ct);
					//TODO: Log stuff;
					ctx.Response.StatusCode=204;
					await ctx.Response.CompleteAsync();
				}
			} else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
 	}

	private static async Task<int> WriteReiksmes(long deklaracija, Guid? user, List<int> rodk, List<DateOnly> date, List<double> reiksme, CancellationToken ct){
		var ret = 0;
		if(rodk.Count>0) {
			ret = await new DBExec("INSERT into public.reiksmes (rks_deklar,rks_user,rks_rodiklis,rks_date,rks_reiksme) SELECT @id, @usr, t.* FROM unnest(@rod,@date,@val) t;",
			("@id",deklaracija),("@usr",user),("@rod",rodk),("@date",date),("@val",reiksme)).Execute(ct);
		}
		rodk.Clear(); date.Clear(); reiksme.Clear();
		return ret;
	}

	/// <summary>Rodiklio reikšmių trynimas</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">Įvedimo identifikatorių masyvas</param>
	/// <param name="ct"></param>
	/// <returns></returns>
 	public static async Task Del(HttpContext ctx, long deklaracija, [FromBody] List<long> data, CancellationToken ct){
		using var db = new DBExec("SELECT dkl_gvts, dkl_status, dkl_metai FROM deklaravimas WHERE dkl_id=@id;","@id",deklaracija);
		using var rdr = await db.GetReader(ct);
		if(rdr.Read()){
			if(ctx.GetUser()?.Roles?.Contains(rdr.GetInt64(0)) == true){			
				if(rdr.GetInt32(1)==3) Error.E422(ctx,true,$"Negalima keisti jau deklaruotų duomenų");
				else {
					 await new DBExec("DELETE FROM public.reiksmes WHERE rks_deklar=@id and rks_id = ANY(@lst)",("@id",deklaracija),("@lst",data)).Execute(ct);
					//TODO: Log stuff;
					ctx.Response.StatusCode=204; await ctx.Response.CompleteAsync();
				}
			} else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
 	}
}


/// <summary>Deklaracijų API</summary>
public static class Deklaracija {
	/// <summary>Gauti informaciją apie deklaruojamus metus</summary>
	/// <param name="ctx"></param><param name="ct"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="metai">Deklaruojami metai</param>
	/// <returns></returns>
	public static async Task Get(HttpContext ctx, long gvts, int metai, CancellationToken ct){
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true) await PrintDeklar(ctx,gvts,metai,ct);
		else Error.E403(ctx,true);
	}

	private static async Task PrintDeklar(HttpContext ctx, long gvts, int metai, CancellationToken ct){
		using var db = new DBExec("SELECT * FROM public.v_deklar WHERE \"GVTS\"=@gvts and \"Metai\"=@metai;", ("@gvts",gvts), ("@metai",metai));
		using var rdr = await db.GetReader(ct);
		if(rdr.HasRows){
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			writer.WriteStartObject();
				await DBExtensions.GetObject(rdr,writer,ct);
			writer.WriteEndObject();
			await writer.FlushAsync(ct);
			return;
		} else Error.E404(ctx,true);
	}


	/// <summary>Deklaravimo pagrindinės informacijos pildymas</summary>
	/// <param name="ctx"></param>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="metai">Deklaruojami metai</param>
	/// <param name="dcl">Deklaruojami duomenys</param>
	/// <param name="ct"></param><returns></returns>
	public static async Task Set(HttpContext ctx, long gvts, int metai, [FromBody] DeklaracijaSet dcl, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		if(ctx.GetUser()?.Roles?.Contains(gvts) == true){
			var param = new DBParams(("@gvts",gvts), ("@metai",metai), ("@kiekis",dcl.Kiekis),("@vartot",dcl.Vartotojai));
			var stat = await new DBExec("SELECT dkl_status FROM public.deklaravimas WHERE dkl_gvts=@gvts and dkl_metai=@metai;", param).ExecuteScalar<int?>(ct);
			if(stat is null) Error.E404(ctx, true);
			else if(stat==3) Error.E422(ctx,true,$"Negalima keisti jau deklaruotų duomenų");
			else {
				await new DBExec("UPDATE public.deklaravimas SET dkl_kiekis=@kiekis, dkl_vartot=@vartot WHERE dkl_gvts=@gvts and dkl_metai=@metai;", param).Execute(ct);
				await PrintDeklar(ctx,gvts,metai,ct);
			}
		} else Error.E403(ctx,true);
	}
}

/// <summary>Deklaravimo API</summary>
public static class Deklaravimas {
	/// <summary>Deklaracijos validacija (NEVEIKIA)</summary>
	/// <param name="ctx"></param>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="ct"></param><returns></returns>
	public static async Task Valid(HttpContext ctx, long deklaracija, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)){
			
		}
	}

	/// <summary>Deklaracijos pateikimas (NEVEIKIA)</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">Neatitikimų patvirtinimo duomenys</param>
	public static async Task Submit(HttpContext ctx, long deklaracija, [FromBody] DeklaravimasSet data, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)){
			if(data is not null) {
				await Save(ctx, data, ct);
			}
		}
	}
	
	/// <summary>Deklaracijos neatitikimų pildymas (NEVEIKIA)</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="deklaracija">Deklaracijos ID</param>
	/// <param name="data">Neatitikimų patvirtinimo duomenys</param>
	public static async Task Update(HttpContext ctx, long deklaracija, [FromBody] DeklaravimasSet data, CancellationToken ct){
		if(await Validate(ctx,deklaracija,ct)){
				await Save(ctx, data, ct);
		}
	}

	private static async Task<bool> Save(HttpContext ctx, DeklaravimasSet data, CancellationToken ct){
		if(data.Trukumas?.Count>0){}
		if(data.Kartojasi?.Count>0){}
		if(data.Virsijimas?.Count>0){}
		await ctx.Response.WriteAsync("",ct);
		return true;
	}

	private static async Task<bool> Validate(HttpContext ctx, long deklaracija, CancellationToken ct){
		using var db = new DBExec("SELECT dkl_gvts, dkl_status, dkl_metai, dkl_kiekis FROM deklaravimas WHERE dkl_id=@id;","@id",deklaracija);
		using var rdr = await db.GetReader(ct);
		if(rdr.Read()){
			if(ctx.GetUser()?.Roles?.Contains(rdr.GetInt64(0)) == true){	
				if(rdr.GetIntN(3)>0) {	
					var status = rdr.GetInt32(1);
					if(status==3) Error.E422(ctx,true,$"Ši deklaracija jau pateikta.");
					else {					
						if(status!=2 && ctx.Request.Method=="POST") 
							Error.E422(ctx,true,$"Šios deklaracijos pateikti negalima.");
						else return true;
					}
				} else Error.E422(ctx,true,"Neįvestas deklaruojamas vandens kiekis.");
			} else Error.E403(ctx,true);
		} else Error.E404(ctx,true);
		return false;
	}
}
