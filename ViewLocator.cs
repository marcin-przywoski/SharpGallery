using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SharpGallery.ViewModels;
using SharpGallery.Views;

namespace SharpGallery
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    // [RequiresUnreferencedCode(
    //     "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    //     Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    public Control Build(object data)
    {
        return data switch
        {
            MainWindowViewModel vm => new MainWindow { DataContext = vm },
            _ => new TextBlock { Text = $"View not found for {data.GetType().Name}" }
        };
    }

    public bool Match(object data) => data is ViewModelBase;
}
}


