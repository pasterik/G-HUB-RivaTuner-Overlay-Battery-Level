using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHUB_Overlay
{
    public interface IWebSocketService
    {
        Task Start();
        Task Stop();
        bool IsRunning { get; }
    }

}
