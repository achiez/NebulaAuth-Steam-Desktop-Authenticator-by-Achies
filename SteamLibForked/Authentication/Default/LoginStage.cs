using AchiesUtilities.Web.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamLib.Authentication;
using SteamLib.Utility;

namespace SteamLib.Login.Default;

internal abstract class LoginStage
{
    protected Dictionary<string, string> Data { get; }
    protected LoginStage()
    {
        Data = new Dictionary<string, string>();
    }

    protected LoginStage(Dictionary<string, string> data)
    {
        Data = data;
    }

    protected bool CheckIfLoginCompleted(LoginResultJson json)
    {
        return !json.CaptchaNeeded && !json.EmailAuthNeeded && !json.RequiresTwoFactor && json.Success;
    }
    protected async Task<LoginResultJson> ConvertJson(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var webResponseString = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<LoginResultJson>(webResponseString)!;
    }
    protected async Task<LoginSuccessStage> CompleteLogin(HttpResponseMessage response)
    {
        var jsonStr = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(jsonStr);
        var transferParameters = json["transfer_parameters"]!.ToObject<TransferParameters>()!;
        return new LoginSuccessStage(transferParameters);
    }
    protected async Task<HttpResponseMessage> LoginRequest(HttpClient client, CancellationToken cancellationToken = default)
    {
        return await client.PostAsync("https://steamcommunity.com/login/dologin", new FormUrlEncodedContent(Data), cancellationToken);
    }
}
internal class LoginErrorStage : LoginStage
{
    public readonly string ErrorString;
    public readonly int ErrorCode;
    public readonly LoginStage Stage;
    public readonly HttpResponseMessage Response;
    public LoginErrorStage(string errorString, HttpResponseMessage responseMessage, LoginStage stage)
    {
        ErrorString = errorString;
        Stage = stage;
        ErrorCode = Utilities.GetSuccessCode(responseMessage.Content.ReadAsStringSync());
        Response = responseMessage;
    }
}
internal class GetRsaStage : LoginStage
{
    public GetRsaStage(Dictionary<string, string> data) : base(data) { }
    public GetRsaStage(string userName)
    {
        Data["username"] = userName;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="cancellationToken"></param>
    /// <returns> <see cref="ReadyToLoginStage"/> if success</returns>
    public async Task<LoginStage> Proceed(HttpClient client, CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(Data);
        var response = await client.PostAsync("https://steamcommunity.com/login/getrsakey", content, cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var rsaJson = JsonConvert.DeserializeObject<RsaKeyJson>(responseString)!;
        if (!rsaJson.success)
            return new LoginErrorStage("Can't get RSA", response, this);

        Data["rsatimestamp"] = rsaJson.timestamp;
        return new ReadyToLoginStage(Data, rsaJson.publickey_exp, rsaJson.publickey_mod);
    }

}
internal class ReadyToLoginStage : LoginStage
{
    private readonly string _publicKeyExp;
    private readonly string _publicKeyMod;
    internal ReadyToLoginStage(Dictionary<string, string> data, string publicKeyExp, string publicKeyMod) : base(data)
    {
        _publicKeyExp = publicKeyExp;
        _publicKeyMod = publicKeyMod;
    }

    public async Task<LoginStage> Proceed(HttpClient client, string password, string loginFriendlyName, CancellationToken cancellationToken = default)
    {
        var encryptedBase64Password = EncryptionHelper.ToBase64EncryptedPassword(_publicKeyExp, _publicKeyMod, password);
        var unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        Data["password"] = encryptedBase64Password;
        Data["remember_login"] = "true";
        Data["loginfriendlyname"] = loginFriendlyName;
        Data["donotcache"] = unixTimestamp + "000"; // Added three "0"'s because Steam has a weird unix timestamp interpretation.

        _ = await client.GetAsync("https://steamcommunity.com/", cancellationToken);
        var webResponse = await LoginRequest(client, cancellationToken);
        var result = await ConvertJson(webResponse);

        if (CheckIfLoginCompleted(result))
        {
            return await CompleteLogin(webResponse);
        }

        if (result.CaptchaNeeded)
        {
            return new CaptchaNeededStage(Data, result.CaptchaGid);
        }

        if (result.EmailAuthNeeded)
        {
            return new EmailAuthStage(Data);
        }

        if (result.RequiresTwoFactor)
        {
            return new TwoFactorStage(Data);
        }

        return new LoginErrorStage("Can't proceed login.", webResponse, this);
    }
}
internal class TwoFactorStage : LoginStage
{
    internal TwoFactorStage(Dictionary<string, string> data) : base(data) { }
    public async Task<LoginStage> Proceed(HttpClient client, string twoFactorCode, CancellationToken cancellationToken = default)
    {
        Data["twofactorcode"] = twoFactorCode;
        var webResponse = await LoginRequest(client, cancellationToken);
        var result = await ConvertJson(webResponse);

        if (CheckIfLoginCompleted(result))
        {
            return await CompleteLogin(webResponse);
        }

        if (result.CaptchaNeeded)
        {
            return new CaptchaNeededStage(Data, result.CaptchaGid);
        }

        if (result.RequiresTwoFactor)
        {
            return new TwoFactorStage(Data);
        }

        return new LoginErrorStage("Can't proceed login. Bad TwoFactor code", webResponse, this);
    }

}
internal class EmailAuthStage : LoginStage
{
    internal EmailAuthStage(Dictionary<string, string> data) : base(data) { }

    public async Task<LoginStage> Proceed(HttpClient client, string emailCode, CancellationToken cancellationToken = default)
    {
        Data["emailauth"] = emailCode;
        var webResponse = await LoginRequest(client, cancellationToken);
        var result = await ConvertJson(webResponse);

        if (CheckIfLoginCompleted(result))
        {
            return await CompleteLogin(webResponse);
        }

        if (result.CaptchaNeeded)
        {
            return new CaptchaNeededStage(Data, result.CaptchaGid);
        }

        if (result.EmailAuthNeeded)
        {
            return new EmailAuthStage(Data);
        }


        return new LoginErrorStage("Can't proceed login. Bad Email code", webResponse, this);
    }
}
internal class CaptchaNeededStage : LoginStage
{
    internal Uri CaptchaImage { get; }
    public CaptchaNeededStage(Dictionary<string, string> data, string captchaGid) : base(data)
    {
        captchaGid = Uri.EscapeDataString(captchaGid);
        Data["captchagid"] = captchaGid;
        CaptchaImage = new Uri("https://steamcommunity.com/login/rendercaptcha?gid=" + captchaGid);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="captchaText"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="ReadyToLoginStage"/> or <see cref="LoginErrorStage"/></returns>
    public async Task<LoginStage> Proceed(HttpClient client, string captchaText, CancellationToken cancellationToken = default)
    {
        Data["captcha_text"] = captchaText;
        //When captcha required we need to do login from start

        var rsaStage = new GetRsaStage(Data);
        var res = await rsaStage.Proceed(client, cancellationToken);

        return res;
    }
}
internal class LoginSuccessStage : LoginStage
{
    public TransferParameters TransferParameters { get; }
    public LoginSuccessStage(TransferParameters transferParameters)
    {
        TransferParameters = transferParameters;
    }


}