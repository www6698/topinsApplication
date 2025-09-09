using System.Diagnostics.CodeAnalysis;

namespace topinsApplication.Common.Events;

public delegate void TxCommandEventHandler(object sender, tpioTxCommandEventArgs e);

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
public class tpioTxCommandEventArgs(string key, tpioLensCommand command)
{
    public string Key { get; } = key;
    public tpioLensCommand Command { get; } = command;
}
