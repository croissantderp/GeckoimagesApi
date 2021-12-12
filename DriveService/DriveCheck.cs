using System.Text.Json;
using Google.Apis.Drive.v3.Data;
using GeckoimagesApi.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeckoimagesApi.DriveService
{
    public class DriveCheck
    {
        private readonly GeckoContext _context;

        public DriveCheck(GeckoContext context)
        {
            _context = context;
        }

        public async Task setTimer()
        {
            System.Timers.Timer t = new System.Timers.Timer(10 * 60 * 1000);
            t.Elapsed += async (sender, e) => await checkDrive();
            t.AutoReset = true;
            t.Start();

            await checkDrive();
        }

        public async Task checkDrive()
        {
            Console.WriteLine("Checking drive");

            List<string> namesCalled = new List<string>();
            List<Geckoimage>? geckos = new List<Geckoimage>();

            if (System.IO.File.Exists(@"./public/db.json"))
            {
                //gets list of geckos already in database
                StreamReader dbRead = new StreamReader(@"./public/db.json");
                geckos = JsonSerializer.Deserialize<List<Geckoimage>>(dbRead.ReadToEnd());
                if (geckos == null)
                {
                    Console.WriteLine("Something catastrophically failed, oof");
                    return;
                }
                dbRead.Close();
            }

            var geckoimagesInApi = (await _context.Geckoimages.ToListAsync()).Select(a => a.number);

            Console.WriteLine(geckoimagesInApi.Count());
            foreach (var gecko in from Geckoimage gecko in geckos 
                                  where !geckoimagesInApi.Contains(gecko.number) 
                                  select gecko)
            {
                _context.Geckoimages.Add(gecko);
                await _context.SaveChangesAsync();
            }

            //signs into drive
            Google.Apis.Drive.v3.DriveService driveService = DriveUtils.AuthenticateServiceAccount(
                "geckoimagerworker@geckoimagesworker.iam.gserviceaccount.com",
                @".\geckoimagesworker-b3ff87875739.json");

            //requests all files in batches of 100
            var listRequest = driveService.Files.List();
            listRequest.PageSize = 100;
            listRequest.OrderBy = "name desc";
            listRequest.Fields = @"nextPageToken, files(*)";

            int count = 0;
            int updateCount = 0;

            bool highestFound = false;
            int highestGecko = 0;

            try
            {
                //iterates through the pages of 100
                while (true)
                {
                    FileList files2 = await listRequest.ExecuteAsync();
                    IList<Google.Apis.Drive.v3.Data.File> files = files2.Files;

                    foreach (Google.Apis.Drive.v3.Data.File a in files)
                    {
                        //if file is not in database and name matches naming conventions
                        if (new Regex(@"^(?:b|)\d+_.+").Match(a.Name).Success)
                        {
                            string description = a.Description != null && a.Description != "" ? a.Description : a.Owners.First().DisplayName;
                            DateTime time = DateTime.Parse(a.CreatedTimeRaw);

                            if (!geckos.Select(a => a.name).Contains(a.Name))
                            {
                                count++;

                                string name = a.Name.Split("_").First();

                                if (!highestFound && new Regex(@"^\d+_.+").Match(a.Name).Success)
                                {
                                    highestGecko = int.Parse(name);
                                    highestFound = true;
                                }

                                //adds gecko to database
                                Geckoimage gecko = new Geckoimage
                                {
                                    number = name,
                                    name = a.Name,
                                    author = description,
                                    created = time,
                                    url = name + "." + a.Name.Split(".").Last(),
                                    driveUrl = a.WebViewLink
                                };

                                geckos.Add(gecko);

                                _context.Geckoimages.Add(gecko);
                                await _context.SaveChangesAsync();


                                
                                //downloads file
                                using var fileStream = new FileStream(
                                    $"./public/{name}.{a.Name.Split(".").Last()}",
                                    FileMode.Create,
                                    FileAccess.Write);
                                await driveService.Files.Get(a.Id).DownloadAsync(fileStream);
                                fileStream.Close();
                            }
                            else
                            {
                                bool infoUpdated = false;
                                int index = geckos.FindIndex(b => b.name == a.Name);
                                if (geckos[index].created != time)
                                {
                                    geckos[index].created = time;
                                    infoUpdated = true;
                                }
                                if (geckos[index].author != description)
                                {
                                    geckos[index].author = description;
                                    infoUpdated = true;
                                }
                                if (!System.IO.File.Exists($"./public/{geckos[index].url}"))
                                {
                                    var geckoo = await _context.Geckoimages.FindAsync(geckos[index].number);
                                    if (geckoo == null)
                                    {
                                        Console.WriteLine(geckos[index].number);
                                        continue;
                                    }
                                    _context.Geckoimages.Remove(geckoo);
                                    await _context.SaveChangesAsync();

                                    geckos.RemoveAt(index);
                                    count++;

                                    string name = a.Name.Split("_").First();

                                    if (!highestFound && new Regex(@"^\d+_.+").Match(a.Name).Success)
                                    {
                                        highestGecko = int.Parse(name);
                                        highestFound = true;
                                    }

                                    //adds gecko to database
                                    Geckoimage gecko = new Geckoimage
                                    {
                                        number = name,
                                        name = a.Name,
                                        author = description,
                                        created = time,
                                        url = name + "." + a.Name.Split(".").Last(),
                                        driveUrl = a.WebViewLink
                                    };

                                    geckos.Add(gecko);

                                    _context.Geckoimages.Add(gecko);
                                    await _context.SaveChangesAsync();

                                    
                                    //downloads file
                                    using var fileStream = new FileStream(
                                        $"./public/{name}.{a.Name.Split(".").Last()}",
                                        FileMode.Create,
                                        FileAccess.Write);
                                    await driveService.Files.Get(a.Id).DownloadAsync(fileStream);
                                    fileStream.Close();
                                }

                                if (infoUpdated)
                                {
                                    var gecko = await _context.Geckoimages.FindAsync(geckos[index].number);
                                    if (gecko == null)
                                    {
                                        Console.WriteLine(geckos[index].number);
                                        continue;
                                    }
                                    _context.Geckoimages.Remove(gecko);
                                    await _context.SaveChangesAsync();
                                    _context.Geckoimages.Add(geckos[index]);
                                    await _context.SaveChangesAsync();
                                }
                            }

                            namesCalled.Add(a.Name);
                        }
                        //else if file matches submission naming convention
                        else if (new Regex(@".+ - .+").Match(a.Name).Success)
                        {
                            updateCount++;

                            //new file to update
                            Google.Apis.Drive.v3.Data.File file = new Google.Apis.Drive.v3.Data.File();

                            //splits name and subsplits it
                            List<string> splitName = a.Name.Split(" - ").ToList();

                            List<string> nameSplit = splitName.Last().Split(".").ToList();

                            string extension = nameSplit.Last();

                            nameSplit.Remove(extension);

                            //updates description
                            file.Description = string.Join(".", nameSplit);

                            splitName.Remove(splitName.Last());

                            //updates name
                            file.Name = highestGecko + "_" + string.Join(" - ", splitName).Replace(" ", "_") + "." + extension;
                            highestGecko++;

                            //updates file in drive
                            driveService.Files.Update(file, a.Id).Execute();
                        }
                    }

                    //scrolls to next page
                    listRequest.PageToken = files2.NextPageToken;

                    //if there is not a next page, quit
                    if (files2.NextPageToken == null)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("drive check failed, reason: " + ex.ToString());
            }

            //remove unfound geckos
            foreach (Geckoimage gecko in geckos.ToList())
            {
                if (gecko.name != null && !namesCalled.Contains(gecko.name))
                {
                    System.IO.File.Delete($"./public/{gecko.url}");
                    geckos.Remove(gecko);

                    _context.Geckoimages.Remove(gecko);
                    await _context.SaveChangesAsync();
                }
            }

            if (count != 0)
            {
                //writes updated list to database
                StreamWriter dbWrite = new StreamWriter(@"./public/db.json");
                await dbWrite.WriteAsync(JsonSerializer.Serialize(geckos, new JsonSerializerOptions { WriteIndented = true }));
                dbWrite.Close();

                //await deploy();
            }

            Console.WriteLine($"Done, added {count} files, updated {updateCount} files in submissions folder");
        }

    }
}
