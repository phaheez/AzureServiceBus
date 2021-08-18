using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SampleShared.Services
{
    public interface IBus
    {
        Task SendAsync<T>(string queue, T message);
        Task ReceiveAsync<T>(string queue, Action<T> onMessage);
    }
}
