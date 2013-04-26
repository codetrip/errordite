using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Management;
using System.Windows.Media;
using CodeTrip.Core.Extensions;
using Errordite.Utils.DevUtility.Configuration;
using Errordite.Utils.DevUtility.Entities;
using Microsoft.Win32;

namespace Errordite.Utils.DevUtility.Pages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ServiceManager : Page
    {
        public ServiceManager()
        {
            InitializeComponent();
            RefreshScreen();
            RunningServices.LoadingRow += RunningServicesLoadingRow;
        }

        #region Event Handling

        private void UninstallAllClick(object sender, RoutedEventArgs e)
        {
            RunningServices.SelectAll();
            BtnUninstallClick(sender, e);
        }

        private void BtnStartAllClick(object sender, RoutedEventArgs e)
        {
            RunningServices.SelectAll();
            BtnStartClick(sender, e);
        }

        private void BtnStopAllClick(object sender, RoutedEventArgs e)
        {
            RunningServices.SelectAll();
            BtnStopClick(sender, e);
        }

        private static void RunningServicesLoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.VerticalAlignment = VerticalAlignment.Center;
            e.Row.VerticalContentAlignment = VerticalAlignment.Center;

            var svc = e.Row.Item as Service;

            if (svc != null)
            {
                if(svc.Status == "Running")
                    e.Row.Background = new SolidColorBrush(Colors.LightGreen);
            }
        }

        private void BtnInstallClick(object sender, RoutedEventArgs e)
        {
            var services = GetSelectedAvailableServices();
            if (services != null)
            {
                foreach (var svc in services)
                {
                    var selectedItem = RepositoryList.SelectedItem as ComboBoxItem;

                    if(selectedItem == null)
                    {
                        MessageBox.Show("Please select a repository to install the service from.");
                        return;
                    }

                    string repositoryName;
                    if (IsServiceInstalled(svc.Name, out repositoryName))
                        Uninstall(svc, repositoryName);

                    Install(svc, selectedItem.Tag.ToString());
                }
            }
            RefreshScreen();
        }

        private void BtnUninstallClick(object sender, RoutedEventArgs e)
        {
            var services = GetSelectedRunningServices();
            if (services != null)
            {
                foreach (var svc in services)
                {
                    var svcElement = GetAvailableService(svc.Name);
                    Uninstall(svcElement, svc.Repository);
                }
            }
            RefreshScreen();
        }

        private void BtnStartClick(object sender, RoutedEventArgs e)
        {
            var services = GetSelectedRunningServices();
            if (services != null)
            {
                foreach (var svc in services)
                {
                    Start(svc.Name);
                }
            }
            RefreshScreen();
        }

        private void BtnStopClick(object sender, RoutedEventArgs e)
        {
            var services = GetSelectedRunningServices();
            if (services != null)
            {
                foreach (var svc in services)
                {
                    Stop(svc.Name);
                }
            }
            RefreshScreen();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshScreen();
        }

        private IEnumerable<Service> GetSelectedRunningServices()
        {
            if (RunningServices.SelectedItems == null || RunningServices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more services from the installed services list.");
                return null;
            }

            return RunningServices.SelectedItems.Cast<Service>();
        }

        private IEnumerable<ServiceElement> GetSelectedAvailableServices()
        {
            if (ServiceList.SelectedItems == null || ServiceList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more services from the available services list.");
                return null;
            }

            return from object svc in ServiceList.SelectedItems 
                   select GetAvailableService(((ListBoxItem) svc).Tag.ToString());
        }

        #endregion

        #region Service Control

        private static bool IsRunning(string serviceName)
        {
            return new ServiceController(serviceName).Status == ServiceControllerStatus.Running;
        }

        private static void Start(string serviceName)
        {
            if (!IsRunning(serviceName))
                new ServiceController(serviceName).Start();
        }

        private static void Stop(string serviceName)
        {
            if (IsRunning(serviceName))
                new ServiceController(serviceName).Stop();
        }

        private void Install(ServiceElement service, string repositoryPath)
        {
            try
            {
                Process svc;

                if(service.IsNServiceBusService)
                {
                    svc = new Process
                    {
                        StartInfo =
                        {
                            Arguments = " /install /serviceName:{0} /displayName:\"{1}\" /description:\"{2}\" /username:\"{3}\" /password:\"{4}\" /startManually".FormatWith(
                                service.Name,
                                service.DisplayName,
                                service.Description,
                                @"{0}\{1}".FormatWith(Environment.UserDomainName, Environment.UserName),
                                Password.Text),
                            FileName = Path.Combine(repositoryPath, service.RelativePath, @"bin\Debug\NServiceBus.Host.exe")
                        }
                    };
                }
                else
                {
                    string path = Path.Combine(repositoryPath, service.RelativePath, service.ServicePath);

                    svc = new Process
                    {
                        StartInfo =
                        {
                            Arguments = " /I:+ /UN:\"{0}\" /PW:\"{1}\"".FormatWith(@"{0}\{1}".FormatWith(Environment.UserDomainName, Environment.UserName), Password.Text),
                            FileName = path
                        }
                    };
                }

                svc.Start();
                svc.WaitForExit();
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to install service '{0}'\r\r{1}".FormatWith(service.Name, e.ToString()));
            }
        }

        private static void Uninstall(ServiceElement service, string repository)
        {
            try
            {
                Process svc;
                if(service.IsNServiceBusService)
                {
                    svc = new Process
                    {
                        StartInfo =
                        {
                            Arguments = " /uninstall /serviceName:\"{0}\"".FormatWith(service.Name),
                            FileName = Path.Combine(GetRepositoryPath(repository), service.RelativePath, @"bin\Debug\NServiceBus.Host.exe")
                        }
                    };
                }
                else
                {
                    svc = new Process
                    {
                        StartInfo =
                        {
                            Arguments = " /U:+",
                            FileName = Path.Combine(GetRepositoryPath(repository), service.RelativePath, service.ServicePath)
                        }
                    };
                }

                svc.Start();
                svc.WaitForExit();
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to uninstall service '{0}'\r\r{1}".FormatWith(service.Name, e.ToString()));
            }
        }

        #endregion

        #region Data Binding

        private void RefreshScreen()
        {
            RepositoryList.ItemsSource = App.GetRepositories().Select(r =>
            {
                string repoName = new DirectoryInfo(r.LocalLocation).Name;
                return new ComboBoxItem
                {
                    Tag = r.LocalLocation,
                    Content = repoName,
                    Name = r.Name,
                    IsSelected = false
                };
            });

            RunningServices.ItemsSource = GetMarketplaceServices().ToList();

            ServiceList.Items.Clear();
            foreach (ServiceElement svc in DevUtilityConfiguration.Current.Services)
            {
                ServiceList.Items.Add(new ListBoxItem
                {
                    Tag = svc.Name,
                    Content = svc.DisplayName,
                    Name = svc.Name,
                    IsSelected = false
                });
            }
        }

        private static IEnumerable<Service> GetMarketplaceServices()
        {
            var repositories = App.GetRepositories();

            return ServiceController.GetServices(Environment.MachineName)
                    .Where(svc => svc.ServiceName.ToLowerInvariant().Contains("errordite"))
                    .Select(svc =>
                        {
                            var serviceEntity = new Service
                            {
                                Name = svc.ServiceName,
                                Repository =
                                    GetRepositoryFromPathToExecutable(svc.ServiceName,
                                                                        repositories),
                                Executable =
                                    GetPathToExecutableFromRegistry(svc.ServiceName),
                                Status = svc.Status.ToString()
                            };
                            serviceEntity.ProcessId = GetProcessIdIfRunning(serviceEntity);
                            return serviceEntity;
                        }
                );
        }

        private static int? GetProcessIdIfRunning(Service serviceEntity)
        {
            //return null;
            var process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(serviceEntity.Executable)).FirstOrDefault(p => p.MainModule.FileName.Equals(serviceEntity.Executable, StringComparison.OrdinalIgnoreCase));
            if (process != null)
                return process.Id;
            return null;
        }

        private bool IsServiceInstalled(string serviceName, out string repositoryName)
        {
            var service = RunningServices.Items.Cast<Service>().FirstOrDefault(svc => svc.Name == serviceName);

            if (service != null)
            {
                repositoryName = service.Repository;
                return true;
            }

            repositoryName = string.Empty;
            return false;
        }

        private static string GetRepositoryFromPathToExecutable(string serviceName, IEnumerable<Repository> repositories)
        {
            string pathToExecutable = GetPathToExecutableFromRegistry(serviceName);

            foreach (var repo in repositories)
            {
                if (pathToExecutable.Contains(repo.LocalLocation + @"\"))
                    return repo.Name;
            }

            return string.Empty;
        }

        private static string GetPathToExecutableFromRegistry(string serviceName)
        {
            var commandLine =  Registry.GetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\" + serviceName, "ImagePath", string.Empty).ToString();
            var r = new Regex(@"[^""]+:\\([^""]*)");

            var executable = r.Match(commandLine).Value;

            return executable;
        }

        private static ServiceElement GetAvailableService(string serviceName)
        {
            return DevUtilityConfiguration.Current.Services
                .Cast<ServiceElement>()
                .FirstOrDefault(svc => svc.Name == serviceName);
        }

        private static string GetRepositoryPath(string repositoryName)
        {
            var repository = App.GetRepositories().Where(r => r.Name == repositoryName).FirstOrDefault();
            if (repository != null)
            {
                return repository.LocalLocation;
            }
            return string.Empty;
        }

        #endregion
    }
}
