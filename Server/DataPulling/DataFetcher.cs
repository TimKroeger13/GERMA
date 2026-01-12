using GERMAG.DataModel.Database;
using GERMAG.Server.DataPulling.JsonDeserialize;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace GERMAG.Server.DataPulling; //Can Push to Azure test!

public interface IDataFetcher
{
    Task FetchAllData();
}

public class DataFetcher(DataContext context, IDatabaseUpdater databaseUpdater, IHttpClientFactory httpFactory, IJsonDeserialize jsonDeserialize) : IDataFetcher
{
    public async Task FetchAllData()
    {
        using var client = httpFactory.CreateClient(HttpClients.LongTimeoutClient);
        var allGeothermalParameters = context.GeothermalParameter.OrderBy(gp => gp.Id).ToList();

        bool dataNotFound;

        context.Database.SetCommandTimeout(TimeSpan.FromMinutes(60));

        for (int i = 0; i < allGeothermalParameters.Count; i++)
        {
            //36 <- 80 meter
            //25 <- water temp 20,40,60,100

            dataNotFound = false;

            Console.WriteLine("Pinging data: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
            var getrequest = allGeothermalParameters[i].Getrequest ?? "";

            //string seriallizedInputJson = await client.GetStringAsync(getrequest);

            const int maxRetries = 3;

            string seriallizedInputJson = "";
            int retryCount = 0;

            if (getrequest.StartsWith("https"))
            {
                while (retryCount < maxRetries)
                {
                    try
                    {
                        seriallizedInputJson = await client.GetStringAsync(getrequest);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        retryCount++;

                        if (retryCount < maxRetries)
                        {
                            TimeSpan delay = TimeSpan.FromSeconds(3);
                            await Task.Delay(delay);
                        }
                        else
                        {
                            Console.WriteLine("Maximum retries reached. Unable to get data.");
                            dataNotFound = true;
                        }
                    }
                }
            }
            else //local case
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string targetDirectory = "gs-germag";

                while (!Directory.Exists(Path.Combine(currentDirectory, targetDirectory)))
                {
                    DirectoryInfo? parentDirectory = Directory.GetParent(currentDirectory);
                    if (parentDirectory == null)
                    {
                        throw new Exception("Target directory not found.");
                    }
                    currentDirectory = parentDirectory.FullName;
                }

                string resourcesFile = Path.Combine(currentDirectory, targetDirectory, "Resources", getrequest);

                if (File.Exists(resourcesFile))
                {
                    using (StreamReader r = new StreamReader(resourcesFile))
                    {
                        seriallizedInputJson = r.ReadToEnd();

                        //Use .Trim() Instead!!!!!!!!!!!!!!
                        seriallizedInputJson = Regex.Replace(Regex.Replace(Regex.Replace(seriallizedInputJson, @"\s+", ""), @"\n", ""), @"\r", "");

                        //seriallizedInputJson = Regex.Replace(Regex.Replace(seriallizedInputJson, @"\n", ""), @"\r", "");
                    }
                }
                else
                {
                    throw new Exception("Target file not found.");
                }
            }

            //Check if Data is up to date

            var dataHash = allGeothermalParameters[i].Hash ?? -1;
            var hash = HashString(seriallizedInputJson);

            if (!dataNotFound)
            {
                context.GeothermalParameter.First(gp => gp.Id == allGeothermalParameters[i].Id).LastPing = DateTime.Now;
                context.SaveChanges();
            }

            if (dataHash != hash && !dataNotFound)
            {
                //Differentiate between different coordiante formats

                bool isSingleCoordinateFormat = Regex.IsMatch(seriallizedInputJson, "coordinate[s]?\\\":\\[\\d");
                bool IsLongCoordianteFormat = Regex.IsMatch(seriallizedInputJson, "coordinate[s]?\\\":\\[\\[\\[\\[");
                bool IsShortCoordianteFormat = Regex.IsMatch(seriallizedInputJson, "coordinate[s]?\\\":\\[\\[\\d");

                var Format = JsonFormat.normal;

                if (isSingleCoordinateFormat)
                {
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinates\"", "\"coordinatesSingle\"");
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinate\"", "\"coordinatesSingle\"");
                    Format = JsonFormat.single_coordiantes;
                }
                if (IsLongCoordianteFormat)
                {
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinates\"", "\"coordinatesLong\"");
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinate\"", "\"coordinatesLong\"");
                    Format = JsonFormat.long_coordiantes;
                }
                if (IsShortCoordianteFormat)
                {
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinates\"", "\"coordinatesShort\"");
                    seriallizedInputJson = Regex.Replace(seriallizedInputJson, "\"coordinate\"", "\"coordinatesShort\"");
                    Format = JsonFormat.short_coordiantes;
                }

                //update Data when not up to date

                Console.WriteLine("DeserializationJson: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
                var jsonData_Root = jsonDeserialize.DeserializeJson(seriallizedInputJson, allGeothermalParameters[i].Type, Format);

                Console.WriteLine("Insert Data into Database: " + allGeothermalParameters[i].Type + " | " + allGeothermalParameters[i].Area);
                databaseUpdater.UpdateDatabase(jsonData_Root, allGeothermalParameters[i].Id);

                context.GeothermalParameter.First(gp => gp.Id == allGeothermalParameters[i].Id).Hash = hash;
                context.SaveChanges();
            }
            else
            {
                if (i == allGeothermalParameters.Count - 1)
                {
                    Console.WriteLine("Database Updated!");
                }
            }
        }
        // Create and update Indexing

        context.Database.ExecuteSqlRaw(@"
            DO $$
            BEGIN
                IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'geometry_index') THEN
                    DROP INDEX geometry_index;
                END IF;
            END $$;

            CREATE INDEX geometry_index
                ON geo_data
                USING GIST (geom);
        ");

        Console.WriteLine("Database updated completly and spatial indexing refreshed!");
    }

    private static int HashString(string text)
    {
        unchecked
        {
            int hash = 23;
            foreach (char c in text)
            {
                hash = (hash * 31) + c;
            }
            return hash;
        }
    }
}