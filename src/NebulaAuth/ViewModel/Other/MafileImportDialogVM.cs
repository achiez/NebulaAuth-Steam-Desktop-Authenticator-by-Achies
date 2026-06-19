using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NebulaAuth.ViewModel.Other;

public partial class MafileImportDialogVM : ObservableObject
{
    public ObservableCollection<string> Groups { get; }
    public int TotalCount { get; }
    public int ConflictCount { get; }
    public bool HasConflicts => ConflictCount > 0;
    [ObservableProperty] private bool _addToGroup;
    [ObservableProperty] private string? _group;
    [ObservableProperty] private bool _overwriteConflicts;

    public MafileImportDialogVM(IEnumerable<string> groups, int totalCount, int conflictCount)
    {
        Groups = new ObservableCollection<string>(groups);
        TotalCount = totalCount;
        ConflictCount = conflictCount;
    }
}

public record MafileImportDialogResult(string? Group, bool OverwriteConflicts);