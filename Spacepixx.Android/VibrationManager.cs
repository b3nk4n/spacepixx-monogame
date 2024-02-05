using System;
using Microsoft.Devices;

namespace Spacepixx
{
    public static class VibrationManager
    {
        private static SettingsManager settings = SettingsManager.GetInstance();

        public static void Vibrate(float seconds)
        {
            // TODO use Xamarin.Essentials
            //if (settings.GetVabrationValue())
            //    VibrateController.Default.Start(TimeSpan.FromSeconds(seconds));
        }
    }
}
