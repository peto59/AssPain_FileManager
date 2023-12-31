using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using AssPain_FileManager;
using System.Resources;

namespace AssPain_FileManager
{
    public class Song : MusicBaseClass
    {
        //TODO: https://stackoverflow.com/questions/44428405/i-am-using-net-core-with-c-sharp-on-linux-and-library-system-drawing-is-miss
        public List<Artist> Artists { get; } = new List<Artist>();
        public Artist Artist => Artists.Count > 0 ? Artists[0] : new Artist("No Artist", "Default", false);
        public List<Album> Albums { get; } = new List<Album>();
        public List<Album> XmlAlbums => Albums.Where(a => a.Title != "No Album").ToList();
        public Album Album => Albums.Count > 0 ? Albums[0] : new Album("No Album", "Default", false);
        public override string Title { get; }
        public DateTime DateCreated { get; }
        public string Path { get; }
        public bool Initialized { get; private set; } = true;
        public override Bitmap Image => GetImage();

        public void AddArtist(ref List<Artist> artists)
        {
            foreach (Artist artist in artists.Where(artist => !Artists.Contains(artist)))
            {
                Artists.Add(artist);
            }
        }
        public void AddArtist(ref Artist artist)
        {
            if(!Artists.Contains(artist))
                Artists.Add(artist);
        }
        
        public void AddAlbum(ref List<Album> albums)
        {
            foreach (Album album in albums.Where(album => !Albums.Contains(album)))
            {
                Albums.Add(album);
            }
        }
        public void AddAlbum(ref Album album)
        {
            if (!Albums.Contains(album))
                Albums.Add(album);
        }
        
        public void RemoveAlbum(Album album)
        {
            Albums.Remove(album);
        }
        
        public void RemoveAlbum(List<Album> albums)
        {
            albums.ForEach(RemoveAlbum);
        }
        
        public void RemoveArtist(Artist artist)
        {
            Artists.Remove(artist);
        }
        
        public void RemoveArtist(List<Artist> artists)
        {
            artists.ForEach(RemoveArtist);
        }

        ///<summary>
        ///Nukes this object out of existence
        ///</summary>
        public void Nuke()
        {
            //TODO: delete from queue pripadne aj inde
            Albums.ForEach(album =>
            {
                album.RemoveSong(this);
                if (album.Songs.Count == 0)
                {
                    album.Nuke();
                }

            });
            
            Artists.ForEach(artist =>
            {
                artist.RemoveSong(this);
                if (artist.Songs.Count == 0 && artist.Albums.Sum(album => album.Songs.Count) == 0)
                {
                    artist.Nuke();
                }
            });
            
            Initialized = false;
        }

        public void Delete()
        {
            Nuke();
            FileManager.Delete(Path);
        }

        public override Bitmap GetImage(bool shouldFallBack = true)
        {
            if (Path == "Default")
            {
                return new Bitmap(Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("AssPain_FileManager.resources.music_placeholder.png"));
                //TODO: default Image
            }
            Bitmap image = null;
            try
            {
                using TagLib.File tagFile = TagLib.File.Create(Path);
                using MemoryStream ms = new MemoryStream(tagFile.Tag.Pictures[0].Data.Data);
                image = new Bitmap(ms);
            }
            catch (Exception ex)
            {
#if DEBUG
                MyConsole.WriteLine(ex.Message);
                MyConsole.WriteLine($"Doesnt contain image: {Path}");
#endif
            }

            if (image != null) return image;
            if (!shouldFallBack)
            {
                image =  new Bitmap(Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("AssPain_FileManager.resources.music_placeholder.png"));
            }
            foreach (Album album in Albums.Where(album => album.Initialized))
            {
                image = album.GetImage(false);
                if (image != null)
                {
                    break;
                }
            }

            if (image != null) return image;
            foreach (Artist artist in Artists.Where(artist => artist.Initialized))
            {
                image = artist.GetImage(false);
                if (image != null)
                {
                    break;
                }
            }

            return image;
        }

        public Song(List<Artist> artists, List<Album> albums, string title)
        {
            Artists = artists.Distinct().ToList();
            Albums = albums.Distinct().ToList();
            Title = title;
            Initialized = false;
        }
        public Song(List<Artist> artists, string title, DateTime dateCreated, string path, Album album = null)
        {
            Artists = artists.Distinct().ToList();
            if (album != null)
            {
                Albums = new List<Album> {album};
            }
            Title = title;
            DateCreated = dateCreated;
            Path = path;
        }
        public Song(Artist artist, string title, DateTime dateCreated, string path, Album album = null)
        {
            Artists = new List<Artist> {artist};
            if (album != null)
            {
                Albums = new List<Album> {album};
            }
            Title = title;
            DateCreated = dateCreated;
            Path = path;
        }
        
        public Song(List<Artist> artists, string title, DateTime dateCreated, string path, List<Album> albums, bool initialized = true)
        {
            Artists = artists.Distinct().ToList();
            Albums = albums.Distinct().ToList();
            Title = title;
            DateCreated = dateCreated;
            Path = path;
            Initialized = initialized;
        }
        
        public Song(Artist artist, string title, DateTime dateCreated, string path, List<Album> albums)
        {
            Artists = new List<Artist> {artist};
            Albums = albums.Distinct().ToList();
            Title = title;
            DateCreated = dateCreated;
            Path = path;
        }
        
        public Song(string title, DateTime dateCreated, string path, bool initialized = true)
        {
            Title = title;
            DateCreated = dateCreated;
            Path = path;
            Initialized = initialized;
        }

        public Song(Song song, string title)
        {
            Artists = song.Artists;
            Albums = song.Albums;
            Title = title;
            DateCreated = song.DateCreated;
            Path = song.Path;
        }
        
        public Song(Song song, string title, string path)
        {
            Artists = song.Artists;
            Albums = song.Albums;
            Title = title;
            DateCreated = song.DateCreated;
            Path = path;
        }
        
        public override bool Equals(object? obj)
        {
            return obj is Song item && Equals(item);
        }
        
        protected bool Equals(Song other)
        {
            return Equals(Artists, other.Artists) && Equals(Albums, other.Albums) && Title == other.Title && DateCreated.Equals(other.DateCreated) && Path == other.Path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Artists, Albums, Title, DateCreated, Path);
        }

        public override string ToString()
        {
            //$"Song: title> {Name} author> {Artist.Title} album> {Album.Title} dateCreated> {DateCreated} path> {Path}"
            /*string x = $"Song: title> {Name}";
            x = $"{x} author> {Artist.Title}";
            x = $"{x} album> {Album.Title}";
            x = $"{x} dateCreated> {DateCreated}";
            x = $"{x} path> {Path}";
            return x;*/
            return $"Song: title> {Title} author> {Artist.Title} album> {Album.Title} dateCreated> {DateCreated} path> {Path}";
        }
    }
}