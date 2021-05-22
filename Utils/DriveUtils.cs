using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using System.Text.RegularExpressions;


namespace GeckoBot.Utils
{
    // Utilities for the Gecko Images Suite
    public class DriveUtils
    {
        // Initializes the DriveService
        public static DriveService AuthenticateServiceAccount(string serviceAccountEmail, string serviceAccountCredentialFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceAccountCredentialFilePath))
                    throw new Exception("Path to the service account credentials file is required.");
                if (!File.Exists(serviceAccountCredentialFilePath))
                    throw new Exception("The service account credentials file does not exist at: " +
                                        serviceAccountCredentialFilePath);
                if (string.IsNullOrEmpty(serviceAccountEmail))
                    throw new Exception("ServiceAccountEmail is required.");

                // These are the scopes of permissions you need. It is best to request only what you need and not all of them
                string[] scopes = { DriveService.Scope.DriveReadonly }; // View your Google Analytics data

                // For Json file
                if (Path.GetExtension(serviceAccountCredentialFilePath).ToLower() == ".json")
                {
                    using var stream = new FileStream(serviceAccountCredentialFilePath, FileMode.Open, FileAccess.Read);
                    
                    GoogleCredential credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(scopes);

                    // Create the Analytics service.
                    return new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "GeckoBot",
                    });
                }
                if (Path.GetExtension(serviceAccountCredentialFilePath).ToLower() == ".p12")
                {
                    // If its a P12 file
                    var certificate = new X509Certificate2(serviceAccountCredentialFilePath, "notasecret",
                        X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                    var credential = new ServiceAccountCredential(
                        new ServiceAccountCredential.Initializer(serviceAccountEmail)
                        {
                            Scopes = scopes
                        }.FromCertificate(certificate));

                    // Create the  Drive service.
                    return new DriveService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Drive Authentication Sample",
                    });
                }
                throw new Exception("Unsupported Service accounts credentials.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Create service account DriveService failed" + ex.Message);
                throw new Exception("CreateServiceAccountDriveFailed", ex);
            }
        }

        // Checks cache for image, returns the pathname if found or "" if not found
        private static string CheckCache(string num)
        {
            List<string> supportedTypes = new (){ "png", "jpeg", "gif" };
            foreach (var type in supportedTypes)
            {
                string path = $"../../Cache/{num}_icon.{type}";
                if (File.Exists(path)) return path;
            }
            return null;
        }
        
        // Adds zeroes to an integer to match the gecko naming convention
        // 25 => 025
        public static string addZeros(int x)
        {
            var name = x.ToString();
            
            //adds 0s as needed
            while (name.Length < 3)
            {
                name = "0" + name;
            }

            return name;
        }

        // Gets path from cache or downloads image to cache from drive
        public static string ImagePath(int num, bool isAlt, bool refresh = false)
        {
            Commands.Gec.RefreshGec();
            FileUtils.checkForCacheExistance();

            string name = addZeros(num);

            if (isAlt)
            {
                name = "b" + name;
            }
            
            // If image already exists in cache, use it
            string cached = CheckCache(name);
            if (cached != null && Commands.Gec.geckos.ContainsKey(name)) 
            {
                if (!refresh)
                {
                    return cached;
                }
                
                File.Delete(cached);
                cached = null;
            }

            if (refresh && Commands.Gec.geckos.ContainsKey(name))
            {
                Console.WriteLine(Commands.Gec.geckos.Count());
                Commands.Gec.geckos.Remove(name);
                Console.WriteLine(Commands.Gec.geckos.Count());
            }

            // Otherwise, fetch it from google drive
            DriveService driveService = AuthenticateServiceAccount(
                "geckobotfileretriever@geckobot.iam.gserviceaccount.com", 
                "../../../GeckoBot-af43fa71833e.json");
            var listRequest = driveService.Files.List();
            listRequest.PageSize = 1; // Only fetch one
            listRequest.Q = $"name contains '{name}_'"; // Search for the image via the given number
            
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            
            if (files != null && files.Count > 0)
            {
                // Use the first result
                var file = files[0];

                //adds name of gecko to list
                if (!Commands.Gec.geckos.ContainsKey(name))
                {
                    Commands.Gec.geckos.TryAdd(name, file.Name);

                    FileUtils.Save(Globals.DictToString(Commands.Gec.geckos, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko7.gek");

                    if (cached != null) return cached;
                }

                //Console.WriteLine(file.MimeType);
                string type = file.MimeType.Replace("image/", ""); // sorta hacky solution to get file type

                for (int i = 0; i < 1000; i++)
                {
                    try
                    {
                        using (var fileStream = new FileStream($"../../Cache/{name}_icon.{type}", FileMode.Create, FileAccess.Write))
                        {
                            driveService.Files.Get(file.Id).Download(fileStream);
                        }
                        break;
                    }
                    catch
                    {
                        continue;
                    }
                }

                return CheckCache(name) ?? throw new Exception("Drive download failed!");
            }
            
            throw new Exception("File " + name + " not found!");
        }

        // Gets path from cache or downloads image to cache from drive
        public static int saveAll(int highest)
        {
            Commands.Gec.RefreshGec();
            FileUtils.checkForCacheExistance();

            DriveService driveService = AuthenticateServiceAccount(
                "geckobotfileretriever@geckobot.iam.gserviceaccount.com",
                "../../../GeckoBot-af43fa71833e.json");
            var listRequest = driveService.Files.List();
            listRequest.PageSize = 1000; // Only fetch one thousand
            listRequest.Q = "mimeType contains 'image'"; // Filter out folders or other non image types

            Google.Apis.Drive.v3.Data.FileList files2 = listRequest.Execute();
            IList<Google.Apis.Drive.v3.Data.File> files = files2.Files;

            int totalAmount = 0;
            int amount = 0;

            while (totalAmount < highest)
            {
                foreach (Google.Apis.Drive.v3.Data.File file in files)
                {
                    string name = file.Name.Remove(3);

                    if (name.Contains("b")) name = file.Name.Remove(4);
                    if (CheckCache(name) == null)
                    {
                        string type = file.MimeType.Replace("image/", ""); // sorta hacky solution to get file type

                        using var fileStream = new FileStream(
                            $"../../Cache/{name}_icon.{type}",
                            FileMode.Create,
                            FileAccess.Write);
                        driveService.Files.Get(file.Id).Download(fileStream);

                        amount++;
                    }

                    if (!Commands.Gec.geckos.ContainsKey(name))
                    {
                        Commands.Gec.geckos.Add(name, file.Name);

                        if (CheckCache(name) != null) continue;
                    }
                    else if (!Commands.Gec.geckos.ContainsValue(file.Name))
                    {
                        Commands.Gec.geckos.Remove(name);
                        Commands.Gec.geckos.Add(name, file.Name);
                    }



                    totalAmount++;
                }
                listRequest.PageToken = files2.NextPageToken;
            }

            FileUtils.Save(Globals.DictToString(Commands.Gec.geckos, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko7.gek");

            return amount;
        }
    }
}