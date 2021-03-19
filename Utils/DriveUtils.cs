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
        public static string ImagePath(int num, bool isAlt)
        {
            FileUtils.checkForCacheExistance();

            string name = addZeros(num);

            if (isAlt)
            {
                name = "b" + name;
            }
            
            // If image already exists in cache, use it
            string cached = CheckCache(name);
            if (cached != null && Commands.Gec.geckos.ContainsKey(name)) return cached;

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
                    Commands.Gec.RefreshGec();
                    Commands.Gec.geckos.Add(name, file.Name);

                    FileUtils.Save(Globals.DictToString(Commands.Gec.geckos, "{0} ⁊ {1} ҩ "), @"..\..\Cache\gecko7.gek");

                    if (cached != null) return cached;
                }

                //Console.WriteLine(file.MimeType);
                string type = file.MimeType.Replace("image/", ""); // sorta hacky solution to get file type

                using var fileStream = new FileStream(
                    $"../../Cache/{name}_icon.{type}",
                    FileMode.Create,
                    FileAccess.Write);
                driveService.Files.Get(file.Id).Download(fileStream);
                return CheckCache(name) ?? throw new Exception("Drive download failed!");
            }
            
            throw new Exception("File " + name + " not found!");
        }
    }
}