using Android.App;
using Android.Runtime;

namespace POCFederacionCafeteros.Platforms.Android
{
  [Application]
  public class MainApplication : MauiApplication
  {
    public MainApplication(nint handle, JniHandleOwnership ownership)
      : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
  }
}
