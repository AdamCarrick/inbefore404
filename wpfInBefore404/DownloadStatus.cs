using System;

namespace wpfInBefore404
{

    public enum DownloadStatus
    {
        CompletedSuccessfully,
        Failed,
        Failed_Retrying,
        Failed_NoMoreRetries,
        Failed_404,
        Cancelled
    }
}

