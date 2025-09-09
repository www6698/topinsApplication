using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpRelayCommand(Action<object> execute, Func<object, bool> canExecute) : ICommand
{
    private readonly Action<object> __execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<object, bool> __canExecute = canExecute;

    public tpRelayCommand(Action<object> execute) : this(execute, null) { }

    public bool CanExecute(object parameter) => __canExecute?.Invoke(parameter) ?? true;

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
    public void Execute(object parameter) => __execute(parameter);
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpRelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null) : ICommand
{
    private readonly Action<T> __execute = execute ?? throw new ArgumentNullException(nameof(execute));
    private readonly Func<T, bool> __canExecute = canExecute;

    public bool CanExecute(object parameter) => parameter is T o && (__canExecute?.Invoke(o) ?? true);

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void Execute(object parameter)
    {
        if (parameter is T typedParameter)
        {
            __execute(typedParameter);
        }
    }
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
