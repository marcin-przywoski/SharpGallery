using System;
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
        public Control? Build(object? data)
        {
            if (data is null)
                return null;

            var viewModelType = data.GetType();
            var viewTypeName = viewModelType.FullName!.Replace("ViewModel", "View");
            var viewType = Type.GetType(viewTypeName);

            if (viewType != null)
            {
                var view = (Control)Activator.CreateInstance(viewType)!;
                view.DataContext = data;
                return view;
            }

            return new TextBlock { Text = $"View not found for {viewModelType.Name}" };
        }

        public bool Match(object? data) => data is ViewModelBase;

        public static T GetViewModel<T>() where T : ViewModelBase
        {
            return ServiceLocator.GetService<T>();
        }
    }
}

