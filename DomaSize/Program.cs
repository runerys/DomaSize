using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileHelpers;

namespace DomaSize
{
    /*
     * SELECT u.FirstName, u.LastName, u.Email, m.Date, m.ID, m.MapName, m.CreatedTime, m.MapImage, m.ThumbnailImage, m.BlankMapImage, m.name
       FROM doma_maps m inner join doma_users u on m.UserID = u.ID
       Where m.CreatedTime > '2019-01-01'
     */
    class Program
    {
        private static string _fileRoot = @"C:\temp\DomaSize";

        static void Main(string[] args)
        {
            
            //var size = GetContentLength("4462.blank.png");
            //var source = @"C:\Users\rune.rystad\kode\DomaSize\DomaSize\2298.jpg";
            //new ImageResizer(2 * 1024 * 1024, source, source.Replace(".jpg", "_result.jpg")).ScaleImage();

            //return;

            var filezillaLogFile = "filezilla.log";

            var inputMapsFile = "doma_maps_all.csv";
            var outputMapsFile = "doma_maps_all_results.csv";
            var statisticsFile = "size_stats.txt";

            var ftpLogFile = ReadFtpLog(filezillaLogFile);

            var engine = new FileHelperEngine<Map>();
            engine.Encoding = Encoding.UTF8;
            var maps = engine.ReadFile(Path.Combine(_fileRoot, inputMapsFile));

            var fileSizes = ftpLogFile.ToDictionary(x => x.Name, x => x);

            var results = from m in maps.AsParallel()
                          let mapSize = GetContentLength(m.MapImage, fileSizes)
                          let blankMapSize = GetContentLength(m.BlankMapImage, fileSizes)
                          select new Map
                              {
                                  Firstname = m.Firstname,
                                  Lastname = m.Lastname,
                                  email = m.email,
                                  date = m.date,
                                  id = m.id,
                                  mapname = m.mapname,
                                  navn = m.navn,
                                  BlankMapImage = m.BlankMapImage,
                                  ThumbnailImage = m.ThumbnailImage,
                                  MapImage = m.MapImage,
                                  MapSize = FormatSize(mapSize),
                                  BlankMapSize = FormatSize(blankMapSize),
                                  createdtime = m.createdtime
                              };

            engine.WriteFile(Path.Combine(_fileRoot, outputMapsFile), results);
            
            var connectedToMaps = ftpLogFile.Where(x => x.IsConnectedToMap).ToList();
            var notConnectedToMaps = ftpLogFile.Where(x => !x.IsConnectedToMap).ToList();

            var totalSize = FormatSize(ftpLogFile.Sum(x => x.Size));
            var notConnectedSize = FormatSize(notConnectedToMaps.Sum(x => x.Size));
            var connectedSize = FormatSize(connectedToMaps.Sum(x => x.Size));

            var statistics = new StringBuilder();
            statistics.AppendLine("Maps: " + maps.Length);
            statistics.AppendLine("Files: " + ftpLogFile.Count + ", total size: " + totalSize);
            statistics.AppendLine("Files connected to maps: " + connectedToMaps.Count + ", size: " + connectedSize);
            statistics.AppendLine("Files not connected to maps: " + notConnectedToMaps.Count + ", size: " + notConnectedSize);
            statistics.AppendLine("-------- Files on disk, not connected to maps ------");
            foreach (var notConnectedToMap in notConnectedToMaps.OrderBy(x => x.Modified))
            {
                statistics.AppendLine(notConnectedToMap.Name + ";" + notConnectedToMap.Size + ";" + notConnectedToMap.Modified);
            }
            File.WriteAllText(Path.Combine(_fileRoot, statisticsFile), statistics.ToString());
        }

        private static List<FileZillaLine> ReadFtpLog(string fileZillaLog)
        {
            var engine = new FileHelperEngine<FileZillaLine>();
            return engine.ReadFileAsList(Path.Combine(_fileRoot, fileZillaLog));
        }

        private static string FormatSize(long bytes)
        {            
            var mb = (double)bytes / 1014 / 1024;
            return mb.ToString("N5");
        }

        private static int Counter = 0;

        private static long GetContentLength(string filename, Dictionary<string, FileZillaLine> ftpInfo)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return 0;

            if(Interlocked.Increment(ref Counter)%100 == 0)
                Console.WriteLine(Counter);

            var filenameWithoutDot = "not_in_dictionary";
            if (filename.EndsWith("."))
            {
                filenameWithoutDot = filename.Replace(".", string.Empty);
            }

            if (ftpInfo != null)
            {
                if (ftpInfo.ContainsKey(filename))
                {
                    return GetAndTagFileSize(filename, ftpInfo);
                }
                if (ftpInfo.ContainsKey(filenameWithoutDot))
                {
                    return GetAndTagFileSize(filenameWithoutDot, ftpInfo);
                }
            }

            if (filename.EndsWith("."))
                return 0;

            try
            {
                WebRequest req = WebRequest.Create("http://kartarkiv.nydalen.idrett.no/map_images/" + filename);
                req.Method = "HEAD";
                using (WebResponse resp = req.GetResponse())
                {
                    
                    int contentLength = 0;
                    if (int.TryParse(resp.Headers.Get("Content-Length"), out contentLength))
                        return contentLength;

                    return 0;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        private static long GetAndTagFileSize(string filename, Dictionary<string, FileZillaLine> ftpInfo)
        {
            var info = ftpInfo[filename];
            info.IsConnectedToMap = true;

            var id = filename;

            var dotIndex = filename.IndexOf(".", StringComparison.OrdinalIgnoreCase);
            if (dotIndex > 0)
                id = filename.Substring(0, dotIndex);

            var thumbnail = ftpInfo.Values.SingleOrDefault(x => x.Name.StartsWith(id + ".thumbnail"));
            if (thumbnail != null)
                thumbnail.IsConnectedToMap = true;

            return info.Size;
        }
    }
}
