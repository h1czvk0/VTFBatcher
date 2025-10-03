using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VTFBatcher.ViewModels;

public partial class ConvertResultWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _conclusion = "Sample Conclusion";

    public Dictionary<string, string> ErrorDic = new();
    
    public void InitInfo(string conclusion, Dictionary<string, string> errorDic)
    {
        Conclusion = conclusion;
        ErrorDic = errorDic;
    }
}