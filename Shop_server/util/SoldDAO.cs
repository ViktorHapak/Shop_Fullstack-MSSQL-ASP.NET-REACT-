using Shop_server.Models;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;

namespace Shop_server.util
{
    public class SoldDAO
    {

        //Path, fileManagers for txt-read/write
        string path = "txt/sold.txt";
        StreamWriter streamWriter;
        StreamReader streamReader;


        //Path, fileManager and json-serialization for json-read/write
        private readonly string jsonPath;
        FileStream fileStream;

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public SoldDAO()
        {
            streamWriter = new StreamWriter(path, append: false);
            streamReader = new StreamReader(path);
        }

        public SoldDAO(IWebHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(environment);
            string jsonDirectoryPath = Path.Combine(environment.WebRootPath, "json");

            if (!Directory.Exists(jsonDirectoryPath))
            {
                Directory.CreateDirectory(jsonDirectoryPath);
            }

            jsonPath = Path.Combine(jsonDirectoryPath, "sold.json");
        }

        //Set actual daily income into txt-resource
        public async Task ReWriteTxt(Decimal Income, DateTime OldDate)
        {
            DateTime today = DateTime.UtcNow;

            using (streamWriter)
            {
                if (today == OldDate)
                {
                    streamWriter.BaseStream.Position++;
                    await streamWriter.WriteLineAsync("Income: " + Income);

                }
                else
                {
                    await streamWriter.WriteLineAsync("Date: " + today);
                    await streamWriter.WriteLineAsync("Income: " + Income);
                }
            }
        }

        //Get actual daily income from txt-resource
        public async Task<(DateTime, Decimal)> ReadTxt()
        {
            DateTime today;
            Decimal income;

            if (!File.Exists(path)) return (DateTime.UtcNow, 0);

            using (streamReader)
            {
                string firstLine = await streamReader.ReadLineAsync();
                today = Convert.ToDateTime(firstLine.Split(new[] { ' ' }, 2)[1]);

                string secondLine = await streamReader.ReadLineAsync();
                income = Convert.ToDecimal(firstLine.Split(new[] { ' ' }, 2)[1]);
            }

            if (today == null || income == null)
                throw new KeyNotFoundException("Hiba történt az aktuális értékek beolvasása során!");

            return (today, income);
        }


        //Set actual daily income into json-resource
        public async Task RewriteJsonAsync(decimal income)
        {
            SoldData newData = new SoldData
            {
                Date = DateTime.UtcNow.Date,
                Income = income
            };

            await using FileStream fileStream = new(
                jsonPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            await JsonSerializer.SerializeAsync(
                fileStream,
                newData,
                jsonOptions
            );
        }

        //Get actual daily income from json-resource
        public async Task<SoldData> ReadJSON()
        {
            if (!File.Exists(jsonPath) || new FileInfo(jsonPath).Length == 0)
            {
                return new SoldData
                {
                    Date = DateTime.UtcNow,
                    Income = 0
                };
            }

            await using FileStream fileStream =
                new(jsonPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            SoldData? data =
                await JsonSerializer.DeserializeAsync<SoldData>(
                    fileStream,
                    jsonOptions
                );

            return data ?? new SoldData
            {
                Date = DateTime.UtcNow,
                Income = 0
            };
        }
    }
}