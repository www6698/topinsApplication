using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpMvvm : INotifyPropertyChanged
{
    private FrameworkElement __element;

    #region Events
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    public FrameworkElement Element
    {
        get => __element;
        set => Set(ref __element, value);
    }

    protected bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;

            OnRaisePropertyChanged(propertyName);

            return true;
        }
        return false;
    }
    protected virtual void OnRaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
