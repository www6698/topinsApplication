using System.IO;
using System.Reflection;
using System.Windows;

namespace topinsApplication
{
    public partial class App : Application
    {
        private tpioSetting __iosetting;
        private Mutex __instance;
        private string __filename;

        private bool __createNew;

        public string Filename => __filename;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                string mutexName = $"{tpCONST.TOPINSTITLE}!!!!!";

                __instance = new(true, mutexName, out __createNew);

                if (__createNew)
                {
                    base.OnStartup(e);

                    tpWorkspace.Workspace.IOSetting = ConfigureSettings();

                    return;
                }
                Current.Shutdown();
            }
            catch
            {
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (__createNew) __instance?.ReleaseMutex();

            __instance?.Dispose();
            __iosetting.Dispose();

            base.OnExit(e);
        }

        private tpioSetting ConfigureSettings()
        {
            try
            {
                if (File.Exists(__filename = tpioGeneric.SetDefaultSettinsName(Assembly.GetExecutingAssembly().GetName().Name)))
                {
                    if (tpioSetting.Load(__filename) is tpioSetting setting) __iosetting = setting;
                }
                else
                {
                    tpioSetting.Save(__filename, __iosetting = new tpioSetting
                    {
                        IOGeneric = new tpioGeneric(),
                        IONetwork = new tpioNetwork(),
                        IORSerial = new tpioRSerial(),

                        CAMLens = new tpioCAMLens()
                    });
                }
                string folder = $"{Path.GetDirectoryName(__filename)}\\{tpCONST.CPFOLDER}";

                tpUtility.CheckPath(folder);

                if (tpioCAMLens.Load($"{folder}\\{__iosetting.IOGeneric.CameraVendor}") is not tpioCAMLens camera)
                {
                    __iosetting.CAMLens.SetCommand();
                    __iosetting.CAMLens.SetOSDCommand();

                    // 
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return __iosetting;
        }
    }
}
