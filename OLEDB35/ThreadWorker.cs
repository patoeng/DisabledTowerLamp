
using System.Diagnostics;

using System.Net;
using System.Net.Sockets;


namespace OLEDB35
{
    public class ThreadWorker
    {
        // This method will be called when the thread is started. 
        public ThreadWorker(int port)
        {
            _udp = new UdpClient(0);
            _ipl = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
           
            
        }
        public void DoWork()
        {
            while (!_shouldStop)
            {
                _dataResult = _udp.Receive(ref _ipl);
                _dataCompleted = true;
                Debug.WriteLine("ttt");

            }
            Debug.WriteLine("eeee");
        }
        public void RequestStop()
        {
            _shouldStop = true;
        }
        // Volatile is used as hint to the compiler that this data 
        // member will be accessed by multiple threads. 
        private volatile bool _shouldStop;
        private volatile byte[] _dataResult={};
        private UdpClient _udp;
        private IPEndPoint _ipl;
        private volatile bool _dataCompleted;

    }
}
