using System.Collections.ObjectModel;

namespace Nova.Avalonia.UI.Gallery.ViewModels;

public sealed record SampleCategory(string Name, ObservableCollection<NavigationSample> Samples);
