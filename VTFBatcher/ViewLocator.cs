using Avalonia.Controls;
using Avalonia.Controls.Templates;
using VTFBatcher.ViewModels;
using VTFBatcher.Views;

namespace VTFBatcher;

public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        return data switch
        {
            MainWindowViewModel => new MainWindow(),
            ConvertResultWindowViewModel => new ConvertResultWindow(),
            _ => new TextBlock { Text = "Not Found: " + (data?.GetType().Name ?? "null") }
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
