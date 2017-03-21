using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamFormsPocketSphinx
{
    public interface ISpeechService
    {
        void StartListening();
        void StopListening();
        Task Setup();
    }
}
