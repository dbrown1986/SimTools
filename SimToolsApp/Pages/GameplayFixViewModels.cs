// SimTools
// Main Application
// SimTools - Model for Gameplay Fixes Page
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SimTools;

// ══════════════════════════════════════════════════════════════════════════════
//  Data record — in its own file so the XAML compiler can resolve the types
//  referenced via {x:Type local:GameplayFixViewModel} / GameplaySectionViewModel
// ══════════════════════════════════════════════════════════════════════════════

public sealed record GameplayFixItem(
    string DisplayName,
    string FileName,
    string Url,
    string OnCheckedMessage = "");

// ══════════════════════════════════════════════════════════════════════════════
//  Item ViewModel
// ══════════════════════════════════════════════════════════════════════════════
public sealed class GameplayFixViewModel : INotifyPropertyChanged
{
    public string DisplayName      { get; }
    public string FileName         { get; }
    public string Url              { get; }
    public string OnCheckedMessage { get; }

    /// <summary>True for real items; false for TBD placeholders (empty FileName).</summary>
    public bool IsActive => !string.IsNullOrEmpty(FileName);

    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value) return;
            _isChecked = value;
            OnPropertyChanged();

            if (value && !string.IsNullOrEmpty(OnCheckedMessage))
                CheckedMessageRequested?.Invoke(this, OnCheckedMessage);
        }
    }

    public event EventHandler<string>? CheckedMessageRequested;

    public GameplayFixViewModel(GameplayFixItem item)
    {
        DisplayName      = item.DisplayName;
        FileName         = item.FileName;
        Url              = item.Url;
        OnCheckedMessage = item.OnCheckedMessage;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

// ══════════════════════════════════════════════════════════════════════════════
//  Section ViewModel
// ══════════════════════════════════════════════════════════════════════════════
public sealed class GameplaySectionViewModel : INotifyPropertyChanged
{
    public string                                     Header { get; }
    public ObservableCollection<GameplayFixViewModel> Items  { get; }
    public ICommand                                   ToggleAllCommand { get; }

    /// <summary>
    /// Drives the section-header three-state checkbox (OneWay binding).
    /// Considers only active (non-TBD) items.
    /// </summary>
    public bool? IsAllSelected
    {
        get
        {
            var active = Items.Where(i => i.IsActive).ToList();
            if (active.Count == 0) return false;
            bool any = active.Any(i => i.IsChecked);
            bool all = active.All(i => i.IsChecked);
            return all ? true : any ? null : false;
        }
    }

    public GameplaySectionViewModel(string header, IEnumerable<GameplayFixViewModel> items)
    {
        Header = header;
        Items  = new ObservableCollection<GameplayFixViewModel>(items);

        ToggleAllCommand = new RelayCommand(_ =>
        {
            var active = Items.Where(i => i.IsActive).ToList();
            bool check = active.Any(i => !i.IsChecked);
            foreach (var item in active)
                item.IsChecked = check;
        });

        foreach (var item in Items)
            item.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(GameplayFixViewModel.IsChecked))
                    OnPropertyChanged(nameof(IsAllSelected));
            };
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
