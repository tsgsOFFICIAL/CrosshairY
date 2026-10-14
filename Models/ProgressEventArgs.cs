namespace CrosshairY.Managers
{
    /// <summary>
    /// Provides data for events that report download progress.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current progress as a percentage from 0 to 100.
        /// </summary>
        public int Progress { get; }

        /// <summary>
        /// Gets an optional status message describing the current progress.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Gets the number of bytes downloaded so far.
        /// </summary>
        public long BytesDone { get; }

        /// <summary>
        /// Gets the total number of bytes to download, or 0 when unknown.
        /// </summary>
        public long BytesTotal { get; }

        public ProgressEventArgs(int progress, string status = "", long bytesDone = 0, long bytesTotal = 0)
        {
            Progress = progress;
            Status = status;
            BytesDone = bytesDone;
            BytesTotal = bytesTotal;
        }
    }
}
