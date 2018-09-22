using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace BrackeysBot.Modules
{
    public class ArchiveListener
    {
        private const int PORT = 18692;
        private const string REPO_NAME = "CodeMyst/BrackeysUsefulArticles";

        private readonly TcpListener _githubListener;
        private readonly Thread _listenerThread;

        public ArchiveListener()
        {
            _githubListener = new TcpListener(IPAddress.Any, PORT);
            _listenerThread = new Thread(new ThreadStart(ListenForGitHubEvents));
            _listenerThread.Start();
        }

        private void ListenForGitHubEvents()
        {
            _githubListener.Start();

            while (true)
            {
                TcpClient client = _githubListener.AcceptTcpClient();

                Thread _eventHandlerThread = new Thread(new ParameterizedThreadStart(HandleGitHubEvent));
                _eventHandlerThread.Start(client);
            }
        }

        private void HandleGitHubEvent (object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
            }

            tcpClient.Close();
        }
    }
}
