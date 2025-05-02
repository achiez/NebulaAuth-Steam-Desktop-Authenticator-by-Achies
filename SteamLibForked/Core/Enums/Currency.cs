// ReSharper disable InconsistentNaming

namespace SteamLib.Core.Enums;

public enum Currency
{
    None = 0,
    USD = 1,
    GBP = 2,
    EUR = 3,
    CHF = 4,
    RUB = 5,
    PLN = 6,
    BRL = 7,
    JPY = 8,
    NOK = 9,
    IDR = 10,
    MYR = 11,
    PHP = 12,
    SGD = 13,
    THB = 14,
    VND = 15,
    KRW = 16,
    TRY = 17,
    UAH = 18,
    MXN = 19,
    CAD = 20,
    AUD = 21,
    NZD = 22,
    CNY = 23,
    INR = 24,
    CLP = 25,
    PEN = 26,
    COP = 27,
    ZAR = 28,
    HKD = 29,
    TWD = 30,
    SAR = 31,
    AED = 32,
    ARS = 34,
    ILS = 35,
    KZT = 37,
    KWD = 38,
    QAR = 39,
    CRC = 40,
    UYU = 41
}

public static class CurrencyInfo
{
    public static IReadOnlyDictionary<Currency, string> CurrencySymbols { get; } = new Dictionary<Currency, string>
    {
        {Currency.USD, "$"},
        {Currency.GBP, "£"},
        {Currency.EUR, "€"},
        {Currency.CHF, "CHF"},
        {Currency.RUB, "pуб."},
        {Currency.PLN, "zł"},
        {Currency.BRL, "R$"},
        {Currency.JPY, "¥"},
        {Currency.NOK, "kr"},
        {Currency.IDR, "Rp"},
        {Currency.MYR, "RM"},
        {Currency.PHP, "P"},
        {Currency.SGD, "S$"},
        {Currency.THB, "฿"},
        {Currency.VND, "₫"},
        {Currency.KRW, "₩"},
        {Currency.TRY, "TL"},
        {Currency.UAH, "₴"},
        {Currency.MXN, "Mex$"},
        {Currency.CAD, "CDN$"},
        {Currency.AUD, "A$"},
        {Currency.NZD, "NZ$"},
        {Currency.CNY, "¥"},
        {Currency.INR, "₹"},
        {Currency.CLP, "CLP$"},
        {Currency.PEN, "S/."},
        {Currency.COP, "COL$"},
        {Currency.ZAR, "R"},
        {Currency.HKD, "HK$"},
        {Currency.TWD, "NT$"},
        {Currency.SAR, "SR"},
        {Currency.AED, "AED"},
        {Currency.ARS, "ARS$"},
        {Currency.ILS, "₪"},
        {Currency.KZT, "₸"},
        {Currency.KWD, "KD"},
        {Currency.QAR, "QR"},
        {Currency.CRC, "₡"},
        {Currency.UYU, "$U"}
    };

    public static int ToInt(this Currency currency)
    {
        return (int) currency;
    }

    public static string ToIntString(this Currency currency)
    {
        return currency.ToInt().ToString();
    }
}