using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Linker;

public partial class DesignLinkAccountAuthStepVM : ObservableObject
{
    [ObservableProperty] private LinkAccountStepVM _currentStep;

    public DesignLinkAccountAuthStepVM()
    {
        CurrentStep = new LinkAccountDoneStepVM("R5555", "76561199680782345");
    }
}