using Microsoft.VisualBasic;
using System.Reflection;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shop_server.util
{
    public class AuthoritiesDAO
    {

        //Path, fileManager and json-serialization for json-read/write
        private readonly string jsonPath;
        FileStream fileStream;

        private readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public AuthoritiesDAO(IWebHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(environment);
            string jsonDirectoryPath = Path.Combine(environment.WebRootPath, "json");

            if (!Directory.Exists(jsonDirectoryPath))
            {
                Directory.CreateDirectory(jsonDirectoryPath);
            }

            jsonPath = Path.Combine(jsonDirectoryPath, "authorities.json");
        }

        public async Task<AuthoritiesData> ReadAuthorities()
        {
            AuthoritiesData? data;

            if (!File.Exists(jsonPath))
            {
                return new AuthoritiesData{};
            }

            fileStream = new(jsonPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            await using (fileStream)
            {
                data = await JsonSerializer.DeserializeAsync<AuthoritiesData>(fileStream, jsonOptions);
            }

            return data ?? throw new InvalidDataException();
        }

        public async Task<bool> ReadAuthority(string authority_name)
        {
            PropertyInfo[] authorities = typeof(AuthoritiesData).GetProperties();

            //1.Is such an authority-key? 
            PropertyInfo authority = authorities.FirstOrDefault(p => p.Name == authority_name);

            if (authority == null)
            {
                throw new InvalidDataException($"Nincs ilyen attribútum: {authority}");
            }

            //2.Can we read it from json or use default value?
            bool access = false;
            var defaultData = new AuthoritiesData();
            access = (bool)(authority.GetValue(defaultData) ?? false);

            if (!File.Exists(jsonPath)) return access;


            //3.Read from json!
            AuthoritiesData data;
            fileStream = new(jsonPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            await using (fileStream)
            {
                data = await JsonSerializer.DeserializeAsync<AuthoritiesData>(fileStream, jsonOptions);
            }

            if (data != null) access = (bool)(authority.GetValue(data) ?? false);
            
            return access;
        }

        public async Task WriteAuthorities(AuthoritiesData authorities)
        {
            //Rewrite or change the json-value
            AuthoritiesData data = new AuthoritiesData();

            if (File.Exists(jsonPath))
            {
                using (FileStream readStream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    data = await JsonSerializer.DeserializeAsync<AuthoritiesData>(readStream, jsonOptions) ?? new AuthoritiesData();
                }
            }


            fileStream = new(jsonPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using (fileStream)
            {
                await JsonSerializer.SerializeAsync(fileStream, authorities, jsonOptions);
            }
        }

        /*
        public async Task WriteAuthority(string authority_name, bool access)
        {
            PropertyInfo[] authorities = typeof(AuthoritiesData).GetProperties();

            //1.Is such an authority-key? 
            PropertyInfo authority = authorities.FirstOrDefault(p => p.Name == authority_name);

            if (authority == null)
            {
                throw new InvalidDataException($"Nincs ilyen attribútum: {authority}");
            }

            //2.Rewrite or change the json-value
            AuthoritiesData data = new AuthoritiesData();

            if (File.Exists(jsonPath))
            {
                using (FileStream readStream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    data = await JsonSerializer.DeserializeAsync<AuthoritiesData>(readStream, jsonOptions) ?? new AuthoritiesData();
                }
            }

            authority.SetValue(data, access);


            fileStream = new(jsonPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using(fileStream)
            {
                await JsonSerializer.SerializeAsync(fileStream, data, jsonOptions);
            }
        }
        */

    }
}
