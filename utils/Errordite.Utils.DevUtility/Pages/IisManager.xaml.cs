using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Errordite.Utils.DevUtility.Entities;
using Microsoft.Web.Administration;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using Site = Microsoft.Web.Administration.Site;

namespace Errordite.Utils.DevUtility.Pages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class IisManager : Page
    {
        private IList<Entities.Site> _sites;
        private IList<Repository> _repos;
        private readonly ServerManager _serverManager;
        private readonly X509Store _x509Store;

        public IisManager()
        {
            InitializeComponent();

            _serverManager = new ServerManager();
            _x509Store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            _uiRepos.LoadingRow +=UiReposLoadingRow;
            _uiRepos.SelectionChanged += _uiRepos_SelectionChanged;

            RefreshData();
        }

        void _uiRepos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var repo = (Repository) e.AddedItems[0];
            }
        }

        private void RefreshData()
        {
            _sites = GetSites();

            var site = _sites.FirstOrDefault(s => s.Name == SiteName);
            var adminSite = _sites.FirstOrDefault(s => s.Name == ReceptionSiteName);

            _repos = App.GetRepositories();

            _uiSites.ItemsSource = _sites;
            _uiRepos.ItemsSource = _repos;

            var appPools = GetAppPools();
            _uiSiteAppPool.ItemsSource = appPools;
            _uiAdminSiteAppPool.ItemsSource = appPools;

            if (site != null)
                _uiSiteAppPool.SelectedItem = appPools.First(ap => ap.Name == site.AppPoolName);
            else
                _uiSiteAppPool.SelectedIndex = 0;

            _uiSiteAppPool.DisplayMemberPath = "Name";

            if (adminSite != null)
                _uiAdminSiteAppPool.SelectedItem = appPools.First(ap => ap.Name == adminSite.AppPoolName);
            else
                _uiAdminSiteAppPool.SelectedIndex = 0;

            _uiAdminSiteAppPool.DisplayMemberPath = "Name";

            _uiCerts.ItemsSource = GetCerts();
            _uiCerts.SelectedIndex = 0;
            _uiCerts.DisplayMemberPath = "Name";
        }
        
        private void UiReposLoadingRow(object sender, DataGridRowEventArgs e)
        {
            var item = e.Row.Item as Repository;

            if (item != null )
            {
                var adminSite =
                    _sites.FirstOrDefault(s => s.Name.Equals(ReceptionSiteName, StringComparison.InvariantCultureIgnoreCase));
                var site =
                    _sites.FirstOrDefault(s => s.Name.Equals(SiteName, StringComparison.InvariantCultureIgnoreCase));

                if (adminSite == null)
                {
                    MessageBox.Show("Your IIS configuration is invalid, please create the admin site in IIS named dev-marketplace-admin.asos.com");
                    System.Windows.Application.Current.Shutdown(0);
                    return;
                }

                if (site == null)
                {
                    MessageBox.Show("Your IIS configuration is invalid, please create the web site in IIS named dev-marketplace.asos.com");
                    System.Windows.Application.Current.Shutdown(0);
                    return;
                }

                if (adminSite.HomeDirectory.StartsWith(item.LocalLocation + @"\") && site.HomeDirectory.StartsWith(item.LocalLocation + @"\"))
                {
                    item.CurrentForIis = true;
                    e.Row.Background = new SolidColorBrush(Colors.LightGreen);
                }

                e.Row.VerticalAlignment = VerticalAlignment.Center;
                e.Row.VerticalContentAlignment = VerticalAlignment.Center;
            }
        }

        private static IList<Entities.Site> GetSites()
        {
            var serverManager = new ServerManager();

            Func<Site, string, string> getBindingInfo = (site, protocol) =>
            {
                var binding = site.Bindings.FirstOrDefault(b => b.Protocol == protocol);
                return binding == null ? null : binding.BindingInformation;
            };

            var sites = serverManager.Sites.Where(
                s => s.Name.Contains("errordite"))
                .Select(s => new Entities.Site
                { 
                    Name = s.Name, 
                    HomeDirectory = s.Applications.First(a => a.Path == "/")
                .VirtualDirectories.First(vd => vd.Path == "/").PhysicalPath,
                HttpsBinding = getBindingInfo(s, "https"),
                HttpBinding = getBindingInfo(s, "http"),
                AppPoolName = s.Applications.First(a => a.Path == "/").ApplicationPoolName}).ToList();

            return sites;
        }

        private IEnumerable<Cert> GetCerts()
        {
            _x509Store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadWrite);
            var certs =
                _x509Store.Certificates.Cast<X509Certificate2>().Select(
                    c => new Cert() {Name = c.FriendlyName, Bytes = c.GetCertHash()}).ToList();
            _x509Store.Close();
            return certs;
        }

        private IEnumerable<AppPool> GetAppPools()
        {
            return _serverManager.ApplicationPools.Select(ap => new AppPool() {Name = ap.Name}).OrderBy(a => a.Name).ToList();
        }

        private void ConfigureIisClick(object sender, RoutedEventArgs e)
        {
            ConfigureIis(((Repository)_uiRepos.SelectedItem) ?? _repos.FirstOrDefault(r => r.ContainsThisApp));
        }

        private void ConfigureIis(Repository repository)
        {
            string httpsBindingInfo = string.Format("*:{0}:", _uiSitePort.Text);
            string adminHttpsBindingInfo = string.Format("*:{0}:", _uiAdminSitePort.Text);
            string httpBindingInfo = string.Format("*:80:{0}", SiteName);
            string adminHttpBindingInfo = string.Format("*:80:{0}", ReceptionSiteName);
            
            RemoveBindings(httpBindingInfo, httpsBindingInfo, adminHttpBindingInfo, adminHttpsBindingInfo);

            var site = _serverManager.Sites.FirstOrDefault(s => s.Name == SiteName);
            var receptionSite = _serverManager.Sites.FirstOrDefault(s => s.Name == ReceptionSiteName);

            ConfigureSite(ref site, SiteName, httpBindingInfo, httpsBindingInfo, repository, "Errordite.Web", _uiSiteAppPool);
            ConfigureSite(ref receptionSite, ReceptionSiteName, adminHttpBindingInfo, adminHttpsBindingInfo, repository, "Errordite.Reception.Web", _uiAdminSiteAppPool);

            _serverManager.CommitChanges();

            _sites = GetSites();
            _uiSites.ItemsSource = _sites;
            _uiSites.Items.Refresh();
            _uiRepos.Items.Refresh();
        }

        private void ConfigureSite(ref Site site, string name, string httpBindingInfo, string httpsBindingInfo, Repository repository, string projectDir, ComboBox uiAppPool)
        {
            string physicalPath = Path.Combine(repository.LocalLocation, "core", projectDir);

            if (site == null)
            {
                site = _serverManager.Sites.Add(name, "http", httpBindingInfo, physicalPath);
            }
            else
            {
                site.Bindings.Add(httpBindingInfo, "http");
                site.Applications.First(a => a.Path == "/").VirtualDirectories.First(a => a.Path == "/").PhysicalPath = physicalPath;
            }

            site.Applications.First().ApplicationPoolName = ((AppPool)uiAppPool.SelectedItem).Name;
            site.Bindings.Add(httpsBindingInfo, ((Cert)_uiCerts.SelectedItem).Bytes, _x509Store.Name);
        }

        private void RemoveBindings(params string[] bindingInformations)
        {
            foreach (var existingSite in _serverManager.Sites)
            {
                for (int ii = existingSite.Bindings.Count - 1; ii >= 0; ii--)
                {
                    var binding = existingSite.Bindings[ii];
                    if (bindingInformations.Contains(binding.BindingInformation))
                        existingSite.Bindings.Remove(binding);
                }
            }
        }

        protected string ReceptionSiteName
        {
            get { return _uiAdminHostName.Text; }
        }

        protected string SiteName
        {
            get { return _uiSiteHostName.Text; }
        }


        private void UiRefreshButtonClick(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }
    }
}
