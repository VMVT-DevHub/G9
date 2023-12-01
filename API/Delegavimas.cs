using System.Text.Json;
using App.Auth;
using G9.Models;

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
			await DBExtensions.PrintArray("SELECT * FROM public.v_deklar WHERE \"GVTS\" = ANY(@gvts)", gvts, writer, ct);
			writer.WriteEndObject();
			await writer.FlushAsync(ct);
		} else Error.E403(ctx,true);
	}

	/// <summary>Pridėti deleguojamą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="asmuo">Deleguojamas asmuo</param>
	public static async Task Set(HttpContext ctx, long gvts, DelegavimasSet asmuo, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync(JsonSerializer.Serialize(asmuo),ct);
	}
	
	/// <summary>Pašalinti deleguotą asmenį</summary>
	/// <param name="ctx"></param><param name="ct"></param><returns></returns>
	/// <param name="gvts">Geriamo vandens tiekimo sistema</param>
	/// <param name="id">Vartotojo identifikatorius</param>
	public static async Task Del(HttpContext ctx, long gvts, Guid id, CancellationToken ct){
		ctx.Response.ContentType="application/json";
		await ctx.Response.WriteAsync($"{{\"id\":\"{gvts}\",\"ak\":\"{id}\"}}",ct);
	}
}