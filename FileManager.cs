using Newtonsoft.Json;

namespace AssPain_FileManager;

public static class FileManager
{
    //TODO: rewrite
    public static string MusicFolder = "";
    
    public static List<string> GetAuthors()
    {
        var root = Directory.EnumerateDirectories($"{AppContext.BaseDirectory}/music");
        List<string> authors = new List<string>();
        foreach (string author in root)
        {
            Console.WriteLine(author);
            authors.Add(author);
        }
        return authors;
    }

    ///<summary>
    ///Gets all albums of all authors
    ///</summary>
    public static List<string> GetAlbums()
    {
        var root = Directory.EnumerateDirectories($"{AppContext.BaseDirectory}/music");
        List<string> albums = new List<string>();

        foreach (string author in root)
        {
            foreach (string album in Directory.EnumerateDirectories(author))
            {
                Console.WriteLine(album);
                albums.Add(album);
            }
        }
        return albums;
    }

    ///<summary>
    ///Gets all albums from <paramref name="author"/>
    ///</summary>
    public static List<string> GetAlbums(string author)
    {
        List<string> albums = new List<string>();
        foreach (string album in Directory.EnumerateDirectories(author))
        {
            Console.WriteLine(album);
            albums.Add(album);
        }
        return albums;
    }

    ///<summary>
    ///Gets all songs in album or all albumless songs for author
    ///</summary>
    public static List<string> GetSongs(string path)
    {
        List<string> songs = new List<string>();
        var mp3Files = Directory.EnumerateFiles(path, "*.mp3");
        foreach (string currentFile in mp3Files)
        {
            Console.WriteLine(currentFile);
            songs.Add(currentFile);
        }
        return songs;
    }

    ///<summary>
    ///Gets all songs in device
    ///</summary>
    public static List<string> GetSongs()
    {
        var root = $"{AppContext.BaseDirectory}/music";
        var mp3Files = Directory.EnumerateFiles(root, "*.mp3", SearchOption.AllDirectories);
        List<string> songs = new List<string>();
        foreach (string currentFile in mp3Files)
        {
            Console.WriteLine(currentFile);
            songs.Add(currentFile);
        }
        return songs;
    }

    public static string GetSongTitle(string path)
    {
        var tfile = TagLib.File.Create(path);
        return tfile.Tag.Title;
    }

    public static List<string> GetSongTitle(List<string> Files)
    {
        List<string> titles = new List<string>();
        foreach (string currentFile in Files)
        {
            var tfile = TagLib.File.Create(currentFile);
            titles.Add(tfile.Tag.Title);
        }
        return titles;
    }

    public static string GetSongAlbum(string path)
    {
        if (File.Exists(path))
        {
            var tfile = TagLib.File.Create(path);
            return tfile.Tag.Album;
        }
        else
        {
            return "cant get album";
        }
    }
    public static string[] GetSongArtist(string path)
    {
        if (File.Exists(path))
        {
            var tfile = TagLib.File.Create(path);
            return tfile.Tag.AlbumArtists;
        }
        else
        {
            string[] noArtist = { "cant get artist" };
            return noArtist;
        }
    }

    ///<summary>
    ///Gets last name/folder from <paramref name="path"/>
    ///</summary>
    public static string GetNameFromPath(string path)
    {
        string[] subs = path.Split('/');
        return subs[subs.Length-1];
    }

    ///<summary>
    ///Gets album name and author from album <paramref name="path"/>
    ///</summary>
    public static Dictionary<string, string> GetAlbumAuthorFromPath(string path)
    {
        return new Dictionary<string, string>
        {
            { "album", GetNameFromPath(path) },
            { "author", GetNameFromPath(Path.GetDirectoryName(path)) }
        };
    }

    public static bool IsDirectory(string path)
    {
        return string.IsNullOrEmpty(Path.GetFileName(path)) || Directory.Exists(path);
    }

    public static string GetAlias(string name)
    {
        string path = AppContext.BaseDirectory;
        Dictionary<string, string> aliases;
        if (File.Exists($"{path}/aliases.json"))
        {
            string json = File.ReadAllText($"{path}/aliases.json");
            aliases = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        else
        {
            aliases = new Dictionary<string, string>();
            File.WriteAllText($"{path}/aliases.json", JsonConvert.SerializeObject(aliases));

        }
        if (aliases.ContainsKey(name))
        {
            return GetAlias(aliases[name]);
        }
        else if (aliases.ContainsKey(Sanitize(name)))
        {
            return GetAlias(aliases[Sanitize(name)]);
        }
        return name;
    }

    public static void AddAlias(string name, string target)
    {
        string path = AppContext.BaseDirectory;
        Dictionary<string, string> aliases;
        if (File.Exists($"{path}/aliases.json"))
        {
            string json = File.ReadAllText($"{path}/aliases.json");
            aliases = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        else
        {
            aliases = new Dictionary<string, string>();
            File.WriteAllText($"{path}/aliases.json", JsonConvert.SerializeObject(aliases));

        }
        if (name == target)
        {
            return;
        }
        string author = Sanitize(target);

        string nameFile = Sanitize(name);

        aliases.Add(nameFile, nameFile);
        File.WriteAllText($"{path}/aliases.json", JsonConvert.SerializeObject(aliases));
        Directory.Move($"{path}/music/{nameFile}", $"{path}/music/{author}");
        foreach (string song in FileManager.GetSongs($"{path}/music/{author}"))
        {
            var tfile = TagLib.File.Create($"{song}");
            string[] autors = tfile.Tag.Performers;
            for (int i = 0; i < autors.Length; i++)
            {
                if (autors[i] == name)
                {
                    autors[i] = target;
                }
                else
                {
                    //TODO: add symlink move
                    if (i == 0)
                    {
                        continue;
                    }
                    //Android.Systems.Os.Symlink();
                }
            }
            tfile.Tag.Performers = autors;
            tfile.Save();
        }
    }

    public static void CreatePlaylist(string name)
    {
        string path = AppContext.BaseDirectory;
        Dictionary<string, List<string>> playlists;
        if (File.Exists($"{path}/playlists.json"))
        {
            string json = File.ReadAllText($"{path}/playlists.json");
            playlists = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            playlists.Add(name, new List<string>());
            File.WriteAllText($"{path}/playlists.json", JsonConvert.SerializeObject(playlists));
        }
        else
        {
            playlists = new Dictionary<string, List<string>> { {name, new List<string>() } };
            File.WriteAllText($"{path}/playlists.json", JsonConvert.SerializeObject(playlists));
        }
    }

    public static void AddToPlaylist(string name, string song)
    {
        string path = AppContext.BaseDirectory;
        string json = File.ReadAllText($"{path}/playlists.json");
        Dictionary<string, List<string>> playlists = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        playlists[name].Add(song);
        File.WriteAllText($"{path}/playlists.json", JsonConvert.SerializeObject(playlists));
    }

    public static void AddToPlaylist(string name, List<string> songs)
    {
        string path = AppContext.BaseDirectory;
        string json = File.ReadAllText($"{path}/playlists.json");
        Dictionary<string, List<string>> playlists = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        playlists[name].AddRange(songs);
        File.WriteAllText($"{path}/playlists.json", JsonConvert.SerializeObject(playlists));
    }

    public static string Sanitize(string value)
    {
        return value.Replace("/", "").Replace("|", "").Replace("\\", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("#", "").Replace("?", "").Replace("<", "").Replace(">", "").Trim(' ');
    }

    public static int GetAvailableFile(string name = "video")
    {
        int i = 0;
        string path = AppContext.BaseDirectory;
        while (File.Exists($"{path}/tmp/{name}{i}.mp3"))
        {
            i++;
        }
        string dest = $"{path}/tmp/{name}{i}.mp3";
        File.Create(dest).Close();
        return i;
    }

    ///<summary>
    ///Gets all playlist names
    ///</summary>
    public static List<string> GetPlaylist()
    {
        string path = AppContext.BaseDirectory;
        Dictionary<string, List<string>> playlists;
        if (File.Exists($"{path}/playlists.json"))
        {
            string json = File.ReadAllText($"{path}/playlists.json");
            playlists = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            return playlists.Keys.ToList();
        }
        else
        {
            playlists = new Dictionary<string, List<string>>();
            File.WriteAllText($"{path}/playlists.json", JsonConvert.SerializeObject(playlists));
            return new List<string>();
        }
    }

    ///<summary>
    ///Gets all songs in <paramref name="playlist"/>
    ///<br>Returns <returns>empty List<string></returns> if <paramref name="playlist"/> doesn't exist</br>
    ///</summary>
    public static List<string> GetPlaylist(string playlist)
    {
        string path = AppContext.BaseDirectory;
        string json = File.ReadAllText($"{path}/playlists.json");
        Dictionary<string, List<string>> playlists = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        if (playlists.ContainsKey(playlist))
        {
            return playlists[playlist];
        }
        return new List<string>();
    }

    public static void AddSyncTarget(string host)
    {
        string path = AppContext.BaseDirectory;
        if (!File.Exists($"{path}/sync_targets.json"))
        {
            File.WriteAllText($"{path}/sync_targets.json", JsonConvert.SerializeObject(new Dictionary<string, List<string>> { { host, GetSongs() } }));
        }
        else
        {
            string json = File.ReadAllText($"{path}/sync_targets.json");
            Dictionary<string, List<string>> targets = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            targets.Add(host, GetSongs());
            File.WriteAllText($"{path}/sync_targets.json", JsonConvert.SerializeObject(targets));

        }
    }

    public static void AddTrustedHost(string host)
    {
        string path = AppContext.BaseDirectory;
        if (!File.Exists($"{path}/trusted_hosts.json"))
        {
            File.WriteAllText($"{path}/trusted_hosts.json", JsonConvert.SerializeObject(new List<string> { { host } }));
        }
        else
        {
            string json = File.ReadAllText($"{path}/trusted_hosts.json");
            List<string> hosts = JsonConvert.DeserializeObject<List<string>>(json);
            hosts.Add(host);
            File.WriteAllText($"{path}/trusted_hosts.json", JsonConvert.SerializeObject(hosts));

        }
    }

    public static bool GetTrustedHost(string host)
    {
        string path = AppContext.BaseDirectory;
        if (!File.Exists($"{path}/trusted_hosts.json"))
        {
            File.WriteAllText($"{path}/trusted_hosts.json", JsonConvert.SerializeObject(new List<string>()));
        }
        else
        {
            string json = File.ReadAllText($"{path}/trusted_hosts.json");
            List<string> hosts = JsonConvert.DeserializeObject<List<string>>(json);
            if (hosts.Contains(host))
            {
                return true;
            }

        }
        return false;
    }

    public static (bool, List<string>) GetSyncSongs(string host)
    {
        string path = AppContext.BaseDirectory;
        if (!File.Exists($"{path}/sync_targets.json"))
        {
            File.WriteAllText($"{path}/sync_targets.json", JsonConvert.SerializeObject(new Dictionary<string, List<string>>()));
        }
        else
        {
            string json = File.ReadAllText($"{path}/sync_targets.json");
            Dictionary<string, List<string>> targets = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            if (targets.ContainsKey(host))
            {
                return (true, targets[host]);
            }
        }
        return (false, null);
    }
    
    public static void DeletePlaylist(string playlist, string song)
    {
        string json = File.ReadAllText($"{MusicFolder}/playlists.json");
        Dictionary<string, List<string>> playlists = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        if (!playlists.TryGetValue(playlist, out List<string> playlist1)) return;
        playlist1.Remove(song);
        File.WriteAllTextAsync($"{MusicFolder}/playlists.json", JsonConvert.SerializeObject(playlists));
    }

    ///<summary>
    ///Deletes <paramref name="playlist"/>
    ///</summary>
    public static void DeletePlaylist(string playlist)
    {
        string json = File.ReadAllText($"{MusicFolder}/playlists.json");
        Dictionary<string, List<string>> playlists = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        playlists.Remove(playlist);
        File.WriteAllTextAsync($"{MusicFolder}/playlists.json", JsonConvert.SerializeObject(playlists));
    }
    
    public static void Delete(string path)
    {
        if (IsDirectory(path))
        {
            foreach (string playlistName in GetPlaylist())
            {
                foreach(string file in GetSongs(path))
                {
                    DeletePlaylist(playlistName, file);
                }
            }
            Directory.Delete(path, true);
        }
        else
        {
            File.Delete(path);
            foreach (string playlistName in GetPlaylist())
            {
                DeletePlaylist(playlistName, path);
            }
        }
    }
}