using System;
using System.Windows.Forms;

namespace MysteryMaker
{
    public class Updater
    {
        public static assets.forms.Splash splashForm;

        public String CheckForUpdates()
        {
            Globals.addToLogs("Prüfe auf Updates...");
            if (!System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                return "Fehler beim Updaten: DebugMode ist aktiviert.";
            }

            var currentDeployment = System.Deployment.Application.ApplicationDeployment.CurrentDeployment;

            System.Deployment.Application.UpdateCheckInfo info;
            try
            {
                info = currentDeployment.CheckForDetailedUpdate();
            }
            catch (Exception)
            {
                return "Fehler beim Updaten: Update-Infos konnten nicht geladen werden. Überprüfen sie ihre Internetverbindung.";
            }

            if (!info.UpdateAvailable)
            {
                return "Bereits auf dem neusten Stand.";
            }

            if (splashForm != null)
                splashForm.setStatus("Updating...");

            Properties.Settings.Default.seenUpdateLog = false;
            Properties.Settings.Default.Save();

            MessageBox.Show("Eine neue Version ist erschienen! Der Client wird nun auf die Version " + info.AvailableVersion + " aktualisiert.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);


            currentDeployment.Update();
            Application.Restart();
            return "Erfolgreich aktualisiert!";
        }
    }
}
