
using App.API;

namespace G9.Models;

/// <summary>Deklaruojamų metų sąrašas veikloms pagal ūkio subjektus</summary>
public class Veiklos {
	/// <summary>Ūkio subjektai priskirti prisijungusiam vartotojui.</summary>
	public ArrayModel<JA>? JA { get; set; }
	/// <summary>Geriamo Vandens Tiekimo sistemų sąrašas</summary>
	public ArrayModel<GVTS>? GVTS { get; set; }
	/// <summary>Deklaruojami metai</summary>
	public ArrayModelA<Deklaracija>? Deklaracijos { get; set; }
}


/// <summary>Ūkio subjekto informacija</summary>
public class JA {
	/// <summary>Unikalus ūkio subjekto numeris</summary>
	public long ID { get; set; }
	/// <summary>Ūkio subjekto pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Registruoto ūkio subjekto adresas</summary>
	public string? Addr { get; set; }
}

/// <summary>Geriamojo vandens tiekimo sistemos informacija</summary>
public class GVTS {
	/// <summary>Unikalus numeris</summary>
	public long ID { get; set; }
	/// <summary>Ūkio subjekto numeris</summary>
	public long JA { get; set; }
	/// <summary>Pavadinimas</summary>
	public string? Title { get; set; }
	/// <summary>Adresas</summary>
	public string? Addr { get; set; }
}


/// <summary>Deklaruojamų metų informacija</summary>
public class Deklaracija	{
	/// <summary>Deklaracijos ID</summary>
	public int ID { get; set; }
	/// <summary>Gerimojo vandens tiekimo sistema</summary>
	public long GVTS { get; set; }
	/// <summary>Deklaruojami metai</summary>
	public int Metai { get; set; }
	/// <summary>Deklaravimo stebėsena</summary>
	public int Stebesenos { get; set; }
	/// <summary>Deklaravimo statusas</summary>
	public int Statusas { get; set; }
	/// <summary>Deklaruojamo vandens kiekis m3/para</summary>
	public double? Kiekis { get; set; }
	/// <summary>Aptarnaujamų vartotojų skaičius</summary>
	public int? Vartotojai { get; set; }
	/// <summary>Deklaravimo data</summary>
	public DateTime? DeklarDate { get; set; }
	/// <summary>Deklaravęs vartotojas</summary>
	/// <example>Vardas Pavardė</example>
	public string? DeklarUser { get; set; }
	/// <summary>Paskutinė redagavimo data</summary>
	public DateTime? RedagDate { get; set; }
	/// <summary>Paskurinis redagavęs vartotojas</summary>
	/// <example>Vardas Pavardė</example>
	public string? RedagUser { get; set; }
}


/// <summary>Deklaravimo informacija</summary>
public class DeklaracijaSet {
	/// <summary>Deklaruojamo vandens kiekis m3/para</summary>
	public double? Kiekis { get; set; }
	/// <summary>Aptarnaujamų vartotojų skaičius</summary>
	public int? Vartotojai { get; set; }
}


/// <summary>Json masyvo modelis su Lookup reikšmėmis</summary>
/// <typeparam name="T"></typeparam>
public class ArrayModelA<T> : ArrayModel<T> {
	/// <summary>Duomenų skaitinės reikšmės</summary>
	/// <example>{"Field":{"1":"Value1","2":"Value2"}}</example>
	public Dictionary<string,Dictionary<string,string>>? Lookup { get; set; }
}

/// <summary>Json masyvo modelis</summary>
/// <typeparam name="T"></typeparam>
public class ArrayModel<T> {
	/// <summary>Duomenų aprašas (pavizdys)</summary>
	public T? ModelExample { get; set; }
	/// <summary>Duomenų laukai</summary>
	/// <example>["ID","Title"]</example>
	public List<string>? Fields { get; set; }
	/// <summary>Duomenų masyvas</summary>
	/// <example>[[1,"Data1"],[2,"Data2"]]</example>
	public List<List<object>>? Data { get; set; }
}



/// <summary>Visų galimų rodiklių sąrašas</summary>
public class RodikliuSarasas {
	/// <summary>Rodikliai</summary>
	public ArrayModelA<Rodiklis>? Rodikliai { get; set; }
	/// <summary>Dažnumo grupės</summary>
	public ArrayModelA<Daznumas>? Daznumas { get; set; }
	/// <summary>Rodiklių sąrašas stebėsenoms</summary>
	public List<Stebesenos>? Stebesenos { get; set; }
}

/// <summary>Stebėsenų informacija</summary>
public class Stebesenos {
	/// <summary>Galimų rodiklių sąrašas stebėsenoms</summary>
	public int ID { get; set; }
	/// <summary>Stebėsenų padavinimas</summary>
	public string? Pavadinimas { get; set; }
	/// <summary>Galimų rodiklių sąrašas</summary>
	public List<int>? Rodikliai { get; set; }
}


/// <summary>Rodiklio informacija</summary>
public class Rodiklis {
	/// <summary>Rodiklio unikalus numeris</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio grupė</summary>
	public int Grupe { get; set; }
	/// <summary>Rodiklio EU numeris</summary>
	public string? Kodas { get; set; }
	/// <summary>Rodiklio pavadinimas</summary>
	public string? Pavadinimas { get; set; }
	/// <summary>Minimalus tyrimų skaičius metams</summary>
	public string? Daznumas { get; set; }
	/// <summary>Mažiausia rodiklio reikšmė</summary>
	public float Min { get; set; }
	/// <summary>Didžiausia rodiklio reikšmė</summary>
	public float Max { get; set; }
	/// <summary>Reikšmės mažiausias žingsnis</summary>
	public float Step { get; set; }
	/// <summary>Matavimo vienetai</summary>
	public string? Vnt { get; set; }
	/// <summary>Aprašymas</summary>
	public string? Aprasymas { get; set; }
}

/// <summary>Rodiklių deklaravimo dažnumas metams</summary>
public class Daznumas {

}

/// <summary>Rodiklio delkaravimo reikšmė</summary>
public class RodiklioReiksme {
	/// <summary>Reikšmės identifikatorius</summary>
	/// <example>123</example>
	public long ID { get; set; }
	/// <summary>Deklaracijos identifikatorius</summary>
	/// <example>12</example>
	public int Deklaracija { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	/// <example>23</example>
	public int Rodiklis { get; set; }
	/// <summary>Mėginio paimimo data</summary>
	public DateOnly Data { get; set; }
	/// <summary>Rodiklio reikšmė</summary>
	/// <example>23.4</example>
	public double Reiksme { get; set; }
}


/// <summary>Rodiklio reikšmės įvedimas</summary>
public class RodiklisSet {
	/// <summary>Rodiklio identifikatorius</summary>
	/// <example>1</example>
	public int Rodiklis { get; set; }
	/// <summary>Mėginio paimimo data</summary>
	public DateOnly Data { get; set; }
	/// <summary>Rodiklio reikšmė</summary>
	/// <example>23.4</example>
	public double Reiksme { get; set; }
}



/// <summary>Deklaravimo pateikimas ir neatitikimų tvirtinimas</summary>
public class Deklaravimas {
	/// <summary>Deklaracijos statusas</summary>
	public string? Statusas { get; set; }
	/// <summary>Klaidos indikatorius</summary>
	public bool Klaidos { get; set; }
	/// <summary>Rodiklių suvedimo trūkumai</summary>
	public ArrayModel<ValidTrukumas>? Trukumas { get; set; }
	/// <summary>Besikartojantys rodiklių duomenys</summary>
	public ArrayModel<ValidKartojasi>? Kartojasi { get; set; }
	/// <summary>Rodiklių viršijimas</summary>
	public ArrayModel<ValidVirsijimas>? Virsijimas { get; set; }
}


/// <summary>Rodiklio reikšmių trūkumo validacija</summary>
public class ValidTrukumas {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	public int Rodiklis { get; set; }
	/// <summary>Suvestas tyrimų skaičius</summary>
	public int Suvesta { get; set; }
	/// <summary>Reikiamas tyrimų skaičius</summary>
	public int Reikia { get; set; }
	/// <summary>Deklaruojančio asmens patvirtinimas</summary>
	public bool? Patvirtinta { get; set; }
	/// <summary>Pastabos</summary>
	public string? Pastabos { get; set; }
}

/// <summary>Pasikartojančių rodmenų validacija</summary>
public class ValidKartojasi {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	public int Rodiklis { get; set; }	
	/// <summary>Suvesto rodiklio data</summary>
	public DateOnly Data { get; set; }
	/// <summary>Suvesto rodiklio reikšmė</summary>
	public double Reiksme { get; set; }
	/// <summary>Nereikšmingas viršijimas</summary>
	public bool? Patvirtinta { get; set; }
	/// <summary>String</summary>
	public string? Pastabos { get; set; }
}


/// <summary>Rodiklio viršyjimo pagrindimas</summary>
public class ValidVirsijimas {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary>Rodiklio identifikatorius</summary>
	public int Rodiklis { get; set; }
	/// <summary></summary>
	public DateOnly Nuo { get; set; }
	/// <summary></summary>
	public DateOnly Iki { get; set; }
	/// <summary>Maksimali viršyjimo reikšmė laikotarpiui</summary>
	public double Max { get; set; }
	/// <summary>Patvirtinimui reikalinga suvesti papildomą informaciją</summary>
	public bool Detales { get; set; }
	/// <summary>Nereikšmingas viršijimas</summary>
	public bool? Nereiksmingas { get; set; }
	/// <summary>Nereikšmingo viršijimo pagrindimas</summary>
	public string? NereiksmApras { get; set; }
	/// <summary>Viršijimo paveiktų žmonių skaičius</summary>
	public int? Zmones { get; set; }
	/// <summary>Mėginių ėmimo vietos tipas</summary>
	public string? Tipas { get; set; }
	/// <summary>Kiekybinio nustatymo ribos LOQ reikšmė	</summary>
	public string? LOQReiksme { get; set; }
	/// <summary>Nustatyta vertė žemiau nei LOQ</summary>
	public string? LOQVerte { get; set; }
	/// <summary>Stebėjimo statusas</summary>
	public string? Statusas { get; set; }
	/// <summary>Neatitikimas patvirtintas</summary>
	public bool Patvirtinta { get; set; }
	/// <summary>Pastabos</summary>
	public string? Pastabos { get; set; }
}


/// <summary>Deklaravimo neatitikčių patvirtinimas</summary>
public class DeklaravimasSet {	
	/// <summary>Rodiklių suvedimo trūkumai</summary>
	public List<DeklarValidTvirtinti>? Trukumas { get; set; }
	/// <summary>Besikartojantys rodiklių duomenys</summary>
	public List<DeklarValidTvirtinti>? Kartojasi { get; set; }
	/// <summary>Rodiklių viršijimas</summary>
	public List<DeklarValidVirsijimas>? Virsijimas { get; set; }

}


/// <summary>Deklaruojamo rodiklio reikšmių kartojimosi patvirtinimas</summary>
public class DeklarValidTvirtinti {
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }
	/// <summary></summary>
	public bool Patvirtinta { get; set; }
	/// <summary></summary>
	public string? Pastabos { get; set; }
}


/// <summary>Deklaruojamo rodiklio reikšmių viršijimo patvirtinimas</summary>
public class DeklarValidVirsijimas{
	/// <summary>Validacijos identifikatorius</summary>
	public int ID { get; set; }	
	/// <summary>Nereikšmingas viršijimas</summary>
	public bool? Nereiksmingas { get; set; }
	/// <summary>Nereikšmingo viršijimo pagrindimas</summary>
	public string? NereiksmApras { get; set; }
	/// <summary>Viršijimo paveiktų žmonių skaičius</summary>
	public int? Zmones { get; set; }
	/// <summary>Mėginių ėmimo vietos tipas</summary>
	public string? Tipas { get; set; }
	/// <summary>Kiekybinio nustatymo ribos LOQ reikšmė	</summary>
	public string? LOQReiksme { get; set; }
	/// <summary>Nustatyta vertė žemiau nei LOQ</summary>
	public string? LOQVerte { get; set; }
	/// <summary>Stebėjimo statusas</summary>
	public string? Statusas { get; set; }
	/// <summary></summary>
	public bool Patvirtinta { get; set; }
	/// <summary></summary>
	public string? Pastabos { get; set; }
}



/// <summary>Prisijungusio vartotojo informacija</summary>
public class Vartotojas {
	/// <summary>Vartotojo ID</summary>
	public Guid ID { get; set; }
	/// <summary>Vardas</summary>
	/// <example>Vardas</example>
	public string? FName { get; set; }
	/// <summary>Pavardė</summary>
	/// <example>Pavardė</example>
	public string? LName { get; set; }
	/// <summary>El paštas</summary>
	/// <example>info@vmvt.lt</example>
	public string? Email { get; set; }
	/// <summary>Telefonas</summary>
	/// <example>+37060000001</example>
	public string? Phone { get; set; }
	/// <summary>Juridinio asmens duomenys</summary>
	public JA? JA { get; set; }
	/// <summary>Vartotojo GVTS rolės</summary>
	public List<int>? Roles { get; set; }
	/// <summary>Administruojami GVTS</summary>
	public List<int>? Admin { get; set; }
}


/// <summary>Vandenvietės ir jų delegavimas</summary>
public class Delegavimas{
	/// <summary>Geriamo vandens tiekimo sistemos</summary>
	public ArrayModel<GVTS>? GVTS { get; set; }
	/// <summary>Deleguoti asmenys</summary>
	public ArrayModel<User>? Users { get; set; }
}

/// <summary>Deleguotas asmuo</summary>
public class User {
	/// <summary>Geriamo vandens tiekimo sistemos identifikatorius</summary>
	public long GVTS { get; set; }
	/// <summary>Deleguojamo asmens kodas</summary>
	public Guid ID { get; set; }
	/// <summary>Deleguojamo asmens vardas</summary>
	/// <example>Vardas</example>
	public string? FName { get; set; }
	/// <summary>Deleguojamo asmens pavardė</summary>
	/// <example>Pavardė</example>
	public string? LName { get; set; }
	/// <summary>Administratoriaus teisė</summary>
	public bool Admin { get; set; }
}


/// <summary>Asmens delegavimas</summary>
public class DelegavimasSet {
	/// <summary>Deleguojamo asmens kodas</summary>
	/// <example>90102031234</example>
	public long AK { get; set; }
	/// <summary>Deleguojamo asmens vardas</summary>
	/// <example>Vardas</example>
	public string? FName { get; set; }
	/// <summary>Deleguojamo asmens pavardė</summary>
	/// <example>Pavardė</example>
	public string? LName { get; set; }
	/// <summary>Administratoriaus teisė</summary>
	public bool Admin { get; set; }
}