using System.Text.Json;
using App.Auth;
using G9.Models;
using Microsoft.AspNetCore.Localization.Routing;

namespace App.API;

/// <summary>Vartotojo teisių delegavimas</summary>
public static class Delegavimas {
	/// <summary>Gauti administruojamų GVTS deleguotus asmenis</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	public static async Task Get(HttpContext ctx,CancellationToken ct){
		var rle = ctx.GetUser()?.Admin;
		if(rle?.Count>0){
			ctx.Response.ContentType="application/json";
			var options = new JsonWriterOptions{ Indented = false }; //todo: if debug
			using var writer = new Utf8JsonWriter(ctx.Response.BodyWriter, options);
			writer.WriteStartObject();
			var gvts = new DBParams(("@gvts", rle.ToArray()));
			writer.WritePropertyName("GVTS");
			await DBExtensions.PrintArray("SELECT * FROM public.v_gvts WHERE \"ID\" = ANY(@gvts);", gvts, writer, ct);
			writer.WritePropertyName("Users");
			await DBExtensions.PrintArray("SELECT * FROM public.gvts_users(@gvts);", gvts, writer, ct);
			writer.WriteEndObject();
			await writer.FlushAsync(ct);
		} else Error.E403(ctx,true);
	}

	/// <summary>Pridėti deleguojamą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="user">Deleguojamas asmuo</param>
	public static async Task Set(HttpContext ctx, long gvts, DelegavimasSet user, CancellationToken ct){
		var usr = ctx.GetUser();
		if(usr?.Admin?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			var guid = await new DBExec("SELECT app.user_check(@ak);","@ak",user.AK).ExecuteScalar<Guid?>(ct);
			if(usr.ID==guid) Error.E422(ctx,true,"Vartotojas negali deleguoti savęs.");
			else {
				if(guid is null){
					using var db = new DBExec("SELECT * FROM app.user_add(@ak,@fname,@lname,null,null);",("@ak",user.AK),("@fname",user.FName),("@lname",user.LName));
					using var rdr = await db.GetReader(ct);
					if(await rdr.ReadAsync(ct)){
						if(await rdr.IsDBNullAsync(0,ct)) { Error.E422(ctx,true,rdr.GetStringN(1)??"Nežinoma klaida"); return; }
						guid = rdr.GetGuid(0);
					} else { Error.E500(ctx,true,"Neįmanoma sukurti vartotojo"); return; }
				}
				if(await AddUser(gvts,guid,user.Admin,ct) is null) {
					var rle = await new DBExec($"SELECT public.gvts_group(@gvts,@adm);",("@gvts",gvts),("@adm",user.Admin)).ExecuteScalar<string?>(ct);
					if(rle is not null) { Error.E422(ctx,true,rle); return; }
				}
				await AddUser(gvts,guid,user.Admin,ct);
				ctx.Response.StatusCode=204;
				await ctx.Response.CompleteAsync();
			}
		}
		else Error.E403(ctx,true);
	}
	private static async Task<long?> AddUser(long gvts, Guid? guid, bool adm, CancellationToken ct) =>
		await new DBExec($"SELECT id FROM app.user_role_add(@usr,'g9.{gvts}{(adm?".admin":"")}');",("@usr",guid),("@role",gvts)).ExecuteScalar<long?>(ct);
	
	
	/// <summary>Pašalinti deleguotą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="user">Vartotojo identifikatorius</param>
	public static async Task Del(HttpContext ctx, long gvts, Guid user, CancellationToken ct){
		var usr = ctx.GetUser();
		if(usr?.Admin?.Contains(gvts) == true) {
			ctx.Response.ContentType="application/json";
			if(usr.ID==user) Error.E422(ctx,true,"Vartotojas negali pašalinti pats save.");
			else {
				new DBExec($"SELECT app.user_role_del(@usr,'g9.{gvts}'), app.user_role_del(@usr,'g9.{gvts}.admin');","@usr",user).Execute();
			}
			ctx.Response.StatusCode=204;
			await ctx.Response.CompleteAsync();
		}
		else Error.E403(ctx,true);
	}
}