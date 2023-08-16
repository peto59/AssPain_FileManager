using System.Drawing;

namespace AssPain_FileManager
{
    public abstract class MusicBaseClass
    {
        public abstract string Title { get; }
        public abstract Bitmap Image { get; }
        public abstract Bitmap GetImage(bool shouldFallBack = true);
    }

    public abstract class MusicBaseContainer : MusicBaseClass
    {
        public abstract List<Song> Songs { get; }
    }
}