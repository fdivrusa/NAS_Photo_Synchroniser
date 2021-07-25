using NAS_Photo_Synchroniser.Classes;
using System;

namespace NAS_Photo_Synchroniser
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Should never happens as the args array is initialiazed to length 0
            if (args is null)
            {
                Console.WriteLine("args parameter is null");
                throw new ArgumentNullException(nameof(args));
            }

            NASPhotoSynchroniserImplementation worker = new();
            worker.DoWork();
        }
    }
}
