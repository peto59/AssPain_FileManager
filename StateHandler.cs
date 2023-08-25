using System.Net;

namespace AssPain_FileManager
{
    public class StateHandler
    {
        public static Random Rng = new Random();
        public CancellationTokenSource cts = new CancellationTokenSource();
        public List<int> NotificationIDs = new List<int>();
        public bool shuffle = false;
        public Dictionary<long, (int?, int)> SessionIdToPlaylistOrderMapping = new Dictionary<long, (int?, int)>();

        public List<(IPAddress ipAddress, DateTime lastSeen, string hostname)> AvailableHosts =
            new List<(IPAddress ipAddress, DateTime lastSeen, string hostname)>();
        // public bool loopAll = false;
        // public bool loopSingle = false;
        // private List<string> queue = new List<string>();
        private int index = 0;
        public int loopState = 0;
        
        
        //----------Downloader Callback resolution helpers---------
        //internal SongSelectionDialogActions songSelectionDialogAction = SongSelectionDialogActions.None;
        internal readonly AutoResetEvent FileEvent = new AutoResetEvent(true);
        internal readonly AutoResetEvent ResultEvent = new AutoResetEvent(false);
        //---------------------------------------------------------
        public bool ProgTimeState
        {
            get; set;
        }

        public List<Song> Songs = new List<Song>();
        public List<Artist> Artists = new List<Artist>();
        public List<Album> Albums = new List<Album>();





        ///<summary>
        ///Returns path to currently playing song or empty string if no playback is active
        ///</summary>
        // public string NowPlaying
        // {
        //     get {
        //         Console.WriteLine($"queue count: {queue.Count}");
        //         Console.WriteLine($"index: {index}");
        //         if (queue.Count > 0 && queue.Count > index)
        //         {
        //             Console.WriteLine($"my queue: {queue[index]}");
        //             return queue[index];
        //         }
        //         else
        //         {
        //             return string.Empty;
        //         }
        //     }
        // }

        ///<summary>
        ///Moves playback of current song to <paramref name="value"/> time in milliseconds
        ///</summary>
        /*public void SeekTo(int value)
        {
            try
            {
                mediaPlayer.SeekTo(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }*/

        ///<summary>
        ///Returns current loop state
        ///</summary>
        public int LoopState
        {
            get { return loopState; }
        }

        ///<summary>
        ///Return bool based on if shuffling is enabled
        ///</summary>
        public bool IsShuffling
        {
            get { return shuffle; }
        }
        

        // public void setQueue(ref List<string> x)
        // {
        //     queue = x;
        // }

        public void setIndex(ref int x)
        {
            index = x;
        }
    }
}