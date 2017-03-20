using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Media;
using Android.Util;
using Java.Lang;
using PocketSphinx;
using SphinxBase;
using Config = SphinxBase.Config;
using String = System.String;
using Thread = System.Threading.Thread;

namespace XamarinAndroidSphinx
{
    public class SpeechRecognizer : IDisposable
    {
        public event EventHandler<bool> InSpeechChange;
        public event EventHandler<SpeechResultEvent> Result;
        public event EventHandler Timeout;


        protected static String TAG => nameof(SpeechRecognizer);

        private readonly Decoder _decoder;
        private int sampleRate;
        private static float BUFFER_SIZE_SECONDS = 0.4f;
        private int bufferSize;
        private readonly AudioRecord _recorder;

        private Thread _recognizerThread;
        private CancellationTokenSource _interruption;

        public SpeechRecognizer(Config config)
        {
            _decoder = new Decoder(config);
            sampleRate = (int)_decoder.GetConfig().GetFloat("-samprate");
            bufferSize = Java.Lang.Math.Round(sampleRate * BUFFER_SIZE_SECONDS);
            _recorder = new AudioRecord(
                    AudioSource.VoiceRecognition, sampleRate,
                    ChannelIn.Mono,
                    Encoding.Pcm16bit, bufferSize * 2);

            if (_recorder.State == State.Uninitialized)
            {
                _recorder.Release();
                throw new IOException(
                        "Failed to initialize recorder. Microphone might be already in use.");
            }
        }


        public bool StartListening(String searchName, int timeout = -1)
        {
            if (null != _recognizerThread)
                return false;

            _decoder.SetSearch(searchName);

            _interruption = new CancellationTokenSource();

            _recognizerThread = new Thread(async () =>
            {
                await RecognizeAsync(timeout);
            });

            _recognizerThread.Start();

            return true;
        }


        private bool StopRecognizerThread()
        {
            if (null == _recognizerThread)
                return false;

            _interruption.Cancel();

            _recognizerThread = null;
            return true;
        }

        public bool Stop()
        {
            bool result = StopRecognizerThread();
            if (result)
            {
                Log.Debug(TAG, "Stop recognition");
                Hypothesis hypothesis = _decoder.Hyp();
                OnResult(hypothesis, true);
            }
            return result;
        }

        public bool Cancel()
        {
            bool result = StopRecognizerThread();
            if (result)
            {
                Log.Debug(TAG, "Cancel recognition");
            }

            return result;
        }

        private async Task RecognizeAsync(int timeout = -1)
        {
            int remainingSamples;
            int timeoutSamples;
            int NO_TIMEOUT = -1;

            if (timeout != NO_TIMEOUT)
                timeoutSamples = timeout * sampleRate / 1000;
            else
                timeoutSamples = NO_TIMEOUT;
            remainingSamples = timeoutSamples;
            
            _recorder.StartRecording();

            if (_recorder.RecordingState == RecordState.Stopped)
            {
                _recorder.Stop();

                throw new Java.IO.IOException("Failed to start recording. Microphone might be already in use.");
            }

            Log.Debug(TAG, "Starting decoding");


            _decoder.StartUtt();
            short[] buffer = new short[bufferSize];
            bool inSpeech = _decoder.GetInSpeech();

            _recorder.Read(buffer, 0, buffer.Length);



            while (!_interruption.IsCancellationRequested
                    && ((timeoutSamples == NO_TIMEOUT) || (remainingSamples > 0)))
            {
                int nread = _recorder.Read(buffer, 0, buffer.Length);

                if (-1 == nread)
                {
                    throw new RuntimeException("error reading audio buffer");
                }
                else if (nread > 0)
                {
                    var bytes = buffer.Select(s => (byte)s).ToArray();
                    _decoder.ProcessRaw(bytes, nread, false, false);

                    // int max = 0;
                    // for (int i = 0; i < nread; i++) {
                    //     max = Math.max(max, Math.abs(buffer[i]));
                    // }
                    // Log.e("!!!!!!!!", "Level: " + max);

                    if (_decoder.GetInSpeech() != inSpeech)
                    {
                        inSpeech = _decoder.GetInSpeech();
                        OnInSpeechChange(inSpeech);
                    }

                    if (inSpeech)
                        remainingSamples = timeoutSamples;

                    Hypothesis hypothesis = _decoder.Hyp();
                    OnResult(hypothesis, false);
                }

                if (timeoutSamples != NO_TIMEOUT)
                {
                    remainingSamples = remainingSamples - nread;
                }
            }

            _recorder.Stop();
            _decoder.EndUtt();

            // If we met timeout signal that speech ended
            if (timeoutSamples != NO_TIMEOUT && remainingSamples <= 0)
            {
                OnTimeout();
            }
        }

        private void OnResult(Hypothesis hypothesis, bool finalResult)
        {
            Result?.Invoke(this, new SpeechResultEvent(hypothesis, finalResult));
        }

        protected virtual void OnInSpeechChange(bool e)
        {
            InSpeechChange?.Invoke(this, e);
        }

        protected virtual void OnTimeout()
        {
            Timeout?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _recorder.Dispose();
        }

        public String GetSearchName()
        {
            return _decoder.GetSearch();
        }

        public void AddFsgSearch(String searchName, FsgModel fsgModel)
        {
            _decoder.SetFsg(searchName, fsgModel);
        }

        public void AddGrammarSearch(String name, Java.IO.File file)
        {
            Log.Debug(TAG, string.Format("Load JSGF %s", file));
            _decoder.SetJsgfFile(name, file.Path);
        }

        public void AddGrammarSearch(String name, String jsgfString)
        {
            _decoder.SetJsgfString(name, jsgfString);
        }

        public void AddNgramSearch(String name, Java.IO.File file)
        {
            Log.Debug(TAG, string.Format("Load N-gram model %s", file));
            _decoder.SetLmFile(name, file.Path);
        }

        public void AddKeyphraseSearch(String name, String phrase)
        {
            _decoder.SetKeyphrase(name, phrase);
        }

        public void AddKeywordSearch(String name, Java.IO.File file)
        {
            _decoder.SetKws(name, file.Path);
        }
    }
}