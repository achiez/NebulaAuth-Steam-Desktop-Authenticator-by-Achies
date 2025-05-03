using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SteamLib.SteamMobile.Confirmations;

namespace NebulaAuth.Model.Entities;

public class MarketMultiConfirmation : Confirmation
{
    public ObservableCollection<MarketConfirmation> Confirmations { get; }

    public MarketMultiConfirmation(IEnumerable<MarketConfirmation> confirmations) : base(0, 0, 0, 0,
        ConfirmationType.Unknown, "")
    {
        Confirmations = new ObservableCollection<MarketConfirmation>(confirmations);
        Time = Confirmations.FirstOrDefault()?.Time ?? default;
    }
}