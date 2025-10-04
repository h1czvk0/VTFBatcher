using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace VTFBatcher.Views;

public partial class ConvertResultWindow : Window
{
    public ConvertResultWindow()
    {
        InitializeComponent();
    }

    public void InitInfo()
    {
        if (DataContext is ViewModels.ConvertResultWindowViewModel vm)
        {
            foreach (var error in vm.ErrorDic)
            {
                var expander = new Expander
                {
                    Header = error.Key,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
                };

                var textBlock = new TextBlock
                {
                    Text = error.Value,
                    TextWrapping = TextWrapping.Wrap,
                };

                expander.Content = textBlock;
                InfoPanel.Children.Add(expander);
            }
        }
    }
}