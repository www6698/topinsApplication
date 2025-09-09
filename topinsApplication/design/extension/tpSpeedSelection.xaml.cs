using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using topinsApplication.Common.Events;

namespace topinsApplication;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public partial class tpSpeedSelection : UserControl
{
    public tpSpeedSelection()
    {
        InitializeComponent();

        DataContext = tpWorkspace.Workspace.Content.ControlPad.SpeedSelection;
    }

    public new static readonly DependencyProperty IsFocusedProperty = DependencyProperty.Register(nameof(IsFocused), typeof(bool), typeof(tpSpeedSelection), new PropertyMetadata(false));
    public new bool IsFocused
    {
        get => (bool)GetValue(IsFocusedProperty);
        set => SetValue(IsFocusedProperty, value);
    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpSpeedSelectionMvvm : tpMvvm
{
    private tpioSetting __iosetting;
    private Visibility __isVisible = Visibility.Collapsed;
    private string __content;

    private readonly ObservableCollection<tpSpeedSelectionItem> __speedSelectionItems = [];
    private tpSpeedSelectionItem __selectedItem;

    private bool __isEnabled;

    #region Events
    public event EventHandler<tpioTxCommandEventArgs> TxCommand;
    #endregion

    #region tpRelayCommand
    private tpRelayCommand __resetCommand;
    private tpRelayCommand __transmitCommand;
    #endregion

    public tpSpeedSelectionMvvm()
    {
        PreventEventBubbling = new tpRelayCommand<MouseButtonEventArgs>(OnPreventEventBubbling);

        //__speedSelectionItems = [];
    }

    #region ICommand
    public ICommand PreventEventBubbling { get; }

    public ICommand Reset => __resetCommand ??= new tpRelayCommand(OnReset);
    public ICommand Transmit => __transmitCommand ??= new tpRelayCommand(OnTransmit);
    #endregion

    public tpioSetting IOSetting
    {
        get => __iosetting;
        set => Set(ref __iosetting, value);
    }

    public ObservableCollection<tpSpeedSelectionItem> SpeedSelectionItems => __speedSelectionItems;
    public string Content
    {
        get => __content;
        set
        {
            if (Set(ref __content, value))
            {
#if DEBUG && FORDEBUG
                System.Diagnostics.Debug.WriteLine($"       __content :{__content}");
#endif
                SelectedItem = null;

                __speedSelectionItems.Clear();

                if (string.IsNullOrEmpty(__content))
                {
                    IsVisible = Visibility.Collapsed;
                }
                else
                {
                    SetSpeedSelectionItems();

                    IsVisible = Visibility.Visible;
                }
            }
        }
    }

    public Visibility IsVisible
    {
        get => __isVisible;
        set => Set(ref __isVisible, value);
    }

    public tpSpeedSelectionItem SelectedItem
    {
        get => __selectedItem;
        set => Set(ref __selectedItem, value);
    }

    public bool IsEnabled
    {
        get => __isEnabled;
        set => Set(ref __isEnabled, value);
    }

    private void SetSpeedSelectionItems()
    {
        foreach (var command in __iosetting.CAMLens.Command)
        {
            if (__content.Equals(command.Value.Operation))
            {
                __speedSelectionItems.Add(new tpSpeedSelectionItem
                {
                    Name = command.Value.Description,
                    Value = command.Value
                });
#if DEBUG && !FORDEBUG
                System.Diagnostics.Debug.WriteLine($"       command.Value.Feature :{command.Value.Feature}");
#endif
            }
        }
    }

    private void ClearSpeedSelectionItems()
    {
        __speedSelectionItems.Clear();

        SelectedItem = null;
    }

    public void OnPreventEventBubbling(MouseButtonEventArgs e)
    {
        //e.Handled = true;
        //e.Handled = false;
    }

    private void OnReset(object parameter)
    {

    }
    private void OnTransmit(object parameter)
    {

    }
}

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpSpeedSelectionItem
{
    public string Name { get; set; }
    public tpioLensCommand Value { get; set; }
}

