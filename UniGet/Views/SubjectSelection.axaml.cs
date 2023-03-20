using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using UniGet.Models;
using UniGet.ViewModels;

namespace UniGet.Views;

public partial class SubjectSelection : Window
{
    public SubjectSelection()
    {
        DataContext = new SubjectSelectionViewModel();
        InitializeComponent();
    }
    public SubjectSelection(ObservableCollection<SubjectNode> subjectNodes)
    {
        DataContext = new SubjectSelectionViewModel(subjectNodes);
        InitializeComponent();
    }
}