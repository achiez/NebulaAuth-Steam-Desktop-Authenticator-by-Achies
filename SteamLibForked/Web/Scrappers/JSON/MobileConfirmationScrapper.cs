using Newtonsoft.Json;
using SteamLib.Exceptions;
using SteamLib.Exceptions.General;
using SteamLib.SteamMobile.Confirmations;
using SteamLib.Web.Models.Mobile;

namespace SteamLib.Web.Scrappers.JSON;

public static class MobileConfirmationScrapper
{
    public static Dictionary<string, LoadConfirmationsError> ErrorMessages { get; } = new()
    {
        {"Oh nooooooes!", LoadConfirmationsError.TryAgainLater},
        {"You are not set up to receive mobile confirmations", LoadConfirmationsError.NotSetupToReceiveConfirmations}
    };

    public static List<Confirmation> Scrap(string response)
    {
        ConfirmationsJson conf;
        try
        {
            conf = JsonConvert.DeserializeObject<ConfirmationsJson>(response, StaticJson.DefaultSettings)!;
        }
        catch (Exception ex)
        {
            throw new UnsupportedResponseException(response, ex);
        }

        if (conf.NeedAuth)
        {
            throw new SessionExpiredException();
        }

        if (conf is {Success: false, Message: not null})
        {
            var error = LoadConfirmationsError.Unknown;
            if (ErrorMessages.TryGetValue(conf.Message, out var e))
            {
                error = e;
            }

            throw new CantLoadConfirmationsException(conf.Message)
            {
                Error = error
            };

        }

        if (conf.Success == false)
        {
            throw new UnsupportedResponseException(response);
        }


        var result = new List<Confirmation>();
        if (conf.Conf.Count == 0) return result;


        foreach (var confirmationJson in conf.Conf)
        {
            result.Add(GetConcrete(confirmationJson));
        }

        return result;
    }


    private static Confirmation GetConcrete(ConfirmationJson json)
    {
        return json.Type switch
        {
            ConfirmationType.Trade => GetTradeConfirmation(json),
            ConfirmationType.AccountRecovery => GetAccountRecoveryConfirmation(json),
            ConfirmationType.MarketSellTransaction => GetMarketConfirmation(json),
            ConfirmationType.RegisterApiKey => GetRegisterApiKeyConfirmation(json),
            _ => new Confirmation(json.Id, json.Nonce, (int)json.Type, json.CreatorId, json.Type, json.TypeName)
            {
                Time = json.CreationTime.ToLocalDateTime()
            },
        };
    }

    private static TradeConfirmation GetTradeConfirmation(ConfirmationJson json)
    {

        var avatarUri = json.Icon;

        var userName = json.Headline;

        
        var receiveNodeText = string.Empty;
        if (json.Summary.Count > 0)
        {
            receiveNodeText = json.Summary[1];
        }

        var receiveNothing = receiveNodeText == "You will receive nothing";


        return new TradeConfirmation(json.Id, json.Nonce, json.CreatorId, json.TypeName)
        {
            UserAvatarUri = avatarUri!.ToString(),
            UserName = userName,
            IsReceiveNothing = receiveNothing,
            Time = json.CreationTime.ToLocalDateTime()
        };
    }

    private static AccountRecoveryConfirmation GetAccountRecoveryConfirmation(ConfirmationJson confirmation)
    {
        return new AccountRecoveryConfirmation(confirmation.Id, confirmation.Nonce, confirmation.CreatorId, confirmation.TypeName)
        {
            Time = confirmation.CreationTime.ToLocalDateTime()
        };
    }
    private static RegisterApiKeyConfirmation GetRegisterApiKeyConfirmation(ConfirmationJson confirmation)
    {
        return new RegisterApiKeyConfirmation(confirmation.Id, confirmation.Nonce, confirmation.CreatorId, confirmation.TypeName)
        {
            Time = confirmation.CreationTime.ToLocalDateTime()
        };
    }

    private static MarketConfirmation GetMarketConfirmation(ConfirmationJson confirmation) //TODO: test this
    {

        var itemName = confirmation.Headline;
        var itemImageUri = confirmation.Icon?.ToString() ?? string.Empty;
        return new MarketConfirmation(confirmation.Id, confirmation.Nonce, confirmation.CreatorId, confirmation.TypeName)
        {
            Time = confirmation.CreationTime.ToLocalDateTime(),
            ItemImageUri = itemImageUri,
            ItemName = itemName,
            PriceString = string.Empty //TODO:
        };
    }
}