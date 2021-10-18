using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Windows.Forms;

namespace MysteryMaker
{
    public class Updater
    {
        public static assets.forms.Splash splashForm;


        public static ApplicationDeployment currentDeployment = ApplicationDeployment.CurrentDeployment;


        public String CheckForUpdates()
        {
            Globals.addToLogs("Check for Updates...");
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                return "Error while updating: Debug mode is active.";
            }

            UpdateCheckInfo info;
            try
            {
                info = currentDeployment.CheckForDetailedUpdate();
            }
            catch (Exception)
            {
                return "Error while updating: Update information could not be load. Check you internet connection.";
            }

            if (!info.UpdateAvailable)
            {
                return "Already up to date.";
            }

            if (splashForm != null)
                splashForm.setStatus("Updating...");

            Properties.Settings.Default.seenUpdateLog = false;
            Properties.Settings.Default.Save();

            MessageBox.Show("A new version is available! MysteryMaker will be updated to version: " + info.AvailableVersion, "Update", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);


            currentDeployment.Update();

            MessageBox.Show("Update was installed successfully. Please restart MysteryMaker for the changes to take effect.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            return "Updated!";
        }
    }
}
