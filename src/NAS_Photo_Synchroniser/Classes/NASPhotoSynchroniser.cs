using NAS_Photo_Synchroniser.Classes;
using NAS_Photo_Synchroniser.Interfaces;
using System;

namespace NAS_Photo_Synchroniser
{
    public class NASPhotoSynchroniser
    {
        static void Main(string[] args)
        {
            //Should never happens as the args array is initialiazed to length 0
            if (args is null)
            {
                Console.WriteLine("args parameter is null");
                throw new ArgumentNullException(nameof(args));
            }

            NASPhotoSynchroniserService worker = new();
            worker.DoWork();
        }
    }
}
