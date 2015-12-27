using System;
using System.Linq;
using System.ServiceModel;

namespace NotificationServiceConsoleHost
{
    class Program
    {
        private const string StopCommand = "STOP";

        static void Main(string[] args)
        {
            ServiceHost serviceHost = null;
            try
            {
                serviceHost = new ServiceHost(typeof(NotificationServiceEngine.NotificationServiceEngine));
                serviceHost.Open();
                Console.WriteLine("****Notification service was started. Type {0} and press Enter to stop it****", StopCommand);
                while (string.Compare(Console.ReadLine(), StopCommand, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                }
            }
            catch (AddressAlreadyInUseException)
            {
                Console.WriteLine("Address specifed in config file '{0}' is already in use", serviceHost.BaseAddresses.Select(x => x.AbsoluteUri).FirstOrDefault());
                Console.WriteLine("Press Enter to close application");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to start service. Error - {0}", ex);
                Console.WriteLine("Press Enter to close application");
                Console.ReadLine();
            }
            finally
            {
                if (serviceHost != null)
                {
                    try
                    {
                        serviceHost.Close();
                    }
                    catch (Exception)
                    {
                        serviceHost.Abort();
                    }
                }
            }
        }
    }
}
