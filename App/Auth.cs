namespace App.Auth;


/// <summary>Autorizacijos reikalavimo funkcijos</summary>
public static class Require {
	/// <summary>Privaloma autorizacija</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> Login(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) 
		=> ctx.HttpContext.GetAuth() ? await next(ctx) : Error.E401(ctx.HttpContext);

	/// <summary>Vartotojas turi turėti bent vieną rolę</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> Role (EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) =>
		ctx.HttpContext.GetAuth() ? (ctx.HttpContext.GetUser()?.Roles?.Count>0 ? await next(ctx) : Error.E403(ctx.HttpContext)) : Error.E401(ctx.HttpContext);
	
	/// <summary>Vartotojas turi turėti bent vieną administratoriaus rolę</summary>
	/// <param name="ctx"></param><param name="next"></param><returns></returns>
	public static async ValueTask<object?> AdminRole (EndpointFilterInvocationContext ctx, EndpointFilterDelegate next) =>
		ctx.HttpContext.GetAuth() ? (ctx.HttpContext.GetUser()?.Admin?.Count>0 ? await next(ctx) : Error.E403(ctx.HttpContext)) : Error.E401(ctx.HttpContext);

}

/// <summary>Klaidos standartinis modelis</summary>
public class Error {
	/// <summary>Klaidos kodas</summary>
	public virtual int Code { get; set; }

	private static T Respond<T>(T obj, HttpContext ctx, bool print=false) where T : Error {
		ctx.Response.StatusCode=obj.Code;
		if(print) ctx.Response.WriteAsJsonAsync(obj);
		return obj;
	}
	private static T Respond<T>(HttpContext ctx, bool print=false, params string[] str) where T : ErrorB, new() {
		var err = new T();
		foreach(var i in str) err.Details.Add(i);
		return Respond(err,ctx,print);
	}

	/// <summary>Autorizacijos klaida</summary>
	public static E401 E401(HttpContext ctx, bool print=false) => Respond(Er401,ctx,print);
	private static E401 Er401 {get;} = new();
	/// <summary>Prieigos klaida</summary>
	public static E403 E403(HttpContext ctx, bool print=false) => Respond(Er403,ctx,print);
	private static E403 Er403 { get; } = new();
	/// <summary>Nerasto resurso klaida</summary>
	public static E404 E404(HttpContext ctx, bool print=false) => Respond(Er404,ctx,print);
	private static E404 Er404 { get; } = new();

	/// <summary>Validacijos klaida</summary>
	public static E422 E422(HttpContext ctx, bool print=false, params string[] str) => Respond<E422>(ctx,print,str);
	/// <summary>kritinė klaida</summary>
	public static E500 E500(HttpContext ctx, bool print=false, params string[] str) => Respond<E500>(ctx,print,str);
}

/// <summary>Klaida su aprašymu</summary>
public class ErrorB : Error {
	
	/// <summary>Klaidos informacija</summary>
	/// <example>Validacijos informacija</example>
	public virtual List<string> Details { get; set; } = new();
}


/// <summary>Vartotojo autorizacijos klaida</summary>
public class E401 : Error {
	/// <summary>Klaidos kodas</summary>
	/// <example>401</example>
	public override int Code { get; set; } = 401;
	/// <summary>Klaidos statusas</summary>
	/// <example>Unauthorized</example>
	public string Status { get; set; } = "Unauthorized";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Reikalinga vartotojo autorizacija</example>
	public string Message { get; set; } = "Reikalinga vartotojo autorizacija";
}


/// <summary>Vartotojo prieigos klaida</summary>
public class E403 : Error {
	/// <summary>Klaidos kodas</summary>
	/// <example>403</example>
	public override int Code { get; set; } = 403;
	/// <summary>Klaidos statusas</summary>
	/// <example>Forbidden</example>
	public string Status { get; set; } = "Forbidden";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Jūs neturite prieigos prie šio resourso</example>
	public string Message { get; set; } = "Jūs neturite prieigos prie šio resourso";

}

/// <summary>Vartotojo prieigos klaida</summary>
public class E404 : Error {
	/// <summary>Klaidos kodas</summary>
	/// <example>404</example>
	public override int Code { get; set; } = 404;
	/// <summary>Klaidos statusas</summary>
	/// <example>Not Found</example>
	public string Status { get; set; } = "Not Found";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Resursas kurio ieškote neegzistuoja</example>
	public string Message { get; set; } = "Resursas kurio ieškote neegzistuoja";

}

/// <summary>Vartotojo prieigos klaida</summary>
public class E422 : ErrorB {
	/// <summary>Klaidos kodas</summary>
	/// <example>422</example>
	public override int Code { get; set; } = 422;
	/// <summary>Klaidos statusas</summary>
	/// <example>Validation Error</example>
	public string Status { get; set; } = "Validation Error";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Duomenų validacijos klaida</example>
	public string Message { get; set; } = "Duomenų validacijos klaida";
	/// <summary>Klaidos informacija</summary>
	/// <example>Validacijos informacija</example>
	public override List<string> Details { get; set; } = new();

}

/// <summary>Vartotojo prieigos klaida</summary>
public class E500 : ErrorB {
	/// <summary>Klaidos kodas</summary>
	/// <example>500</example>
	public override int Code { get; set; } = 500;
	/// <summary>Klaidos statusas</summary>
	/// <example>Server Error</example>
	public string Status { get; set; } = "Server Error";
	/// <summary>Klaidos Žinutė</summary>
	/// <example>Sistemos klaida</example>
	public string Message { get; set; } = "Sistemos klaida";
	/// <summary>Klaidos informacija</summary>
	/// <example>Validacijos informacija</example>
	public override List<string> Details { get; set; } = new();

}
