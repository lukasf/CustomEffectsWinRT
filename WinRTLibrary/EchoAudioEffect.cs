using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using WinRT;

namespace WinRTLibrary
{
    public sealed partial class EchoAudioEffect : IBasicAudioEffect
    {
        #region Fields
        private IPropertySet configuration;
        private float[] echoBuffer;
        private int currentActiveSampleIndex;
        private AudioEncodingProperties currentEncodingProperties;
        #endregion

        #region Properties

        public bool TimeIndependent => true;

        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var supportedEncodingProperties = new List<AudioEncodingProperties>();

                AudioEncodingProperties encodingProps1 = AudioEncodingProperties.CreatePcm(44100, 1, 32);
                encodingProps1.Subtype = MediaEncodingSubtypes.Float;
                supportedEncodingProperties.Add(encodingProps1);

                AudioEncodingProperties encodingProps2 = AudioEncodingProperties.CreatePcm(48000, 1, 32);
                encodingProps2.Subtype = MediaEncodingSubtypes.Float;
                supportedEncodingProperties.Add(encodingProps2);

                return supportedEncodingProperties;

            }
        }

        public bool UseInputFrameForOutput => true;
        public float Mix
        {
            get
            {
                if (configuration != null && configuration.TryGetValue("Mix", out object val))
                {
                    return (float)val;
                }
                return .5f;
            }
        }
        #endregion

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;
            echoBuffer = new float[encodingProperties.SampleRate]; // exactly one second delay
            currentActiveSampleIndex = 0;
        }

        public unsafe void ProcessFrame(ProcessAudioFrameContext context)
        {
            using (AudioBuffer inputBuffer = context.InputFrame.LockBuffer(AudioBufferAccessMode.Read))
            using(AudioBuffer outputBuffer = context.OutputFrame.LockBuffer(AudioBufferAccessMode.Read))
            using (var inputReference = inputBuffer.CreateReference())
            using (var outputReference = outputBuffer.CreateReference())
            {
                inputReference.As<IMemoryBufferByteAccess>().GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);
                outputReference.As<IMemoryBufferByteAccess>().GetBuffer(out byte* outputDataInBytes, out uint outputCapacity);

                float* inputDataInFloat = (float*)inputDataInBytes;
                float* outputDataInFloat = (float*)outputDataInBytes;

                float inputData;
                float echoData;

                // Process audio data
                int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);

                for (int i = 0; i < dataInFloatLength; i++)
                {
                    inputData = inputDataInFloat[i] * (1.0f - this.Mix);
                    echoData = echoBuffer[currentActiveSampleIndex] * this.Mix;
                    outputDataInFloat[i] = inputData + echoData;
                    echoBuffer[currentActiveSampleIndex] = inputDataInFloat[i];
                    currentActiveSampleIndex++;

                    if (currentActiveSampleIndex == echoBuffer.Length)
                    {
                        // Wrap around (after one second of samples)
                        currentActiveSampleIndex = 0;
                    }
                }
            }
        }

        public void DiscardQueuedFrames()
        {
            // Reset contents of the samples buffer
            Array.Clear(echoBuffer, 0, echoBuffer.Length - 1);
            currentActiveSampleIndex = 0;
        }

        public void Close(MediaEffectClosedReason reason)
        {
            echoBuffer = null;
        }
    }
}
